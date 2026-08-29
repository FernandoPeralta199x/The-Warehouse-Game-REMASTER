#if UNITY_EDITOR
using System.Collections.Generic;
using TW08.Audio;
using UnityEditor;
using UnityEngine;
using Env = TW08.Editor.TW08AudioSynth.Envelope;

namespace TW08.Editor
{
    /// <summary>
    /// Banco de efeitos do jogo: gera os WAVs e os <see cref="AudioEvent"/>
    /// correspondentes.
    ///
    /// Cobre as prioridades P0 e P1 da lista de sound design. Cada som recebe
    /// três variações quando é de repetição alta (passo, empurrão, impacto):
    /// o pilar de "repetição confortável" do documento existe porque o jogador
    /// ouve esses sons centenas de vezes por sessão, e uma amostra única vira
    /// tique nervoso em dez minutos.
    ///
    /// A nomeação segue a convenção do documento
    /// (<c>categoria_acao_material_variacao.wav</c>).
    /// </summary>
    internal static class TW08SoundBank
    {
        internal const string BankRoot = "Assets/_Project/Audio/GeneratedStarter/Bank";

        /// <summary>Todos os eventos gerados, indexados pelo id do catálogo.</summary>
        internal static Dictionary<string, AudioEvent> EnsureAll()
        {
            TW08ProductionSceneUtility.EnsureFolder(BankRoot);
            Dictionary<string, AudioEvent> events = new();

            // ------------------------------------------------------- P0 --

            // Passo: transiente curto de sola em concreto. Três variações.
            events["puzzleStepVariants"] = MultiEvent(
                "Puzzle_Step_Var",
                new[] { "step_concrete_01", "step_concrete_02", "step_concrete_03" },
                seed => TW08AudioSynth.Mix(
                    TW08AudioSynth.Noise(0.075f, 0.55f, Env.Percussive, seed, 0.22f),
                    TW08AudioSynth.Tone(0.06f, 132f, 96f, 0.30f, Env.Percussive)),
                volume: 0.52f, pitchMin: 0.92f, pitchMax: 1.08f);

            // Empurrão: atrito longo de madeira arrastando.
            events["cratePushWood"] = MultiEvent(
                "Crate_Push_Wood",
                new[] { "crate_push_wood_01", "crate_push_wood_02", "crate_push_wood_03" },
                seed => TW08AudioSynth.Mix(
                    TW08AudioSynth.Noise(0.20f, 0.62f, new Env(0.02f, 0.10f, 0.60f, 0.30f), seed, 0.14f),
                    TW08AudioSynth.Tone(0.20f, 92f, 74f, 0.34f, new Env(0.03f, 0.12f, 0.55f, 0.30f))),
                volume: 0.78f, pitchMin: 0.93f, pitchMax: 1.05f);

            // Carga pesada: mesmo gesto, corpo mais grave e mais longo.
            events["cratePushHeavy"] = MultiEvent(
                "Crate_Push_Heavy",
                new[] { "crate_push_heavy_01", "crate_push_heavy_02" },
                seed => TW08AudioSynth.Mix(
                    TW08AudioSynth.Noise(0.30f, 0.55f, new Env(0.03f, 0.14f, 0.62f, 0.32f), seed, 0.10f),
                    TW08AudioSynth.Tone(0.30f, 58f, 44f, 0.52f, new Env(0.04f, 0.16f, 0.60f, 0.30f)),
                    TW08AudioSynth.Tone(0.30f, 116f, 88f, 0.16f, Env.Soft)),
                volume: 0.86f, pitchMin: 0.95f, pitchMax: 1.03f);

            // Impacto: caixa batendo em parede ou outra caixa.
            events["crateHit"] = MultiEvent(
                "Crate_Hit_Wood",
                new[] { "crate_hit_wood_01", "crate_hit_wood_02", "crate_hit_wood_03" },
                seed => TW08AudioSynth.Mix(
                    TW08AudioSynth.Noise(0.13f, 0.70f, Env.Percussive, seed, 0.30f),
                    TW08AudioSynth.Tone(0.13f, 190f, 70f, 0.55f, Env.Percussive, harmonics: 0.20f)),
                volume: 0.80f, pitchMin: 0.90f, pitchMax: 1.10f);

            // Carga no alvo: confirmação clara e curta, sem virar fanfarra —
            // acontece muitas vezes por fase.
            events["crateOnGoal"] = SingleEvent(
                "Crate_On_Goal",
                "crate_place_goal_01",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Tone(0.09f, 520f, 520f, 0.40f, Env.Percussive)),
                    (0.07f, TW08AudioSynth.Tone(0.16f, 784f, 784f, 0.34f, Env.Percussive))),
                volume: 0.72f, pitchMin: 0.99f, pitchMax: 1.01f);

