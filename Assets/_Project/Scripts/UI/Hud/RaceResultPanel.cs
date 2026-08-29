using TW08.Motion;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Hud
{
    /// <summary>
    /// Tela de resultado da corrida: medalha, tempo final e comparação com o
    /// recorde da pista.
    ///
    /// O tempo entra por máquina de escrever em vez de contador numérico porque
    /// <c>CountTo</c> interpola um inteiro, e um cronômetro interpolado passaria
    /// por tempos que nunca existiram na corrida.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaceResultPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text medalText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text bestText;
        [SerializeField] private Text cargoText;
        [SerializeField, Min(0.05f)] private float panelDuration = 0.44f;

        private MotionHandle groupHandle;
        private MotionHandle slideHandle;
        private MotionHandle medalPopHandle;
        private MotionHandle medalPunchHandle;
        private MotionHandle medalFadeHandle;
        private MotionHandle timeHandle;
        private MotionHandle bestHandle;
        private MotionHandle cargoHandle;
        private MotionHandle punchDelayHandle;

        private Vector2 panelHome;
        private bool panelHomeCached;

        public void Configure(
            CanvasGroup panelGroup,
            RectTransform panelRect,
            Text title,
            Text medal,
            Text time,
            Text best,
            Text cargo)
        {
            group = panelGroup;
            panel = panelRect;
            titleText = title;
            medalText = medal;
            timeText = time;
            bestText = best;
            cargoText = cargo;
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
        }

        private void OnDisable()
        {
            StopMotion();
        }

        public void Show(string trackName, float finishTime, float bestTime, int medal, string cargoLine)
        {
            if (group == null && panel == null)
            {
                return;
            }

            StopMotion();
            CachePanelHome();
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(trackName)
                    ? "CORRIDA ENCERRADA"
                    : trackName.Trim().ToUpperInvariant();
            }

            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
                groupHandle = UIMotion.FadeTo(group, 1f, panelDuration * 0.8f, Ease.OutQuad);
            }

            if (panel != null)
            {
                panel.anchoredPosition = panelHome;
                slideHandle = UIMotion.SlideIn(panel, new Vector2(0f, -56f), panelDuration, Ease.OutCubic);
            }

            AnimateMedal(medal);

            if (timeText != null)
            {
                timeText.color = HudPalette.Green;
                timeHandle = UIMotion.Typewriter(timeText, HudFormat.Time(finishTime), 26f, 0.34f);
            }

            if (bestText != null)
            {
                bestText.text = HudFormat.BestTime(bestTime);
                bestHandle = HudFx.FadeInFrom(bestText, HudPalette.TextMuted, 0.3f, 0.62f);
            }

            if (cargoText == null)
            {
                return;
            }

            cargoText.text = string.IsNullOrWhiteSpace(cargoLine) ? "CARGA // --" : cargoLine;
            cargoHandle = HudFx.FadeInFrom(cargoText, HudPalette.Cyan, 0.3f, 0.74f);
        }

        public void Hide()
        {
            StopMotion();
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void AnimateMedal(int medal)
        {
            if (medalText == null)
            {
                return;
            }

            medalText.text = ShiftReportPresenter.MedalLabel(medal);
            medalText.color = HudPalette.WithAlpha(HudPalette.Medal(medal), 0f);
            medalFadeHandle = UIMotion.FadeTo(medalText, 1f, 0.28f, Ease.OutQuad, 0.14f);
            HudFx.PopIn(ref medalPopHandle, medalText.transform, 0.44f, 0.55f, 0.14f);
            punchDelayHandle = HudFx.Delayed(
                0.56f,
                () => HudFx.Punch(ref medalPunchHandle, medalText.transform, 0.24f, 0.36f));
        }

        private void StopMotion()
        {
            HudFx.Finish(ref groupHandle);
            HudFx.Finish(ref slideHandle);
            HudFx.Finish(ref medalPopHandle);
            HudFx.Finish(ref medalPunchHandle);
            HudFx.Finish(ref medalFadeHandle);
            HudFx.Finish(ref timeHandle);
            HudFx.Finish(ref bestHandle);
            HudFx.Finish(ref cargoHandle);
            // O passo adiantado só dispara um pulso: concluí-lo iniciaria uma
            // animação nova no exato momento em que a tela está saindo.
            HudFx.Abort(ref punchDelayHandle);
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
