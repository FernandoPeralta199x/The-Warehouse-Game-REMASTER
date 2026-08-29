using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Economy;
using UnityEngine;

namespace TW08.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class PuzzleRuntime : MonoBehaviour
    {
        [SerializeField] private PuzzleLevelDefinition level;
        [SerializeField] private PuzzleEntityView playerView;
        [SerializeField] private List<PuzzleEntityView> crateViews = new();
        [SerializeField] private bool initializeOnStart = true;

        private readonly PuzzleHistory history = new();
        private readonly Dictionary<string, bool> switchStates = new(StringComparer.Ordinal);
        private Dictionary<string, PuzzleEntityView> crateViewById = new();

        public PuzzleBoardModel Board { get; private set; }
        public PuzzleLevelDefinition Level => level;
        public int UndoCount => history.UndoCount;
        public int RedoCount => history.RedoCount;

        /// <summary>Empurrões de carga no turno atual — zera junto com o tabuleiro.</summary>
        public int PushCount { get; private set; }

        /// <summary>Ferramentas da Oficina N-8 acionadas no turno atual.</summary>
        public int ToolsUsed { get; private set; }

        /// <summary>Dicas do Assistente de Turno reveladas no turno atual.</summary>
        public int HintsUsed { get; private set; }

        /// <summary>Turno assistido perde o direito ao ranking competitivo.</summary>
        public bool IsAssisted => ToolsUsed > 0 || HintsUsed > 0;

        public event Action Initialized;
        public event Action<PuzzleMove> MoveApplied;
        public event Action<PuzzleMove> MoveUndone;
        public event Action<PuzzleMove> MoveRedone;
        public event Action LevelRestarted;
        public event Action LevelCompleted;
        public event Action StaticDeadlockDetected;
        public event Action<string, bool> SwitchGroupStateChanged;

        /// <summary>Disparado quando uma ferramenta é acionada — a HUD reage a isso.</summary>
        public event Action AssistanceUsed;

        private void Start()
        {
            if (initializeOnStart)
            {
                Initialize();
            }
        }

        public void Configure(PuzzleLevelDefinition definition, PuzzleEntityView player, IEnumerable<PuzzleEntityView> crates)
        {
            level = definition;
            playerView = player;
            crateViews = crates?.ToList() ?? new List<PuzzleEntityView>();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                if (gameObject.scene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
            }
#endif
        }

        public void Initialize()
        {
            IReadOnlyList<string> validationErrors = PuzzleLevelValidator.Validate(level);
            if (validationErrors.Count > 0)
            {
                Board = null;
                history.Clear();
                switchStates.Clear();
                crateViewById.Clear();
                Debug.LogError("Puzzle level is invalid:\n- " + string.Join("\n- ", validationErrors), level);
                return;
            }

            Board = new PuzzleBoardModel(level);
            history.Clear();
            switchStates.Clear();
            PushCount = 0;
            ToolsUsed = 0;
            HintsUsed = 0;
            crateViewById = crateViews
                .Where(view => view != null && !string.IsNullOrWhiteSpace(view.EntityId))
                .GroupBy(view => view.EntityId)
                .ToDictionary(group => group.Key, group => group.First());

            ApplySwitchGroups(true);
            SyncViews(false);
            Initialized?.Invoke();
        }

        public bool TryMove(GridCoordinate direction)
        {
            if (Board == null || Board.IsComplete || !Board.TryMove(direction, out PuzzleMove move))
            {
                return false;
            }

            history.Record(move);
            if (move.CrateMoved)
            {
                PushCount++;
            }

            ApplySwitchGroups(false);
            SyncViews(true);
            MoveApplied?.Invoke(move);
            EvaluateBoardState();
            return true;
        }

        /// <summary>
        /// Contabiliza o uso de uma ferramenta da Oficina N-8. Dicas contam
        /// separadamente porque a bíblia de design as trata como bônus próprio.
        /// </summary>
        public void RegisterAssistance(bool isHint)
        {
            if (isHint)
            {
                HintsUsed++;
            }
            else
            {
                ToolsUsed++;
            }

            AssistanceUsed?.Invoke();
        }

        /// <summary>Fotografia do turno para o cálculo de Créditos de Turno.</summary>
        public PuzzleRunSummary BuildSummary()
        {
            return new PuzzleRunSummary
            {
                Moves = Board?.MoveCount ?? 0,
                Pushes = PushCount,
                ToolsUsed = ToolsUsed,
                HintsUsed = HintsUsed
            };
        }

        public bool Undo()
        {
            if (Board == null || !history.TryPopUndo(out PuzzleMove move))
            {
                return false;
            }

            if (!Board.TryUndo(move))
            {
                history.RestoreUndo(move);
                return false;
            }

            history.PushRedo(move);
            if (move.CrateMoved)
            {
                PushCount = Math.Max(0, PushCount - 1);
            }

            ApplySwitchGroups(false);
            SyncViews(true);
            MoveUndone?.Invoke(move);
            return true;
        }

        public bool Redo()
        {
            if (Board == null || !history.TryPopRedo(out PuzzleMove move))
            {
                return false;
            }

            GridCoordinate direction = move.PlayerTo - move.PlayerFrom;
            if (!Board.TryMove(direction, out PuzzleMove repeated))
            {
                history.PushRedo(move);
                return false;
            }

            history.RestoreUndo(repeated);
            if (repeated.CrateMoved)
            {
                PushCount++;
            }

            ApplySwitchGroups(false);
            SyncViews(true);
            MoveRedone?.Invoke(repeated);
            EvaluateBoardState();
            return true;
        }

        public void Restart()
        {
            Initialize();
            if (Board != null)
            {
                LevelRestarted?.Invoke();
            }
        }

        public bool IsSwitchGroupOpen(string groupId)
        {
            return !string.IsNullOrWhiteSpace(groupId)
                && switchStates.TryGetValue(groupId, out bool open)
                && open;
        }

        private void ApplySwitchGroups(bool forceNotify)
        {
            if (Board == null || level == null)
            {
                return;
            }

            IReadOnlyList<PuzzleSwitchGroupDefinition> groups = level.SwitchGroups;
            if (groups == null || groups.Count == 0)
            {
                return;
            }

            foreach (PuzzleSwitchGroupDefinition group in groups)
            {
                if (group == null || string.IsNullOrWhiteSpace(group.Id))
                {
                    continue;
                }

                IReadOnlyList<GridCoordinate> doors = group.Doors ?? Array.Empty<GridCoordinate>();
                bool requestedOpen = group.Sensors != null
                    && group.Sensors.Count > 0
                    && group.Sensors.All(sensor => Board.Crates.ContainsKey(sensor));

                bool effectiveOpen = requestedOpen;
                if (requestedOpen)
                {
                    foreach (GridCoordinate door in doors)
                    {
                        Board.SetDynamicBlocked(door, false);
                    }
                }
                else
                {
                    bool allClosed = true;
                    foreach (GridCoordinate door in doors)
                    {
                        if (!Board.SetDynamicBlocked(door, true) && !Board.DynamicBlockedCells.Contains(door))
                        {
                            allClosed = false;
                        }
                    }

                    if (!allClosed)
                    {
                        // Door groups transition atomically: if one panel cannot close because the player
                        // or cargo occupies the cell, keep every panel in the group open.
                        foreach (GridCoordinate door in doors)
                        {
                            Board.SetDynamicBlocked(door, false);
                        }
                        effectiveOpen = true;
                    }
                    else
                    {
                        effectiveOpen = false;
                    }
                }

                bool changed = !switchStates.TryGetValue(group.Id, out bool previous) || previous != effectiveOpen;
                switchStates[group.Id] = effectiveOpen;
                if (forceNotify || changed)
                {
                    SwitchGroupStateChanged?.Invoke(group.Id, effectiveOpen);
                }
            }
        }

        private void EvaluateBoardState()
        {
            if (Board == null)
            {
                return;
            }

            if (Board.IsComplete)
            {
                LevelCompleted?.Invoke();
            }
            else if (SimpleDeadlockDetector.HasStaticCornerDeadlock(Board))
            {
                StaticDeadlockDetected?.Invoke();
            }
        }

        private void SyncViews(bool animate)
        {
            if (Board == null)
            {
                return;
            }

            playerView?.MoveTo(Board.PlayerPosition, level.CellSize, animate);
            foreach (KeyValuePair<GridCoordinate, string> crate in Board.Crates)
            {
                if (crateViewById.TryGetValue(crate.Value, out PuzzleEntityView view))
                {
                    view.MoveTo(crate.Key, level.CellSize, animate);
                }
            }
        }
    }
}
