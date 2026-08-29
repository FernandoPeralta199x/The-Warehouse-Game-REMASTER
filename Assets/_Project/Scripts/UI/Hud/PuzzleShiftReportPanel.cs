using System.Collections.Generic;
using TW08.Economy;
using TW08.Motion;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Hud
{
    /// <summary>
    /// Tela de conclusão de turno: medalha, extrato de Créditos de Turno linha a
    /// linha e o total subindo.
    ///
    /// O painel só apresenta — quem fecha o turno e credita continua sendo o
    /// <c>SaveManager</c>. Se qualquer animação falhar, os números já estão
    /// gravados; por isso todo passo aqui reaplica o texto final ao terminar.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleShiftReportPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text medalText;
        [SerializeField] private Text rankingText;
        [SerializeField] private Text totalText;
        [SerializeField] private Text balanceText;
        [SerializeField] private List<Text> lineLabels = new();
        [SerializeField, Min(0f)] private float lineInterval = 0.085f;
        [SerializeField, Min(0.05f)] private float panelDuration = 0.42f;

        private readonly List<MotionHandle> handles = new();
        private readonly AnimatedCounter totalCounter = new(HudFormat.CreditsFormat, 0.85f);
        private readonly AnimatedCounter balanceCounter = new(HudFormat.BalanceFormat, 0.85f);

        private MotionHandle medalPunchHandle;
        private MotionHandle medalPopHandle;
        private Vector2 panelHome;
        private bool panelHomeCached;

        public void Configure(
            CanvasGroup panelGroup,
            RectTransform panelRect,
            Text title,
            Text medal,
            Text ranking,
            Text total,
            Text balance,
            IEnumerable<Text> statementLines)
        {
            group = panelGroup;
            panel = panelRect;
            titleText = title;
            medalText = medal;
            rankingText = ranking;
            totalText = total;
            balanceText = balance;
            lineLabels = statementLines != null ? new List<Text>(statementLines) : new List<Text>();

            totalCounter.Attach(totalText);
            balanceCounter.Attach(balanceText);
            CachePanelHome();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void Awake()
        {
            CachePanelHome();
            totalCounter.Attach(totalText);
            balanceCounter.Attach(balanceText);
        }

        private void OnDisable()
        {
            // Sair da cena no meio da cascata não pode deixar linhas invisíveis
            // nem contadores parados num número intermediário.
            HudFx.FinishAll(handles);
            HudFx.Finish(ref medalPunchHandle);
            HudFx.Finish(ref medalPopHandle);
            totalCounter.Stop();
            balanceCounter.Stop();
        }

        /// <summary>
        /// Apresenta o resultado do turno. <paramref name="statement"/> pode vir
        /// nulo quando a cena não tem SaveManager (fase aberta isolada) — nesse
        /// caso o extrato some e o resto da tela continua válido.
        /// </summary>
        public void Show(
            string levelTitle,
            IReadOnlyList<CreditEntry> statement,
            int creditsEarned,
            int creditBalance,
            int medal,
            bool assisted)
        {
            if (group == null && panel == null)
            {
                return;
            }

            HudFx.AbortAll(handles);
            HudFx.Abort(ref medalPunchHandle);
            HudFx.Abort(ref medalPopHandle);
            CachePanelHome();
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(levelTitle) ? "TURNO ENCERRADO" : levelTitle;
            }

            AnimatePanelEntrance();
            AnimateMedal(medal, assisted);

            IReadOnlyList<ShiftReportLine> lines = ShiftReportPresenter.BuildLines(statement, ShiftCredits.CapFor(medal));
            float lastLineDelay = AnimateStatement(lines);

            AnimateTotals(creditsEarned, creditBalance, lastLineDelay);
        }

        /// <summary>Recolhe o painel — usado ao reiniciar a fase.</summary>
        public void Hide()
        {
            HudFx.AbortAll(handles);
            HudFx.Abort(ref medalPunchHandle);
            HudFx.Abort(ref medalPopHandle);
            totalCounter.Stop();
            balanceCounter.Stop();

            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void AnimatePanelEntrance()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
                HudFx.Track(handles, UIMotion.FadeTo(group, 1f, panelDuration * 0.8f, Ease.OutQuad));
            }

            if (panel == null)
            {
                return;
            }

            panel.anchoredPosition = panelHome;
            HudFx.Track(handles, UIMotion.SlideIn(panel, new Vector2(0f, -64f), panelDuration, Ease.OutCubic));
        }

        private void AnimateMedal(int medal, bool assisted)
        {
            if (medalText != null)
            {
                medalText.text = ShiftReportPresenter.MedalLabel(medal);
                Color medalColor = HudPalette.Medal(medal);
                medalText.color = HudPalette.WithAlpha(medalColor, 0f);
                HudFx.Track(handles, UIMotion.FadeTo(medalText, 1f, 0.3f, Ease.OutQuad, 0.16f));
                HudFx.PopIn(ref medalPopHandle, medalText.transform, 0.42f, 0.55f, 0.16f);
                HudFx.Track(handles, HudFx.Delayed(0.58f, () => HudFx.Punch(ref medalPunchHandle, medalText.transform, 0.22f, 0.36f)));
            }

            if (rankingText == null)
            {
                return;
            }

            rankingText.text = ShiftReportPresenter.RankingLabel(assisted);
            Color rankingColor = assisted ? HudPalette.Amber : HudPalette.Green;
            HudFx.Track(handles, HudFx.FadeInFrom(rankingText, rankingColor, 0.3f, 0.3f));
        }

        /// <summary>Cascata do extrato. Devolve o atraso da última linha exibida.</summary>
        private float AnimateStatement(IReadOnlyList<ShiftReportLine> lines)
        {
            const float firstLineDelay = 0.42f;
            float lastDelay = firstLineDelay;

            for (int i = 0; i < lineLabels.Count; i++)
            {
                Text label = lineLabels[i];
                if (label == null)
                {
                    continue;
                }

                if (i >= lines.Count)
                {
                    label.gameObject.SetActive(false);
                    continue;
                }

                ShiftReportLine line = lines[i];
                label.gameObject.SetActive(true);
                label.text = $"{line.Label}   {line.AmountText}";

                Color rest = line.IsDeduction ? HudPalette.Red : HudPalette.TextPrimary;
                float delay = firstLineDelay + i * lineInterval;
                lastDelay = delay;

                label.transform.localScale = Vector3.one;
                HudFx.Track(handles, HudFx.FadeInFrom(label, rest, 0.26f, delay));
                HudFx.Track(handles, UIMotion.PopIn(label.transform, 0.28f, 0.9f, delay));
            }

            return lastDelay;
        }

        private void AnimateTotals(int creditsEarned, int creditBalance, float lastLineDelay)
        {
            float totalDelay = lastLineDelay + 0.24f;

            if (totalText != null)
            {
                totalCounter.Attach(totalText);
                totalCounter.SetImmediate(0);
                totalText.color = HudPalette.Green;
                HudFx.Track(handles, HudFx.Delayed(totalDelay, () => totalCounter.Set(creditsEarned)));
            }

            if (balanceText == null)
            {
                return;
            }

            balanceCounter.Attach(balanceText);
            // O saldo sobe a partir do valor anterior ao turno para o jogador ver
            // exatamente quanto este turno somou à conta da Oficina N-8.
            balanceCounter.SetImmediate(Mathf.Max(0, creditBalance - creditsEarned));
            balanceText.color = HudPalette.TextMuted;
            HudFx.Track(handles, HudFx.Delayed(totalDelay + 0.18f, () => balanceCounter.Set(creditBalance)));
        }

        private void CachePanelHome()
        {
            if (panelHomeCached || panel == null)
            {
                return;
            }

            panelHome = panel.anchoredPosition;
            panelHomeCached = true;
        }
    }
}
