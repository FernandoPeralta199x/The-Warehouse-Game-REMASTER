using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Menus
{
    /// <summary>
    /// Movimento contínuo e discreto do fundo dos menus: a grade do terminal
    /// deriva devagar e uma varredura desce a tela, como um CRT que nunca desliga.
    ///
    /// Não usa o serviço de tween porque não há fim: é um loop de <c>Update</c> em
    /// tempo não-escalado. Cor só é escrita quando muda de verdade — a grade gera
    /// mais de uma centena de quads e remontar essa malha todo frame seria
    /// desperdício num menu parado.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuBackdropAnimator : MonoBehaviour
    {
        private const float ColorEpsilon = 0.002f;

        [SerializeField] private RectTransform drift;
        [SerializeField] private Graphic driftGraphic;
        [SerializeField] private RectTransform sweep;
        [SerializeField] private Graphic sweepGraphic;
        [SerializeField] private Vector2 driftAmplitude = new(24f, 16f);
        [SerializeField, Min(1f)] private float driftPeriod = 19f;
        [SerializeField, Range(0f, 1f)] private float pulseMin = 0.035f;
        [SerializeField, Range(0f, 1f)] private float pulseMax = 0.085f;
        [SerializeField, Min(0.5f)] private float pulsePeriod = 7.5f;
        [SerializeField, Min(1f)] private float sweepTravel = 1180f;
        [SerializeField, Min(0.5f)] private float sweepPeriod = 9f;
        [SerializeField, Range(0f, 1f)] private float sweepAlpha = 0.055f;

        private Vector2 driftOrigin;
        private Vector2 sweepOrigin;
        private float phase;

        public void Configure(
            RectTransform gridRect,
            Graphic gridGraphic,
            RectTransform sweepRect,
            Graphic sweepBar)
        {
            drift = gridRect;
            driftGraphic = gridGraphic;
            sweep = sweepRect;
            sweepGraphic = sweepBar;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void Awake()
        {
            if (drift != null)
            {
                driftOrigin = drift.anchoredPosition;
            }

            if (sweep != null)
            {
                sweepOrigin = sweep.anchoredPosition;
            }
        }

        private void OnDisable()
        {
            // Devolver a posição inicial evita que a cena salve — ou reapareça —
            // com a grade deslocada num ponto arbitrário do ciclo.
            if (drift != null)
            {
                drift.anchoredPosition = driftOrigin;
            }

            if (sweep != null)
            {
                sweep.anchoredPosition = sweepOrigin;
            }
        }

        private void Update()
        {
            phase += Time.unscaledDeltaTime;

            if (drift != null)
            {
                float x = Mathf.Sin(phase * Mathf.PI * 2f / driftPeriod) * driftAmplitude.x;
                float y = Mathf.Cos(phase * Mathf.PI * 2f / (driftPeriod * 1.37f)) * driftAmplitude.y;
                drift.anchoredPosition = driftOrigin + new Vector2(x, y);
            }

            if (driftGraphic != null)
            {
                float t = 0.5f + 0.5f * Mathf.Sin(phase * Mathf.PI * 2f / pulsePeriod);
                ApplyAlpha(driftGraphic, Mathf.Lerp(pulseMin, pulseMax, t));
            }

            if (sweep == null)
            {
                return;
            }

            float progress = Mathf.Repeat(phase / sweepPeriod, 1f);
            sweep.anchoredPosition = sweepOrigin + new Vector2(0f, -progress * sweepTravel);

            if (sweepGraphic != null)
            {
                // Some nas pontas do trajeto para a barra não "nascer" e "morrer"
                // cortada na borda da tela.
                ApplyAlpha(sweepGraphic, Mathf.Sin(progress * Mathf.PI) * sweepAlpha);
            }
        }

        private static void ApplyAlpha(Graphic graphic, float alpha)
        {
            Color current = graphic.color;
            if (Mathf.Abs(current.a - alpha) < ColorEpsilon)
            {
                return;
            }

            current.a = alpha;
            graphic.color = current;
        }
    }
}
