using System.Collections;
using System.Collections.Generic;
using TW08.Economy;
using TW08.Motion;
using TW08.Puzzle;
using TW08.UI.Hud;
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
    ///
    /// Feedback tátil: o slot acende ao ficar disponível, pulsa ao ser usado e
    /// treme quando a ferramenta é recusada — a recusa precisa ser sentida sem
    /// depender de o jogador ler a mensagem.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleToolBarController : MonoBehaviour
    {
        private const float MessageDuration = 3.5f;
        private const float MessageFadeOut = 0.3f;

        [SerializeField] private PuzzleToolService toolService;
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private List<Button> slotButtons = new();
        [SerializeField] private Text messageText;
        [SerializeField] private Text modeText;

        [Header("Movimento")]
        [SerializeField] private List<Text> slotLabels = new();
        [SerializeField] private List<Text> slotCounters = new();

        private readonly List<PuzzleToolDefinition> boundTools = new();
        private readonly List<bool> slotAvailability = new();
        private readonly List<AnimatedCounter> slotCounterAnimators = new();
        private readonly List<MotionHandle> slotEffects = new();
        private readonly List<MotionHandle> slotFlashes = new();

        private Coroutine messageRoutine;
        private MotionHandle messageFadeHandle;
        private MotionHandle messageSlideHandle;
        private MotionHandle modeColorHandle;
        private MotionHandle modePunchHandle;

        private RectTransform messageRect;
        private Vector2 messageHome;
        private bool messageHomeCached;
        private bool slotsPrimed;
        private bool modePrimed;
        private bool modeAssisted;

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
            messageHomeCached = false;
            slotsPrimed = false;
            modePrimed = false;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        /// <summary>
        /// Rótulo e contador dedicados de cada slot. Sem eles a barra volta ao
        /// formato antigo, com o número embutido no nome da ferramenta e o
        /// rótulo descoberto por busca no filho do botão.
        /// </summary>
        public void ConfigureSlotDisplays(IEnumerable<Text> labels, IEnumerable<Text> counters)
        {
            slotLabels = labels != null ? new List<Text>(labels) : new List<Text>();
            slotCounters = counters != null ? new List<Text>(counters) : new List<Text>();
            slotCounterAnimators.Clear();
            slotsPrimed = false;

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
                runtime.AssistanceUsed += Refresh;
            }

            CacheMessageHome();
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
                runtime.AssistanceUsed -= Refresh;
            }

            foreach (Button button in slotButtons)
            {
                button?.onClick.RemoveAllListeners();
            }

            StopMotion();
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
                ShakeSlot(index);
            }

            Refresh();
        }

        private void OnToolUsed(PuzzleToolDefinition tool)
        {
            int index = boundTools.IndexOf(tool);

            if (tool != null && tool.Kind != PuzzleToolKind.ShiftAssistant)
            {
                ShowMessage($"{tool.DisplayName} acionada.");
            }

            Refresh();
            PunchSlot(index);
        }

        private void OnBoardChanged(PuzzleMove _) => Refresh();

        private void Refresh()
        {
            boundTools.Clear();
            if (toolService != null)
            {
                boundTools.AddRange(toolService.GetEquippedTools());
            }

            EnsureSlotState();

            for (int i = 0; i < slotButtons.Count; i++)
            {
                Button button = slotButtons[i];
                if (button == null)
                {
                    continue;
                }

                Text label = ResolveSlotLabel(i, button);
                Text counter = i < slotCounters.Count ? slotCounters[i] : null;

                if (i >= boundTools.Count)
                {
                    button.interactable = false;
                    if (label != null)
                    {
                        label.text = "SLOT VAZIO";
                        label.color = HudPalette.TextMuted;
                    }

                    if (counter != null)
                    {
                        // Solta o rótulo do contador: sem isso ele guardaria o
                        // último número e um slot reequipado com a mesma
                        // contagem ficaria em branco para sempre.
                        slotCounterAnimators[i].Attach(null);
                        counter.text = string.Empty;
                    }

                    slotAvailability[i] = false;
                    continue;
                }

                PuzzleToolDefinition tool = boundTools[i];
                bool usable = toolService.CanUse(tool, out _);
                button.interactable = usable;

                int remaining = toolService.RemainingUses(tool);
                if (counter != null)
                {
                    AnimatedCounter animator = slotCounterAnimators[i];
                    animator.Attach(counter);
                    animator.Set(remaining);
                    if (label != null)
                    {
                        label.text = tool.ShortLabel;
                    }
                }
                else if (label != null)
                {
                    label.text = $"{tool.ShortLabel} x{remaining}";
                }

                if (label != null)
                {
                    label.color = usable ? HudPalette.Cyan : HudPalette.TextMuted;
                }

                // Só acende na transição indisponível -> disponível: repintar todo
                // Refresh faria a barra piscar a cada movimento do tabuleiro.
                if (usable && !slotAvailability[i] && slotsPrimed)
                {
                    LightUpSlot(i, label);
                }

                slotAvailability[i] = usable;
            }

            slotsPrimed = true;
            RefreshModeLabel();
        }

        /// <summary>
        /// Rótulo do slot. A busca por filho só entra como reserva: com o
        /// contador de usos no mesmo botão, a ordem dos filhos deixa de ser uma
        /// forma confiável de achar o rótulo principal.
        /// </summary>
        private Text ResolveSlotLabel(int index, Button button)
        {
            if (index >= 0 && index < slotLabels.Count && slotLabels[index] != null)
            {
                return slotLabels[index];
            }

            return button != null ? button.GetComponentInChildren<Text>() : null;
        }

        /// <summary>Mantém as listas paralelas com o mesmo tamanho da lista de slots.</summary>
        private void EnsureSlotState()
        {
            while (slotAvailability.Count < slotButtons.Count)
            {
                slotAvailability.Add(false);
            }

            while (slotCounterAnimators.Count < slotButtons.Count)
            {
                slotCounterAnimators.Add(new AnimatedCounter(HudFormat.ToolUsesFormat, 0.26f));
            }

            while (slotEffects.Count < slotButtons.Count)
            {
                slotEffects.Add(null);
            }

            while (slotFlashes.Count < slotButtons.Count)
            {
                slotFlashes.Add(null);
            }
        }

        private void LightUpSlot(int index, Text label)
        {
            Button button = slotButtons[index];
            if (button == null)
            {
                return;
            }

            MotionHandle effect = slotEffects[index];
            HudFx.PopIn(ref effect, button.transform, 0.3f, 0.86f);
            slotEffects[index] = effect;

            if (label == null)
            {
                return;
            }

            MotionHandle flash = slotFlashes[index];
            flash?.Kill();
            slotFlashes[index] = HudFx.Flash(label, Color.white, HudPalette.Cyan, 0.4f);
        }

        private void PunchSlot(int index)
        {
            if (index < 0 || index >= slotButtons.Count)
            {
                return;
            }

            Button button = slotButtons[index];
            if (button == null)
            {
                return;
            }

            MotionHandle effect = slotEffects[index];
            HudFx.Punch(ref effect, button.transform, 0.2f, 0.32f);
            slotEffects[index] = effect;
        }

        private void ShakeSlot(int index)
        {
            if (index < 0 || index >= slotButtons.Count)
            {
                return;
            }

            Button button = slotButtons[index];
            if (button == null || button.transform is not RectTransform rect)
            {
                return;
            }

            MotionHandle effect = slotEffects[index];
            HudFx.Shake(ref effect, rect, 9f, 0.36f);
            slotEffects[index] = effect;
        }

        private void RefreshModeLabel()
        {
            if (modeText == null)
            {
                return;
            }

            bool assisted = runtime != null && runtime.IsAssisted;
            modeText.text = HudFormat.RankingLine(assisted);
            Color target = assisted ? HudPalette.Amber : HudPalette.Green;

            if (!modePrimed)
            {
                modePrimed = true;
                modeAssisted = assisted;
                modeText.color = target;
                return;
            }

            if (assisted == modeAssisted)
            {
                return;
            }

            modeAssisted = assisted;
            HudFx.Abort(ref modeColorHandle);
            modeColorHandle = UIMotion.ColorTo(modeText, target, 0.32f, Ease.OutQuad);
            HudFx.Punch(ref modePunchHandle, modeText.transform, 0.2f, 0.4f);
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

            CacheMessageHome();

            HudFx.Abort(ref messageFadeHandle);
            messageFadeHandle = HudFx.FadeInFrom(messageText, HudPalette.Amber, 0.22f);

            if (messageRect != null)
            {
                HudFx.Finish(ref messageSlideHandle);
                // Reposiciona antes de deslizar: SlideIn assume que a posição
                // atual é o destino, e um aviso interrompido deixaria o destino
                // deslocado para sempre.
                messageRect.anchoredPosition = messageHome;
                messageSlideHandle = UIMotion.SlideIn(messageRect, new Vector2(0f, -20f), 0.3f, Ease.OutCubic);
            }

            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
            }

            messageRoutine = StartCoroutine(FadeMessageOut());
        }

        private IEnumerator FadeMessageOut()
        {
            yield return new WaitForSecondsRealtime(MessageDuration);

            if (messageText != null)
            {
                HudFx.Abort(ref messageFadeHandle);
                messageFadeHandle = UIMotion.FadeTo(messageText, 0f, MessageFadeOut, Ease.InQuad);
            }

            yield return new WaitForSecondsRealtime(MessageFadeOut);

            if (messageText != null)
            {
                messageText.text = string.Empty;
            }

            messageRoutine = null;
        }

        private void CacheMessageHome()
        {
            if (messageHomeCached || messageText == null)
            {
                return;
            }

            messageRect = messageText.rectTransform;
            messageHome = messageRect.anchoredPosition;
            messageHomeCached = true;
        }

        private void StopMotion()
        {
            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
                messageRoutine = null;
            }

            HudFx.Finish(ref messageFadeHandle);
            HudFx.Finish(ref messageSlideHandle);
            HudFx.Finish(ref modeColorHandle);
            HudFx.Finish(ref modePunchHandle);

            for (int i = 0; i < slotEffects.Count; i++)
            {
                slotEffects[i]?.Complete();
                slotEffects[i] = null;
            }

            for (int i = 0; i < slotFlashes.Count; i++)
            {
                slotFlashes[i]?.Complete();
                slotFlashes[i] = null;
            }

            foreach (AnimatedCounter counter in slotCounterAnimators)
            {
                counter?.Stop();
            }
        }
    }
}
