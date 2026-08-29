using UnityEngine;

namespace TW08.UI.Hud
{
    /// <summary>Estado que a faixa de status do puzzle está comunicando.</summary>
    public enum PuzzleHudStatus
    {
        /// <summary>Sem tabuleiro válido — fase quebrada ou ainda não inicializada.</summary>
        Unavailable,

        /// <summary>Turno em andamento.</summary>
        Active,

        /// <summary>Carga presa num canto: o turno não tem mais solução sem undo.</summary>
        Deadlock,

        /// <summary>Todas as cargas entregues.</summary>
        Complete
    }

    /// <summary>
    /// Decide estado, texto e cor da faixa de status.
    ///
    /// É lógica pura de propósito: os controllers só animam a transição, e o
    /// teste de HUD consegue cobrir a decisão sem montar cena.
    /// </summary>
    public static class PuzzleHudStatusResolver
    {
        public const string UnavailableLabel = "ROTA INDISPONÍVEL";
        public const string ActiveLabel = "ROTA ATIVA";
        public const string DeadlockLabel = "ALERTA: CARGA TRAVADA // USE UNDO";
        public const string CompleteLabel = "ROTA LIBERADA";

        public static PuzzleHudStatus Resolve(bool hasBoard, bool isComplete, bool deadlocked)
        {
            if (!hasBoard)
            {
                return PuzzleHudStatus.Unavailable;
            }

            if (isComplete)
            {
                return PuzzleHudStatus.Complete;
            }

            // Conclusão vence travamento: um tabuleiro completo nunca é alarme.
            return deadlocked ? PuzzleHudStatus.Deadlock : PuzzleHudStatus.Active;
        }

        public static string LabelFor(PuzzleHudStatus status)
        {
            return status switch
            {
                PuzzleHudStatus.Complete => CompleteLabel,
                PuzzleHudStatus.Deadlock => DeadlockLabel,
                PuzzleHudStatus.Active => ActiveLabel,
                _ => UnavailableLabel
            };
        }

        /// <summary>Faixa de conclusão, com ou sem o extrato já fechado pela economia.</summary>
        public static string CompletionLabel(int medal, bool assisted, int creditsEarned, bool hasReport)
        {
            string ranking = assisted ? "ASSISTIDO" : "LIMPO";
            return hasReport
                ? $"ROTA LIBERADA // MEDALHA {medal} // {ranking} // +{creditsEarned} CRÉDITOS"
                : $"ROTA LIBERADA // MEDALHA {medal} // {ranking}";
        }

        public static Color ColorFor(PuzzleHudStatus status)
        {
            return status switch
            {
                PuzzleHudStatus.Complete => HudPalette.Cyan,
                PuzzleHudStatus.Deadlock => HudPalette.Red,
                PuzzleHudStatus.Active => HudPalette.Green,
                _ => HudPalette.TextMuted
            };
        }

        /// <summary>Estados que pedem tremor e cor piscando em vez de transição suave.</summary>
        public static bool IsAlarming(PuzzleHudStatus status)
        {
            return status == PuzzleHudStatus.Deadlock;
        }
    }
}
