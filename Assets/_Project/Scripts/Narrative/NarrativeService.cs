using System;
using System.Collections.Generic;
using TW08.Core.Services;
using UnityEngine;

namespace TW08.Narrative
{
    [DisallowMultipleComponent]
    public sealed class NarrativeService : MonoBehaviour, IGameService
    {
        private readonly HashSet<string> completedSequences = new(StringComparer.Ordinal);

        public NarrativeSequence Current { get; private set; }
        public event Action<NarrativeSequence> SequenceStarted;
        public event Action<NarrativeSequence> SequenceCompleted;

        public void Initialize(ServiceRegistry services)
        {
        }

        public void Shutdown()
        {
            Current = null;
        }

        public bool TryStart(NarrativeSequence sequence)
        {
            if (sequence == null || Current != null)
            {
                return false;
            }

            if (sequence.PlayOnce && completedSequences.Contains(sequence.SequenceId))
            {
                return false;
            }

            Current = sequence;
            SequenceStarted?.Invoke(sequence);
            return true;
        }

        public void CompleteCurrent()
        {
            if (Current == null)
            {
                return;
            }

            NarrativeSequence completed = Current;
            Current = null;
            completedSequences.Add(completed.SequenceId);
            SequenceCompleted?.Invoke(completed);
        }
    }
}
