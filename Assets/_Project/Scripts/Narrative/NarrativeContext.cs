using System;

namespace TW08.Narrative
{
    /// <summary>Momento do jogo em que uma sequência pode entrar.</summary>
    public enum NarrativeTriggerKind
    {
        /// <summary>Só toca quando alguém chama explicitamente (gatilho de cena, debug).</summary>
        Manual = 0,

        /// <summary>Chegada de John ao armazém — abre a campanha.</summary>
        Opening = 1,

        /// <summary>Primeira vez que o jogador pisa em um setor.</summary>
        SectorEntry = 2,

        /// <summary>Antes do primeiro movimento de uma fase específica.</summary>
        LevelStart = 3,

        /// <summary>Depois que o tabuleiro é resolvido.</summary>
        LevelCompleted = 4,

        /// <summary>Desfecho da campanha.</summary>
        Ending = 5
    }

    /// <summary>
    /// Situação consultada pelo catálogo: "estou entrando no setor S03, fase X".
    /// Setor e fase já chegam normalizados para a comparação nunca depender de
    /// como o dado foi digitado no asset.
    /// </summary>
    public readonly struct NarrativeContext
    {
        public NarrativeContext(NarrativeTriggerKind trigger, string sectorId, string levelId)
        {
            Trigger = trigger;
            SectorId = NarrativeMatching.Normalize(sectorId);
            LevelId = NarrativeMatching.Normalize(levelId);
        }

        public NarrativeTriggerKind Trigger { get; }
        public string SectorId { get; }
        public string LevelId { get; }
    }

    /// <summary>
    /// Regra de casamento entre o gancho declarado na sequência e o contexto atual.
    /// Lógica pura de propósito: é o único ponto onde a campanha decide o que
    /// aparece na tela, e precisa ser testável sem cena, sem asset e sem Play Mode.
    /// </summary>
    public static class NarrativeMatching
    {
        public static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Campo de filtro vazio funciona como curinga: uma sequência de setor sem
        /// fase declarada entra na primeira fase daquele setor que o jogador abrir.
        /// </summary>
        public static bool Matches(
            NarrativeTriggerKind trigger, string sectorId, string levelId, in NarrativeContext context)
        {
            if (trigger == NarrativeTriggerKind.Manual || trigger != context.Trigger)
            {
                return false;
            }

            string level = Normalize(levelId);
            if (level.Length > 0 && !string.Equals(level, context.LevelId, StringComparison.Ordinal))
            {
                return false;
            }

            string sector = Normalize(sectorId);
            if (sector.Length > 0 && !string.Equals(sector, context.SectorId, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Peso do desempate quando duas sequências casam com o mesmo contexto.
        /// Filtro de fase vale mais que filtro de setor: o específico ganha do geral.
        /// </summary>
        public static int Specificity(string sectorId, string levelId)
        {
            int score = 0;
            if (Normalize(levelId).Length > 0)
            {
                score += 2;
            }

            if (Normalize(sectorId).Length > 0)
            {
                score += 1;
            }

            return score;
        }
    }
}
