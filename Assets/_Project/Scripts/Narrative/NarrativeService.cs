using System;
using System.Collections.Generic;
using TW08.Core.Services;
using UnityEngine;

namespace TW08.Narrative
{
    /// <summary>
    /// Estado da narrativa em execução: qual sequência está no ar, em que fala
    /// ela está e o que ainda vai entrar depois dela.
    ///
    /// A fila existe porque a abertura e a entrada do Setor 01 acontecem na mesma
    /// fase: sem ela, uma das duas seria descartada em silêncio.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NarrativeService : MonoBehaviour, IGameService
    {
        private readonly HashSet<string> sessionCompleted = new(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<NarrativeSequence> pending = new();

        public NarrativeSequence Current { get; private set; }
        public NarrativePlayback Playback { get; private set; }
        public bool IsPlaying => Current != null;
        public bool HasPending => pending.Count > 0;
        public int PendingCount => pending.Count;

        public event Action<NarrativeSequence> SequenceStarted;
        public event Action<NarrativeSequence> SequenceCompleted;
        public event Action<NarrativeLine> LineChanged;

        public void Initialize(ServiceRegistry services)
        {
        }

        public void Shutdown()
        {
            pending.Clear();
            Current = null;
            Playback = null;
        }

        public bool HasPlayed(NarrativeSequence sequence)
        {
            if (sequence == null)
            {
                return false;
            }

            return sessionCompleted.Contains(sequence.SequenceId)
                || NarrativeProgressStore.HasPlayed(sequence.SequenceId);
        }

        /// <summary>Sequência com conteúdo e que ainda pode ser exibida.</summary>
        public bool IsEligible(NarrativeSequence sequence)
        {
            if (sequence == null || sequence.Lines.Count == 0)
            {
                return false;
            }

            return !sequence.PlayOnce || !HasPlayed(sequence);
        }

        /// <summary>Entra na fila. Não começa nada sozinho — chame <see cref="PlayQueued"/>.</summary>
        public bool Enqueue(NarrativeSequence sequence)
        {
            if (!IsEligible(sequence) || ReferenceEquals(sequence, Current) || pending.Contains(sequence))
            {
                return false;
            }

            pending.Enqueue(sequence);
            return true;
        }

        /// <summary>Puxa a próxima sequência elegível da fila. Ignora as que ficaram obsoletas.</summary>
        public bool PlayQueued()
        {
            if (Current != null)
            {
                return false;
            }

            while (pending.Count > 0)
            {
                NarrativeSequence next = pending.Dequeue();
                if (StartInternal(next))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Começa agora. Falha se já houver uma cutscene no ar.</summary>
        public bool TryStart(NarrativeSequence sequence)
        {
            return Current == null && StartInternal(sequence);
        }

        /// <summary>Avança uma fala. Ao passar da última, encerra a sequência.</summary>
        public bool Advance()
        {
            if (Current == null || Playback == null)
            {
                return false;
            }

            if (Playback.Advance())
            {
                LineChanged?.Invoke(Playback.Current);
                return true;
            }

            CompleteCurrent();
            return false;
        }

        public void CompleteCurrent()
        {
            if (Current == null)
            {
                return;
            }

            NarrativeSequence completed = Current;
            Current = null;
            Playback = null;

            MarkSeen(completed);
            SequenceCompleted?.Invoke(completed);
            PlayQueued();
        }

        /// <summary>
        /// Pula a cutscene inteira, fila incluída. O que foi pulado conta como
        /// visto: quem apertou ESC não quer reencontrar a mesma cena na próxima fase.
        /// </summary>
        public void SkipAll()
        {
            while (pending.Count > 0)
            {
                MarkSeen(pending.Dequeue());
            }

            CompleteCurrent();
        }

        private void MarkSeen(NarrativeSequence sequence)
        {
            if (sequence == null)
            {
                return;
            }

            sessionCompleted.Add(sequence.SequenceId);
            if (sequence.PlayOnce)
            {
                NarrativeProgressStore.MarkPlayed(sequence.SequenceId);
            }
        }

        private bool StartInternal(NarrativeSequence sequence)
        {
            if (!IsEligible(sequence))
            {
                return false;
            }

            NarrativePlayback playback = sequence.CreatePlayback();
            if (playback.IsFinished)
            {
                return false;
            }

            Current = sequence;
            Playback = playback;
            SequenceStarted?.Invoke(sequence);
            LineChanged?.Invoke(playback.Current);
            return true;
        }
    }
}
