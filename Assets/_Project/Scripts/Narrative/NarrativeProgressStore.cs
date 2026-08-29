using System.Collections.Generic;
using UnityEngine;

namespace TW08.Narrative
{
    /// <summary>
    /// Registro do que o jogador já viu.
    ///
    /// Fica em PlayerPrefs, e não em SaveGameData, porque narrativa não pode
    /// forçar migração de save nem invalidar um perfil existente. O prefixo segue
    /// o mesmo padrão de PuzzleProgressStore.
    /// </summary>
    public static class NarrativeProgressStore
    {
        private const string Prefix = "tw08.narrative.";

        public static bool HasPlayed(string sequenceId)
        {
            return PlayerPrefs.GetInt(Key(sequenceId), 0) == 1;
        }

        public static void MarkPlayed(string sequenceId)
        {
            if (string.IsNullOrWhiteSpace(sequenceId))
            {
                return;
            }

            PlayerPrefs.SetInt(Key(sequenceId), 1);
            PlayerPrefs.Save();
        }

        public static void Clear(string sequenceId)
        {
            if (string.IsNullOrWhiteSpace(sequenceId))
            {
                return;
            }

            PlayerPrefs.DeleteKey(Key(sequenceId));
            PlayerPrefs.Save();
        }

        /// <summary>Reseta a campanha narrativa inteira — usado por ferramenta de QA.</summary>
        public static void ClearAll(IEnumerable<NarrativeSequence> sequences)
        {
            if (sequences == null)
            {
                return;
            }

            foreach (NarrativeSequence sequence in sequences)
            {
                if (sequence != null)
                {
                    PlayerPrefs.DeleteKey(Key(sequence.SequenceId));
                }
            }

            PlayerPrefs.Save();
        }

        private static string Key(string sequenceId)
        {
            string id = string.IsNullOrWhiteSpace(sequenceId) ? "unknown" : sequenceId.Trim().ToLowerInvariant();
            return Prefix + id + ".played";
        }
    }
}
