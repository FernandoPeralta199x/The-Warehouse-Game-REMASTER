using System.Collections.Generic;
using TW08.Economy;
using TW08.Puzzle;
using UnityEngine;

namespace TW08.Audio
{
    /// <summary>
    /// Traduz o que acontece na fase em som.
    ///
    /// Fica separado do <see cref="PuzzleRuntime"/> de propósito: a regra do
    /// tabuleiro não deve saber que áudio existe. O diretor assina os eventos
    /// que já são publicados e decide o que tocar.
    ///
    /// A diferença entre passo, empurrão comum e empurrão de carga pesada é
    /// deliberada — o pilar de "peso" do documento de sound design pede que o
    /// jogador ouça a diferença de esforço antes de olhar para a tela.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleAudioDirector : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private PuzzleToolService toolService;
        [SerializeField] private TW08AudioCatalog catalog;
        [SerializeField] private bool playAmbience = true;

        private readonly HashSet<string> openGroups = new();
        private bool ambienceStarted;
        private bool completionPlayed;
        private bool alarmPlayed;

        public void Configure(PuzzleRuntime puzzleRuntime, PuzzleToolService tools, TW08AudioCatalog audioCatalog)
        {
            runtime = puzzleRuntime;
            toolService = tools;
            catalog = audioCatalog;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            if (catalog == null)
            {
                return;
            }

            if (runtime != null)
            {
                runtime.Initialized += OnInitialized;
                runtime.MoveApplied += OnMoveApplied;
                runtime.MoveUndone += OnMoveUndone;
                runtime.MoveRedone += OnMoveApplied;
                runtime.LevelRestarted += OnRestarted;
                runtime.LevelCompleted += OnCompleted;
                runtime.StaticDeadlockDetected += OnDeadlock;
                runtime.SwitchGroupStateChanged += OnSwitchChanged;
            }

            if (toolService != null)
            {
                toolService.ToolUsed += OnToolUsed;
            }

            StartAmbience();
        }

        private void OnDisable()
        {
            if (runtime != null)
            {
                runtime.Initialized -= OnInitialized;
                runtime.MoveApplied -= OnMoveApplied;
                runtime.MoveUndone -= OnMoveUndone;
                runtime.MoveRedone -= OnMoveApplied;
                runtime.LevelRestarted -= OnRestarted;
                runtime.LevelCompleted -= OnCompleted;
                runtime.StaticDeadlockDetected -= OnDeadlock;
                runtime.SwitchGroupStateChanged -= OnSwitchChanged;
            }

            if (toolService != null)
            {
                toolService.ToolUsed -= OnToolUsed;
            }

            StopAmbience();
        }

        private void OnInitialized()
        {
            openGroups.Clear();
            completionPlayed = false;
            alarmPlayed = false;
        }

        private void OnRestarted()
        {
            OnInitialized();
            Play(catalog.UiBack);
        }

        private void OnMoveApplied(PuzzleMove move)
        {
            if (!move.CrateMoved)
            {
                Play(catalog.PuzzleStep);
                return;
            }

            bool heavy = runtime != null
                         && runtime.Board != null
                         && runtime.Board.GetCrateKind(move.CrateId) == PuzzleEntityKind.HeavyCrate;

            Play(heavy ? catalog.CratePushHeavy : catalog.PuzzlePush);

            if (runtime?.Board == null)
            {
                return;
            }

            // Chegar ao alvo confirma; encostar em algo sólido soa como impacto.
            if (runtime.Board.IsGoal(move.CrateTo))
            {
                Play(catalog.CrateOnGoal);
                return;
            }

            GridCoordinate direction = move.CrateTo - move.CrateFrom;
            GridCoordinate ahead = move.CrateTo + direction;
            if (!runtime.Board.IsFree(ahead))
            {
                Play(catalog.CrateHit);
            }
        }

        private void OnMoveUndone(PuzzleMove _)
        {
            // Desfazer nunca reusa o som de empurrar: o jogador precisa
            // distinguir "avancei" de "voltei" sem olhar o contador.
            Play(catalog.UiBack);
        }

        private void OnCompleted()
        {
            if (completionPlayed)
            {
                return;
            }

            completionPlayed = true;
            Play(catalog.PuzzleSuccess);

            if (runtime?.Level != null && runtime.Board != null)
            {
                int medal = PuzzleProgressStore.EvaluateMedal(runtime.Level, runtime.Board.MoveCount);
                Play(catalog.MedalFor(medal));
            }
        }

        private void OnDeadlock()
        {
            // Alarme uma vez por travamento: repetir a cada frame viraria ruído.
            if (alarmPlayed)
            {
                return;
            }

            alarmPlayed = true;
            Play(catalog.PuzzleError);
        }

        private void OnSwitchChanged(string groupId, bool open)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return;
            }

            bool wasOpen = openGroups.Contains(groupId);
            if (open == wasOpen)
            {
                return;
            }

            if (open)
            {
                openGroups.Add(groupId);
                Play(catalog.SensorOn);
                Play(catalog.DoorOpen);
                return;
            }

            openGroups.Remove(groupId);
            Play(catalog.SensorOff);
            Play(catalog.DoorClose);
        }

        private void OnToolUsed(PuzzleToolDefinition tool)
        {
            if (tool == null)
            {
                return;
            }

            Play(tool.Kind switch
            {
                PuzzleToolKind.RewindMove => catalog.ToolRewind,
                PuzzleToolKind.LogisticsScanner => catalog.ToolScanner,
                PuzzleToolKind.ShiftAssistant => catalog.ToolAssistant,
                PuzzleToolKind.RouteMarker => catalog.ToolMarker,
                _ => null
            });
        }

        private void StartAmbience()
        {
            if (!playAmbience || ambienceStarted || catalog == null || AudioService.Instance == null)
            {
                return;
            }

            // Setor 03 é câmara fria: a ambiência muda com o cenário.
            bool freezer = runtime?.Level != null
                           && runtime.Level.SectorId != null
                           && runtime.Level.SectorId.Contains("03");

            AudioEvent ambience = freezer ? catalog.FreezerAmbience : catalog.WarehouseAmbience;
            if (ambience == null)
            {
                return;
            }

            AudioService.Instance.StartLoop(ambience);
            ambienceStarted = true;
        }

        private void StopAmbience()
        {
            if (!ambienceStarted || catalog == null || AudioService.Instance == null)
            {
                return;
            }

            // StopLoop indexa um Dictionary: id nulo lançaria em vez de virar no-op.
            StopLoopSafely(catalog.WarehouseAmbience);
            StopLoopSafely(catalog.FreezerAmbience);
            ambienceStarted = false;
        }

        private static void StopLoopSafely(AudioEvent audioEvent)
        {
            if (audioEvent != null && !string.IsNullOrEmpty(audioEvent.EventId))
            {
                AudioService.Instance.StopLoop(audioEvent.EventId);
            }
        }

        private static void Play(AudioEvent audioEvent)
        {
            if (audioEvent != null && AudioService.Instance != null)
            {
                AudioService.Instance.PlayOneShot(audioEvent);
            }
        }
    }
}
