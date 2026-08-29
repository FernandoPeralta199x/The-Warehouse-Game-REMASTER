#if UNITY_EDITOR
using System;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Síntese procedural dos efeitos do jogo.
    ///
    /// O projeto é clean-room e não pode usar samples de terceiros, então todo
    /// SFX é gerado aqui. Não é substituto de gravação — é a camada que deixa o
    /// jogo audível e coerente enquanto não há banco de áudio próprio.
    ///
    /// As escolhas seguem os pilares do documento de sound design: clareza
    /// (ataque curto, banda estreita), peso (corpo grave para carga pesada) e
    /// repetição confortável (variação de tom e ruído para o mesmo som não
    /// cansar em centenas de empurrões por sessão).
    ///
    /// Toda geração é determinística: a semente vem do nome do arquivo, então
    /// rodar o pipeline duas vezes produz bytes idênticos e o git não vê ruído.
    /// </summary>
    internal static class TW08AudioSynth
    {
        internal const int SampleRate = 22050;

        /// <summary>Envelope ADSR simplificado, em fração da duração total.</summary>
        internal readonly struct Envelope
        {
            public Envelope(float attack, float decay, float sustain, float release, float decayShape = 1f)
            {
                Attack = Mathf.Max(0.0001f, attack);
                Decay = Mathf.Max(0f, decay);
                Sustain = Mathf.Clamp01(sustain);
                Release = Mathf.Max(0.0001f, release);
                DecayShape = Mathf.Max(0.2f, decayShape);
            }

            public float Attack { get; }
            public float Decay { get; }
            public float Sustain { get; }
            public float Release { get; }

            /// <summary>Curvatura do decaimento. 1 é linear; acima disso cai como corpo físico.</summary>
            public float DecayShape { get; }

            public static Envelope Percussive => new(0.004f, 0.18f, 0f, 0.30f);
            public static Envelope Soft => new(0.06f, 0.10f, 0.75f, 0.35f);
            public static Envelope Sustained => new(0.02f, 0.05f, 0.90f, 0.20f);

            /// <summary>
            /// Impacto: decaimento exponencial ao longo de toda a duração.
            ///
            /// Existe porque <see cref="Percussive"/> combina decay de 0,18 com
            /// sustain 0 — o sinal zera em 18,4% do arquivo e os outros 81,6%
            /// são silêncio gravado. Todo transiente saía cinco vezes mais curto
            /// que o declarado e sem cauda, soando como clique digital em vez de
            /// objeto ressoando.
            /// </summary>
            public static Envelope Impact => new(0.0015f, 0.998f, 0f, 0.0015f, 2.6f);

            /// <summary>
            /// Loop: platô com micro-fades. <see cref="Sustained"/> tem release
            /// em 20% da duração, o que derrubava a ambiência 6,3 dB no fim de
            /// cada volta — uma respiração audível a cada poucos segundos.
            /// </summary>
            public static Envelope Looping => new(0.004f, 0.004f, 1f, 0.004f);

            public float Evaluate(float t)
            {
                if (t < Attack)
                {
                    return t / Attack;
                }

                float afterAttack = t - Attack;
                if (afterAttack < Decay)
                {
                    // DecayShape 1 reproduz exatamente o Lerp anterior, então
                    // nenhum envelope existente muda de comportamento.
                    float k = afterAttack / Decay;
                    return Sustain + (1f - Sustain) * Mathf.Pow(1f - k, DecayShape);
                }

                float releaseStart = 1f - Release;
                if (t >= releaseStart)
                {
                    float k = (t - releaseStart) / Release;
                    return Mathf.Lerp(Sustain, 0f, k);
                }

                return Sustain;
            }
        }

        internal static int SampleCount(float seconds)
        {
            return Mathf.Max(64, Mathf.RoundToInt(seconds * SampleRate));
        }

        internal static System.Random SeededRandom(string name)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in name ?? string.Empty)
                {
                    hash = hash * 31 + c;
                }

                return new System.Random(hash);
            }
        }

        // -------------------------------------------------------- Primitivas --

        /// <summary>Tom com varredura de frequência e envelope.</summary>
        internal static float[] Tone(
            float seconds,
            float startHz,
            float endHz,
            float amplitude,
            Envelope envelope,
            float harmonics = 0f)
        {
            int count = SampleCount(seconds);
            float[] samples = new float[count];
            double phase = 0d;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                float hz = Mathf.Lerp(startHz, endHz, t);
                phase += Math.PI * 2d * hz / SampleRate;

                float value = Mathf.Sin((float)phase);
                if (harmonics > 0f)
                {
                    // Terceiro harmônico dá corpo metálico sem sujar o ataque.
                    value += Mathf.Sin((float)phase * 3f) * harmonics;
                }

                samples[i] = value * amplitude * envelope.Evaluate(t);
            }

            return samples;
        }

        /// <summary>Ruído filtrado — base de impacto, passo e atrito.</summary>
        internal static float[] Noise(
            float seconds,
            float amplitude,
            Envelope envelope,
            string seed,
            float lowPass = 0.35f)
        {
            int count = SampleCount(seconds);
            float[] samples = new float[count];
            System.Random random = SeededRandom(seed);
            float previous = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                float white = (float)random.NextDouble() * 2f - 1f;

                // Passa-baixa de um polo: ruído branco puro soa como estática de
                // rádio, não como madeira ou metal.
                previous += (white - previous) * lowPass;
                samples[i] = previous * amplitude * envelope.Evaluate(t);
            }

            return samples;
        }

        /// <summary>Onda quadrada suavizada — terminais, alarmes, bipes de UI.</summary>
        internal static float[] Square(
            float seconds, float hz, float amplitude, Envelope envelope, float duty = 0.5f)
        {
            int count = SampleCount(seconds);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                float phase = (i * hz / SampleRate) % 1f;
                float value = phase < duty ? 1f : -1f;

                // Suaviza a borda: quadrada crua estoura em alto-falante pequeno.
                samples[i] = value * 0.7f * amplitude * envelope.Evaluate(t);
            }

            return samples;
        }

        // ------------------------------------------------------ Composição --

        internal static float[] Mix(params float[][] layers)
        {
            int length = 0;
            foreach (float[] layer in layers)
            {
                if (layer != null && layer.Length > length)
                {
                    length = layer.Length;
                }
            }

            float[] result = new float[length];
            foreach (float[] layer in layers)
            {
                if (layer == null)
                {
                    continue;
                }

                for (int i = 0; i < layer.Length; i++)
                {
                    result[i] += layer[i];
                }
            }

            return Normalize(result, 0.92f);
        }

        /// <summary>Concatena camadas no tempo, com deslocamento em segundos.</summary>
        internal static float[] Sequence(params (float atSeconds, float[] layer)[] parts)
        {
            int length = 0;
            foreach ((float at, float[] layer) in parts)
            {
                if (layer == null)
                {
                    continue;
                }

                length = Mathf.Max(length, Mathf.RoundToInt(at * SampleRate) + layer.Length);
            }

            float[] result = new float[Mathf.Max(64, length)];
            foreach ((float at, float[] layer) in parts)
            {
                if (layer == null)
                {
                    continue;
                }

                int offset = Mathf.RoundToInt(at * SampleRate);
                for (int i = 0; i < layer.Length && offset + i < result.Length; i++)
                {
                    result[offset + i] += layer[i];
                }
            }

            return Normalize(result, 0.92f);
        }

        /// <summary>
        /// Ajusta o pico para <paramref name="peak"/>. Sem isso, somar camadas
        /// estoura em 1.0 e o WAV grava distorcido.
        /// </summary>
        internal static float[] Normalize(float[] samples, float peak)
        {
            float max = 0f;
            foreach (float sample in samples)
            {
                max = Mathf.Max(max, Mathf.Abs(sample));
            }

            if (max <= 0.0001f)
            {
                return samples;
            }

            float scale = peak / max;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = Mathf.Clamp(samples[i] * scale, -1f, 1f);
            }

            return samples;
        }

        /// <summary>
        /// Costura o fim no começo para um loop sem clique. Necessário em
        /// ambiência e motor, que tocam continuamente.
        /// </summary>
        internal static float[] MakeSeamless(float[] samples, float crossfadeSeconds = 0.12f)
        {
            int fade = Mathf.Min(SampleCount(crossfadeSeconds), samples.Length / 3);
            if (fade <= 1)
            {
                return samples;
            }

            int length = samples.Length - fade;
            float[] result = new float[length];
            Array.Copy(samples, result, length);

            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                result[i] = Mathf.Lerp(samples[length + i], samples[i], k);
            }

            return result;
        }
    }
}
#endif