            // Porta industrial: servo grave subindo/descendo.
            events["doorOpen"] = SingleEvent(
                "Door_Open_Heavy",
                "door_open_heavy_01",
                TW08AudioSynth.Mix(
                    TW08AudioSynth.Tone(0.55f, 70f, 130f, 0.44f, Env.Soft, harmonics: 0.12f),
                    TW08AudioSynth.Noise(0.55f, 0.26f, Env.Soft, "door_open", 0.08f)),
                volume: 0.74f, pitchMin: 0.98f, pitchMax: 1.02f);

            events["doorClose"] = SingleEvent(
                "Door_Close_Heavy",
                "door_close_heavy_01",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Mix(
                        TW08AudioSynth.Tone(0.42f, 130f, 62f, 0.40f, Env.Soft, harmonics: 0.10f),
                        TW08AudioSynth.Noise(0.42f, 0.22f, Env.Soft, "door_close", 0.08f))),
                    (0.40f, TW08AudioSynth.Mix(
                        TW08AudioSynth.Noise(0.14f, 0.62f, Env.Percussive, "door_clunk", 0.30f),
                        TW08AudioSynth.Tone(0.14f, 96f, 52f, 0.50f, Env.Percussive)))),
                volume: 0.78f, pitchMin: 0.98f, pitchMax: 1.02f);

            // Sensor: bipe eletrônico limpo. Ligar sobe, desligar desce.
            events["sensorOn"] = SingleEvent(
                "Sensor_On",
                "sensor_activate_01",
                TW08AudioSynth.Square(0.10f, 1180f, 0.30f, Env.Percussive, 0.35f),
                volume: 0.56f, pitchMin: 1f, pitchMax: 1f);

            events["sensorOff"] = SingleEvent(
                "Sensor_Off",
                "sensor_deactivate_01",
                TW08AudioSynth.Square(0.12f, 620f, 0.28f, Env.Percussive, 0.35f),
                volume: 0.52f, pitchMin: 1f, pitchMax: 1f);

            events["uiBack"] = SingleEvent(
                "UI_Back",
                "ui_back_01",
                TW08AudioSynth.Tone(0.09f, 520f, 340f, 0.30f, Env.Percussive),
                volume: 0.60f, pitchMin: 1f, pitchMax: 1f);

            events["uiFocus"] = SingleEvent(
                "UI_Focus",
                "ui_focus_01",
                TW08AudioSynth.Tone(0.045f, 880f, 940f, 0.20f, Env.Percussive),
                volume: 0.34f, pitchMin: 0.98f, pitchMax: 1.03f);

            events["uiDenied"] = SingleEvent(
                "UI_Denied",
                "ui_denied_01",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Square(0.07f, 220f, 0.30f, Env.Percussive, 0.5f)),
                    (0.08f, TW08AudioSynth.Square(0.11f, 165f, 0.30f, Env.Percussive, 0.5f))),
                volume: 0.62f, pitchMin: 1f, pitchMax: 1f);

            events["terminalBoot"] = SingleEvent(
                "Terminal_Boot",
                "terminal_boot_one_shot",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Square(0.06f, 440f, 0.22f, Env.Percussive, 0.3f)),
                    (0.07f, TW08AudioSynth.Square(0.06f, 660f, 0.22f, Env.Percussive, 0.3f)),
                    (0.14f, TW08AudioSynth.Square(0.06f, 880f, 0.22f, Env.Percussive, 0.3f)),
                    (0.21f, TW08AudioSynth.Tone(0.30f, 1320f, 1320f, 0.20f, Env.Soft))),
                volume: 0.58f, pitchMin: 1f, pitchMax: 1f);

            // Empilhadeira: motor em loop, ré e colisão.
            events["forkliftEngine"] = LoopEvent(
                "Forklift_Engine_Idle",
                "forklift_engine_idle_loop",
                TW08AudioSynth.MakeSeamless(TW08AudioSynth.Mix(
                    TW08AudioSynth.Tone(2.4f, 62f, 62f, 0.42f, Env.Sustained, harmonics: 0.28f),
                    TW08AudioSynth.Tone(2.4f, 93f, 93f, 0.16f, Env.Sustained),
                    TW08AudioSynth.Noise(2.4f, 0.14f, Env.Sustained, "engine", 0.06f))),
                volume: 0.44f);

            events["forkliftReverse"] = LoopEvent(
                "Forklift_Reverse",
                "forklift_reverse_beep_loop",
                TW08AudioSynth.MakeSeamless(TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Square(0.18f, 990f, 0.30f, Env.Percussive, 0.5f)),
                    (0.55f, TW08AudioSynth.Square(0.18f, 990f, 0.30f, Env.Percussive, 0.5f))),
                    0.05f),
                volume: 0.50f);

            events["forkliftImpact"] = MultiEvent(
                "Forklift_Impact",
                new[] { "forklift_impact_01", "forklift_impact_02" },
                seed => TW08AudioSynth.Mix(
                    TW08AudioSynth.Noise(0.22f, 0.72f, Env.Percussive, seed, 0.36f),
                    TW08AudioSynth.Tone(0.22f, 240f, 70f, 0.58f, Env.Percussive, harmonics: 0.34f)),
                volume: 0.84f, pitchMin: 0.90f, pitchMax: 1.08f);

            // ------------------------------------------------------- P1 --

            // Ferramentas da Oficina N-8: cada uma tem assinatura própria para o
            // jogador reconhecer o que acionou sem olhar a barra.
            events["toolRewind"] = SingleEvent(
                "Tool_Rewind",
                "powerup_rewind_activate_01",
                TW08AudioSynth.Tone(0.42f, 880f, 220f, 0.34f, Env.Soft, harmonics: 0.10f),
                volume: 0.66f, pitchMin: 1f, pitchMax: 1f);

            events["toolScanner"] = SingleEvent(
                "Tool_Scanner",
                "powerup_scanner_activate_01",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Square(0.05f, 1320f, 0.22f, Env.Percussive, 0.25f)),
                    (0.10f, TW08AudioSynth.Square(0.05f, 1320f, 0.22f, Env.Percussive, 0.25f)),
                    (0.20f, TW08AudioSynth.Tone(0.34f, 1760f, 1320f, 0.24f, Env.Soft))),
                volume: 0.62f, pitchMin: 1f, pitchMax: 1f);

            events["toolAssistant"] = SingleEvent(
                "Tool_Assistant",
                "powerup_assistant_activate_01",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Tone(0.16f, 587f, 587f, 0.28f, Env.Percussive)),
                    (0.14f, TW08AudioSynth.Tone(0.28f, 784f, 784f, 0.26f, Env.Soft))),
                volume: 0.60f, pitchMin: 1f, pitchMax: 1f);

            events["toolMarker"] = SingleEvent(
                "Tool_Marker",
                "powerup_marker_activate_01",
                TW08AudioSynth.Tone(0.24f, 440f, 1100f, 0.28f, Env.Percussive),
                volume: 0.58f, pitchMin: 1f, pitchMax: 1f);

            // Medalhas: três degraus de celebração, do reconhecimento ao brilho.
            events["medalBronze"] = SingleEvent(
                "Medal_Bronze",
                "victory_bronze_stinger",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Tone(0.20f, 392f, 392f, 0.34f, Env.Percussive)),
                    (0.16f, TW08AudioSynth.Tone(0.34f, 523f, 523f, 0.32f, Env.Soft))),
                volume: 0.74f, pitchMin: 1f, pitchMax: 1f);

            events["medalGold"] = SingleEvent(
                "Medal_Gold",
                "victory_gold_stinger",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Tone(0.16f, 523f, 523f, 0.34f, Env.Percussive)),
                    (0.14f, TW08AudioSynth.Tone(0.16f, 659f, 659f, 0.34f, Env.Percussive)),
                    (0.28f, TW08AudioSynth.Tone(0.44f, 784f, 784f, 0.36f, Env.Soft, harmonics: 0.12f))),
                volume: 0.80f, pitchMin: 1f, pitchMax: 1f);

            events["medalPlatinum"] = SingleEvent(
                "Medal_Platinum",
                "victory_platinum_stinger",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Tone(0.14f, 523f, 523f, 0.32f, Env.Percussive)),
                    (0.12f, TW08AudioSynth.Tone(0.14f, 659f, 659f, 0.32f, Env.Percussive)),
                    (0.24f, TW08AudioSynth.Tone(0.14f, 784f, 784f, 0.32f, Env.Percussive)),
                    (0.36f, TW08AudioSynth.Tone(0.62f, 1046f, 1046f, 0.38f, Env.Soft, harmonics: 0.18f))),
                volume: 0.86f, pitchMin: 1f, pitchMax: 1f);

            // Loja: compra confirmada e créditos entrando.
            events["shopPurchase"] = SingleEvent(
                "Shop_Purchase",
                "shop_purchase_01",
                TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Square(0.05f, 880f, 0.24f, Env.Percussive, 0.3f)),
                    (0.06f, TW08AudioSynth.Tone(0.30f, 1318f, 1318f, 0.28f, Env.Soft))),
                volume: 0.68f, pitchMin: 1f, pitchMax: 1f);

            events["creditsTick"] = SingleEvent(
                "Credits_Tick",
                "ui_credit_tick_01",
                TW08AudioSynth.Tone(0.035f, 1480f, 1480f, 0.16f, Env.Percussive),
                volume: 0.30f, pitchMin: 0.96f, pitchMax: 1.06f);

            // Alarme de lockdown e ambiências de setor.
            events["lockdownAlarm"] = LoopEvent(
                "Lockdown_Alarm",
                "alarm_lockdown_loop",
                TW08AudioSynth.MakeSeamless(TW08AudioSynth.Sequence(
                    (0f, TW08AudioSynth.Tone(0.55f, 440f, 620f, 0.34f, Env.Soft)),
                    (0.60f, TW08AudioSynth.Tone(0.55f, 620f, 440f, 0.34f, Env.Soft))),
                    0.08f),
                volume: 0.44f);

            events["conveyorLoop"] = LoopEvent(
                "Conveyor_Belt",
                "conveyor_belt_loop",
                TW08AudioSynth.MakeSeamless(TW08AudioSynth.Mix(
                    TW08AudioSynth.Noise(2.0f, 0.30f, Env.Sustained, "conveyor", 0.05f),
                    TW08AudioSynth.Tone(2.0f, 88f, 88f, 0.14f, Env.Sustained))),
                volume: 0.36f);

            events["warehouseAmbience"] = LoopEvent(
                "Ambience_Warehouse",
                "sector_warehouse_ambience_loop",
                TW08AudioSynth.MakeSeamless(TW08AudioSynth.Mix(
                    TW08AudioSynth.Noise(4.0f, 0.16f, Env.Sustained, "ambience", 0.03f),
                    TW08AudioSynth.Tone(4.0f, 48f, 48f, 0.10f, Env.Sustained))),
                volume: 0.26f);

            events["freezerAmbience"] = LoopEvent(
                "Ambience_Freezer",
                "sector03_freezer_ambience_loop",
                TW08AudioSynth.MakeSeamless(TW08AudioSynth.Mix(
                    TW08AudioSynth.Noise(4.0f, 0.22f, Env.Sustained, "freezer", 0.10f),
                    TW08AudioSynth.Tone(4.0f, 116f, 116f, 0.07f, Env.Sustained))),
                volume: 0.30f);

            // Voz de narrativa: o documento pede um marcador de fala por
            // personagem, não voz gravada. Cada um tem uma altura própria.
            events["voiceJohn"] = SingleEvent(
                "Voice_John",
                "voice_john_blip_01",
                TW08AudioSynth.Tone(0.045f, 190f, 175f, 0.22f, Env.Percussive),
                volume: 0.32f, pitchMin: 0.96f, pitchMax: 1.05f);

            events["voiceDuda"] = SingleEvent(
                "Voice_Duda",
                "voice_duda_blip_01",
                TW08AudioSynth.Tone(0.042f, 330f, 310f, 0.20f, Env.Percussive),
                volume: 0.32f, pitchMin: 0.96f, pitchMax: 1.05f);

            events["voiceRobert"] = SingleEvent(
                "Voice_Robert",
                "voice_robert_blip_01",
                TW08AudioSynth.Tone(0.05f, 140f, 128f, 0.24f, Env.Percussive),
                volume: 0.34f, pitchMin: 0.96f, pitchMax: 1.05f);

            AssetDatabase.SaveAssets();
            return events;
        }

        // ------------------------------------------------------- Helpers --

        private static AudioEvent SingleEvent(
            string id, string fileName, float[] samples, float volume, float pitchMin, float pitchMax)
        {
            AudioClip clip = TW08StarterAudioSetup.EnsureImportedClip(
                $"{BankRoot}/{fileName}.wav", samples);
            return TW08StarterAudioSetup.EnsureEvent(id, clip, volume, pitchMin, pitchMax);
        }

        private static AudioEvent LoopEvent(string id, string fileName, float[] samples, float volume)
        {
            AudioClip clip = TW08StarterAudioSetup.EnsureImportedClip(
                $"{BankRoot}/{fileName}.wav", samples);
            AudioEvent audioEvent = TW08StarterAudioSetup.EnsureEvent(id, clip, volume, 1f, 1f);

            SerializedObject serialized = new(audioEvent);
            serialized.FindProperty("loop").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(audioEvent);
            return audioEvent;
        }

        /// <summary>
        /// Evento com várias amostras. O <see cref="AudioEvent"/> sorteia uma a
        /// cada disparo, que é o que evita o efeito de tique em sons de repetição alta.
        /// </summary>
        private static AudioEvent MultiEvent(
            string id,
            string[] fileNames,
            System.Func<string, float[]> generator,
            float volume,
            float pitchMin,
            float pitchMax)
        {
            List<AudioClip> clips = new();
            foreach (string fileName in fileNames)
            {
                clips.Add(TW08StarterAudioSetup.EnsureImportedClip(
                    $"{BankRoot}/{fileName}.wav", generator(fileName)));
            }

            AudioEvent audioEvent = TW08StarterAudioSetup.EnsureEvent(
                id, clips.Count > 0 ? clips[0] : null, volume, pitchMin, pitchMax);

            SerializedObject serialized = new(audioEvent);
            SerializedProperty list = serialized.FindProperty("clips");
            list.arraySize = clips.Count;
            for (int i = 0; i < clips.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(audioEvent);
            return audioEvent;
        }
    }
}
#endif
