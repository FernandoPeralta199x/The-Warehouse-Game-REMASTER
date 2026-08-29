using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Puzzle;
using TW08.Save;
using UnityEngine;

namespace TW08.Economy
{
    /// <summary>
    /// Executa as ferramentas da Oficina N-8 dentro de uma fase.
    ///
    /// O serviço só gasta estoque quando a ferramenta realmente produz efeito, e
    /// respeita duas travas: o limite de usos por turno de cada ferramenta e o
    /// bloqueio por fase (<see cref="PuzzleLevelDefinition.AllowPowerUps"/>), que
    /// o level designer usa em fases que uma ferramenta quebraria.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleToolService : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private PuzzleToolCatalog catalog;
        [SerializeField] private SaveManager saveManager;

        private readonly Dictionary<PuzzleToolKind, int> usesThisLevel = new();
        private int hintTier;

        /// <summary>Ferramenta usada com sucesso (para SFX/HUD).</summary>
        public event Action<PuzzleToolDefinition> ToolUsed;

        /// <summary>Célula da carga crítica apontada pelo Scanner Logístico.</summary>
        public event Action<GridCoordinate> ScannerHighlighted;

        /// <summary>Texto da dica revelada pelo Assistente de Turno.</summary>
        public event Action<string> HintRevealed;

        /// <summary>Alvos ainda descobertos, destacados pelo Marcador de Rota.</summary>
        public event Action<IReadOnlyList<GridCoordinate>> RouteMarked;

        /// <summary>Motivo textual da última recusa — a HUD mostra ao jogador.</summary>
        public string LastRejection { get; private set; } = string.Empty;

        public PuzzleToolCatalog Catalog => catalog;

        public void Configure(PuzzleRuntime puzzleRuntime, PuzzleToolCatalog toolCatalog, SaveManager save)
        {
            runtime = puzzleRuntime;
            catalog = toolCatalog;
            saveManager = save;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            if (runtime != null)
            {
                runtime.Initialized += ResetLevelUsage;
                runtime.LevelRestarted += ResetLevelUsage;
            }

            if (saveManager == null)
            {
                saveManager = FindFirstObjectByType<SaveManager>();
            }
        }

        private void OnDisable()
        {
            if (runtime != null)
            {
                runtime.Initialized -= ResetLevelUsage;
                runtime.LevelRestarted -= ResetLevelUsage;
            }
        }

        /// <summary>Ferramentas equipadas para este turno que ainda têm estoque.</summary>
        public IReadOnlyList<PuzzleToolDefinition> GetEquippedTools()
        {
            if (catalog == null || saveManager?.Data == null)
            {
                return Array.Empty<PuzzleToolDefinition>();
            }

            return saveManager.Data.equippedTools
                .Select(catalog.Find)
                .Where(tool => tool != null && saveManager.Data.GetToolCount(tool.ToolId) > 0)
                .ToList();
        }

        public int RemainingUses(PuzzleToolDefinition tool)
        {
            if (tool == null)
            {
                return 0;
            }

            usesThisLevel.TryGetValue(tool.Kind, out int used);
            return Mathf.Max(0, tool.UsesPerLevel - used);
        }

        public bool CanUse(PuzzleToolDefinition tool, out string reason)
        {
            reason = string.Empty;

            if (tool == null || runtime == null || runtime.Board == null)
            {
                reason = "Ferramenta indisponível.";
                return false;
            }

            if (runtime.Board.IsComplete)
            {
                reason = "Turno encerrado.";
                return false;
            }

            if (runtime.Level != null && !runtime.Level.AllowPowerUps)
            {
                reason = "Ferramentas bloqueadas nesta rota.";
                return false;
            }

            if (RemainingUses(tool) <= 0)
            {
                reason = "Limite desta ferramenta atingido no turno.";
                return false;
            }

            if (saveManager?.Data == null || saveManager.Data.GetToolCount(tool.ToolId) <= 0)
            {
                reason = "Sem unidades no estoque.";
                return false;
            }

            return true;
        }

        public bool TryUse(PuzzleToolKind kind)
        {
            return TryUse(catalog != null ? catalog.Find(kind) : null);
        }

        public bool TryUse(PuzzleToolDefinition tool)
        {
            if (!CanUse(tool, out string reason))
            {
                LastRejection = reason;
                return false;
            }

            if (!Execute(tool))
            {
                LastRejection = "A ferramenta não teve efeito agora.";
                return false;
            }

            // Só cobra estoque depois de confirmar o efeito.
            saveManager.TryConsumeTool(tool.ToolId);
            usesThisLevel[tool.Kind] = usesThisLevel.GetValueOrDefault(tool.Kind) + 1;
            runtime.RegisterAssistance(tool.Kind == PuzzleToolKind.ShiftAssistant);
            LastRejection = string.Empty;
            ToolUsed?.Invoke(tool);
            return true;
        }

        private bool Execute(PuzzleToolDefinition tool)
        {
            switch (tool.Kind)
            {
                case PuzzleToolKind.RewindMove:
                    int undone = 0;
                    for (int i = 0; i < 3 && runtime.Undo(); i++)
                    {
                        undone++;
                    }

                    return undone > 0;

                case PuzzleToolKind.LogisticsScanner:
                    if (!PuzzleAdvisor.TryFindCriticalCrate(runtime.Board, out GridCoordinate critical))
                    {
                        return false;
                    }

                    ScannerHighlighted?.Invoke(critical);
                    return true;

                case PuzzleToolKind.ShiftAssistant:
                    hintTier = Mathf.Clamp(hintTier + 1, 1, 3);
                    HintRevealed?.Invoke(PuzzleAdvisor.BuildHint(runtime.Board, hintTier));
                    return true;

                case PuzzleToolKind.RouteMarker:
                    IReadOnlyList<GridCoordinate> goals = PuzzleAdvisor.FindOpenGoals(runtime.Board);
                    if (goals.Count == 0)
                    {
                        return false;
                    }

                    RouteMarked?.Invoke(goals);
                    return true;

                default:
                    return false;
            }
        }

        private void ResetLevelUsage()
        {
            usesThisLevel.Clear();
            hintTier = 0;
            LastRejection = string.Empty;
        }
    }
}
