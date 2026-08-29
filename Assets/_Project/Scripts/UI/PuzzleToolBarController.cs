using System.Collections;
using System.Collections.Generic;
using TW08.Economy;
using TW08.Puzzle;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>
    /// Barra de ferramentas da fase. Cada slot representa uma ferramenta
    /// equipada e mostra quantos usos restam no turno.
    ///
    /// A barra também avisa quando o turno vira assistido, porque a bíblia de
    /// design exige que o jogador saiba na hora que saiu do ranking competitivo.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleToolBarController : MonoBehaviour
    {
        private const float MessageDuration = 3.5f;

        [SerializeField] private PuzzleToolService toolService;
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private List<Button> slotButtons = new();
        [SerializeField] private Text messageText;
        [SerializeField] private Text modeText;

        private readonly List<PuzzleToolDefinition> boundTools = new();
        private Coroutine messageRoutine;

        public void Configure(
            PuzzleToolService service,
            PuzzleRuntime puzzleRuntime,
            IEnumerable<Button> slots,
            Text message,
            Text mode)
        {
            toolService = service;
            runtime = puzzleRuntime;
            slotButtons = new List<Button>(slots ?? new List<Button>());
            messageText = message;
            modeText = mode;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            if (toolService != null)
            {
                toolService.ToolUsed += OnToolUsed;
                toolService.HintRevealed += ShowMessage;
            }

            if (runtime != null)
            {
                runtime.Initialized += Refresh;
                runtime.LevelRestarted += Refresh;
                runtime.MoveApplied += OnBoardChanged;
                runtime.LevelCompleted += Refresh;
            }

            BindSlots();
            Refresh();
        }

        private void OnDisable()
        {
            if (toolService != null)
            {
                toolService.ToolUsed -= OnToolUsed;
                toolService.HintRevealed -= ShowMessage;
            }

            if (runtime != null)
            {
                runtime.Initialized -= Refresh;
                runtime.LevelRestarted -= Refresh;
                runtime.MoveApplied -= OnBoardChanged;
                runtime.LevelCompleted -= Refresh;
            }

            foreach (Button button in slotButtons)
            {
                button?.onClick.RemoveAllListeners();
            }
        }

        private void BindSlots()
        {
            for (int i = 0; i < slotButtons.Count; i++)
            {
                Button button = slotButtons[i];
                if (button == null)
                {
                    continue;
                }

                int index = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => UseSlot(index));
            }
        }

        private void UseSlot(int index)
        {
            if (toolService == null || index < 0 || index >= boundTools.Count)
            {
                return;
            }

            PuzzleToolDefinition tool = boundTools[index];
            if (!toolService.TryUse(tool))
            {
                ShowMessage(toolService.LastRejection);
            }

            Refresh();
        }

        private void OnToolUsed(PuzzleToolDefinition tool)
        {
            if (tool != null && tool.Kind != PuzzleToolKind.ShiftAssistant)
            {
                ShowMessage($"{tool.DisplayName} acionada.");
            }

            Refresh();
        }

        private void OnBoardChanged(PuzzleMove _) => Refresh();

        private void Refresh()
        {
            boundTools.Clear();
            if (toolService != null)
            {
                boundTools.AddRange(toolService.GetEquippedTools());
            }

            for (int i = 0; i < slotButtons.Count; i++)
            {
                Button button = slotButtons[i];
                if (button == null)
                {
                    continue;
                }

                Text label = button.GetComponentInChildren<Text>();
                if (i >= boundTools.Count)
                {
                    button.interactable = false;
                    if (label != null)
                    {
                        label.text = "SLOT VAZIO";
                    }

                    continue;
                }

                PuzzleToolDefinition tool = boundTools[i];
                bool usable = toolService.CanUse(tool, out _);
                button.interactable = usable;
                if (label != null)
                {
                    label.text = $"{tool.ShortLabel} x{toolService.RemainingUses(tool)}";
                }
            }

            RefreshModeLabel();
        }

        private void RefreshModeLabel()
        {
            if (modeText == null)
            {
                return;
            }

            if (runtime != null && runtime.IsAssisted)
            {
                modeText.text = "MODO ASSISTIDO // FORA DO RANKING";
                modeText.color = new Color(0.94f, 0.61f, 0.12f, 1f);
                return;
            }

            modeText.text = "TURNO LIMPO // RANKING ATIVO";
            modeText.color = new Color(0.36f, 0.85f, 0.45f, 1f);
        }

        private void ShowMessage(string message)
        {
            if (messageText == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            messageText.text = message;
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
            }

            messageRoutine = StartCoroutine(ClearMessageAfterDelay());
        }

        private IEnumerator ClearMessageAfterDelay()
        {
            yield return new WaitForSeconds(MessageDuration);
            if (messageText != null)
            {
                messageText.text = string.Empty;
            }

            messageRoutine = null;
        }
    }
}
