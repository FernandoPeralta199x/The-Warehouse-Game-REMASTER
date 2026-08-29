using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW08.Narrative
{
    /// <summary>
    /// Tom da fala. Não muda o texto: muda ritmo da máquina de escrever e a cor
    /// do corpo do diálogo. Voz da automação e memória gravada da Duda precisam
    /// soar diferentes de uma conversa de rádio.
    /// </summary>
    public enum NarrativeTone
    {
        Neutro = 0,
        Seco = 1,
        Tenso = 2,
        Sistema = 3,
        Memoria = 4
    }

    [Serializable]
    public sealed class NarrativeLine
    {
        [SerializeField] private string speakerId = "john";
        [SerializeField, TextArea(2, 6)] private string text = string.Empty;
        [SerializeField] private NarrativeTone tone = NarrativeTone.Neutro;
        [SerializeField, Min(0f)] private float charactersPerSecond;
        [SerializeField, Min(0f)] private float minimumDisplaySeconds = 0.45f;
        [SerializeField] private string voiceEventId = string.Empty;

        // Unity precisa do construtor sem parâmetros para desserializar o array.
        public NarrativeLine()
        {
        }

        public NarrativeLine(
            string speakerId,
            string text,
            NarrativeTone tone = NarrativeTone.Neutro,
            float charactersPerSecond = 0f,
            float minimumDisplaySeconds = 0.45f,
            string voiceEventId = null)
        {
            this.speakerId = speakerId;
            this.text = text;
            this.tone = tone;
            this.charactersPerSecond = Mathf.Max(0f, charactersPerSecond);
            this.minimumDisplaySeconds = Mathf.Max(0f, minimumDisplaySeconds);
            this.voiceEventId = voiceEventId ?? string.Empty;
        }

        public string SpeakerId => string.IsNullOrWhiteSpace(speakerId) ? string.Empty : speakerId.Trim();
        public string Text => text ?? string.Empty;
        public NarrativeTone Tone => tone;

        /// <summary>Zero significa "use o ritmo padrão da sequência".</summary>
        public float CharactersPerSecond => charactersPerSecond;

        /// <summary>Janela em que o avanço fica bloqueado, para a fala não sumir no clique seguinte.</summary>
        public float MinimumDisplaySeconds => minimumDisplaySeconds;

        public string VoiceEventId => voiceEventId ?? string.Empty;
        public bool HasText => !string.IsNullOrWhiteSpace(text);
    }

    [CreateAssetMenu(fileName = "NarrativeSequence", menuName = "TW08/Narrative/Sequence")]
    public sealed class NarrativeSequence : ScriptableObject
    {
        [Header("Identidade")]
        [SerializeField] private string sequenceId = "sequence-id";
        [SerializeField] private string title = "Sem título";

        [Header("Disparo")]
        [SerializeField] private NarrativeTriggerKind trigger = NarrativeTriggerKind.Manual;
        [Tooltip("Vazio = qualquer setor. Preenchido = só naquele setor (S01..S06).")]
        [SerializeField] private string sectorId = string.Empty;
        [Tooltip("Vazio = qualquer fase. Preenchido = só naquela fase.")]
        [SerializeField] private string levelId = string.Empty;
        [Tooltip("Desempata quando duas sequências casam com o mesmo momento. Maior ganha.")]
        [SerializeField] private int priority;
        [SerializeField] private bool playOnce = true;

        [Header("Falas")]
        [SerializeField, Min(1f)] private float defaultCharactersPerSecond = 38f;
        [SerializeField] private NarrativeLine[] lines = Array.Empty<NarrativeLine>();

        public string SequenceId => string.IsNullOrWhiteSpace(sequenceId) ? name : sequenceId.Trim();
        public string Title => string.IsNullOrWhiteSpace(title) ? name : title;
        public NarrativeTriggerKind Trigger => trigger;
        public string SectorId => sectorId ?? string.Empty;
        public string LevelId => levelId ?? string.Empty;
        public int Priority => priority;
        public bool PlayOnce => playOnce;
        public float DefaultCharactersPerSecond => Mathf.Max(1f, defaultCharactersPerSecond);
        public IReadOnlyList<NarrativeLine> Lines => lines ?? Array.Empty<NarrativeLine>();

        public bool Matches(in NarrativeContext context)
        {
            return NarrativeMatching.Matches(trigger, sectorId, levelId, context);
        }

        public int Specificity => NarrativeMatching.Specificity(sectorId, levelId);

        public float ResolveSpeed(NarrativeLine line)
        {
            if (line != null && line.CharactersPerSecond > 0f)
            {
                return line.CharactersPerSecond;
            }

            return DefaultCharactersPerSecond;
        }

        public NarrativePlayback CreatePlayback()
        {
            return new NarrativePlayback(Lines);
        }

        private void OnValidate()
        {
            sequenceId = string.IsNullOrWhiteSpace(sequenceId) ? name.ToLowerInvariant() : sequenceId.Trim();
            defaultCharactersPerSecond = Mathf.Max(1f, defaultCharactersPerSecond);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Escrita de conteúdo pelo setup de editor e pelos testes.
        ///
        /// API tipada em vez de SerializedProperty por nome: um nome de campo
        /// errado em SerializedProperty só falha em runtime, e o roteiro é escrito
        /// sem o compilador do Unity à mão. Aqui o erro aparece na compilação.
        /// </summary>
        public void ConfigureAuthoring(
            string id,
            string sequenceTitle,
            NarrativeTriggerKind triggerKind,
            string sector,
            string level,
            bool once,
            int order,
            float defaultSpeed,
            IReadOnlyList<NarrativeLine> content)
        {
            sequenceId = id;
            title = sequenceTitle;
            trigger = triggerKind;
            sectorId = sector ?? string.Empty;
            levelId = level ?? string.Empty;
            playOnce = once;
            priority = order;
            defaultCharactersPerSecond = Mathf.Max(1f, defaultSpeed);

            if (content == null)
            {
                lines = Array.Empty<NarrativeLine>();
            }
            else
            {
                lines = new NarrativeLine[content.Count];
                for (int i = 0; i < content.Count; i++)
                {
                    lines[i] = content[i];
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
