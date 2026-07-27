using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW08.Narrative
{
    [Serializable]
    public sealed class NarrativeLine
    {
        [SerializeField] private string speakerId;
        [SerializeField] private string localizationKey;
        [SerializeField] private string voiceEventId;
        [SerializeField, Min(0f)] private float minimumDisplaySeconds = 1f;

        public string SpeakerId => speakerId;
        public string LocalizationKey => localizationKey;
        public string VoiceEventId => voiceEventId;
        public float MinimumDisplaySeconds => minimumDisplaySeconds;
    }

    [CreateAssetMenu(fileName = "NarrativeSequence", menuName = "TW08/Narrative/Sequence")]
    public sealed class NarrativeSequence : ScriptableObject
    {
        [SerializeField] private string sequenceId = "sequence-id";
        [SerializeField] private NarrativeLine[] lines = Array.Empty<NarrativeLine>();
        [SerializeField] private bool playOnce;

        public string SequenceId => sequenceId;
        public IReadOnlyList<NarrativeLine> Lines => lines;
        public bool PlayOnce => playOnce;
    }
}
