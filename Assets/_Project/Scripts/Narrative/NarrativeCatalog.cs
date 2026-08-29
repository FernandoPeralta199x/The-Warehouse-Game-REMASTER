using System;
using System.Collections.Generic;
using TW08.Data;
using UnityEngine;

namespace TW08.Narrative
{
    /// <summary>
    /// Índice da campanha narrativa: quem fala, quando, e em que setor.
    ///
    /// Mora em uma pasta Resources para que qualquer cena de puzzle consiga se
    /// autoconfigurar sem que os construtores de cena precisem conhecer narrativa.
    /// O elenco vem junto no asset para o retrato não depender de wiring de cena.
    /// </summary>
    [CreateAssetMenu(fileName = "NarrativeCatalog", menuName = "TW08/Narrative/Catalog")]
    public sealed class NarrativeCatalog : ScriptableObject
    {
        public const string ResourceName = "TW08_NarrativeCatalog";

        [SerializeField] private CharacterRoster roster;
        [Tooltip("Instala o diretor automaticamente em toda cena que tiver um PuzzleRuntime.")]
        [SerializeField] private bool autoInstallInScenes = true;
        [SerializeField] private List<NarrativeSequence> sequences = new();

        public CharacterRoster Roster => roster;
        public bool AutoInstallInScenes => autoInstallInScenes;
        public IReadOnlyList<NarrativeSequence> Sequences => sequences ?? (IReadOnlyList<NarrativeSequence>)Array.Empty<NarrativeSequence>();

        /// <summary>Melhor sequência para o momento, ignorando o que já foi visto.</summary>
        public NarrativeSequence Resolve(in NarrativeContext context)
        {
            return Resolve(context, null);
        }

        /// <summary>
        /// Melhor sequência para o momento entre as que <paramref name="isEligible"/>
        /// aceita. Quando a preferida já foi vista, a próxima candidata assume —
        /// é assim que uma fala de fase específica cede lugar à genérica do setor.
        /// </summary>
        public NarrativeSequence Resolve(in NarrativeContext context, Func<NarrativeSequence, bool> isEligible)
        {
            if (sequences == null)
            {
                return null;
            }

            NarrativeSequence best = null;
            int bestScore = int.MinValue;

            for (int i = 0; i < sequences.Count; i++)
            {
                NarrativeSequence candidate = sequences[i];
                if (candidate == null || candidate.Lines.Count == 0 || !candidate.Matches(context))
                {
                    continue;
                }

                if (isEligible != null && !isEligible(candidate))
                {
                    continue;
                }

                int score = candidate.Priority * 10 + candidate.Specificity;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        public NarrativeSequence Find(string sequenceId)
        {
            if (sequences == null || string.IsNullOrWhiteSpace(sequenceId))
            {
                return null;
            }

            foreach (NarrativeSequence sequence in sequences)
            {
                if (sequence != null &&
                    string.Equals(sequence.SequenceId, sequenceId, StringComparison.OrdinalIgnoreCase))
                {
                    return sequence;
                }
            }

            return null;
        }

        public static NarrativeCatalog LoadDefault()
        {
            return Resources.Load<NarrativeCatalog>(ResourceName);
        }

#if UNITY_EDITOR
        /// <summary>Escrita pelo setup de editor e pelos testes. Ver NarrativeSequence.ConfigureAuthoring.</summary>
        public void ConfigureAuthoring(
            CharacterRoster characterRoster, IEnumerable<NarrativeSequence> content, bool autoInstall)
        {
            roster = characterRoster;
            autoInstallInScenes = autoInstall;

            sequences ??= new List<NarrativeSequence>();
            sequences.Clear();
            if (content != null)
            {
                sequences.AddRange(content);
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
