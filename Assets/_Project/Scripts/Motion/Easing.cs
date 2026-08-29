using UnityEngine;

namespace TW08.Motion
{
    /// <summary>Curvas de aceleração usadas por todo o movimento de interface do jogo.</summary>
    public enum Ease
    {
        Linear,
        SmoothStep,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        OutBack,
        OutElastic,
        OutBounce
    }

    /// <summary>
    /// Avaliação de easing sobre t normalizado (0..1).
    ///
    /// Funções puras e sem alocação: o serviço de movimento chama isto a cada
    /// frame de cada tween ativo.
    /// </summary>
    public static class Easing
    {
        private const float BackOvershoot = 1.70158f;

        public static float Evaluate(Ease ease, float t)
        {
            t = Mathf.Clamp01(t);

            switch (ease)
            {
                case Ease.Linear:
                    return t;

                case Ease.SmoothStep:
                    return t * t * (3f - 2f * t);

                case Ease.InQuad:
                    return t * t;

                case Ease.OutQuad:
                    return 1f - (1f - t) * (1f - t);

                case Ease.InOutQuad:
                    return t < 0.5f
                        ? 2f * t * t
                        : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;

                case Ease.InCubic:
                    return t * t * t;

                case Ease.OutCubic:
                    return 1f - Mathf.Pow(1f - t, 3f);

                case Ease.InOutCubic:
                    return t < 0.5f
                        ? 4f * t * t * t
                        : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;

                case Ease.OutBack:
                {
                    const float c3 = BackOvershoot + 1f;
                    float p = t - 1f;
                    return 1f + c3 * p * p * p + BackOvershoot * p * p;
                }

                case Ease.OutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    const float period = 2f * Mathf.PI / 3f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * period) + 1f;
                }

                case Ease.OutBounce:
                {
                    const float n1 = 7.5625f;
                    const float d1 = 2.75f;
                    if (t < 1f / d1) return n1 * t * t;
                    if (t < 2f / d1)
                    {
                        t -= 1.5f / d1;
                        return n1 * t * t + 0.75f;
                    }

                    if (t < 2.5f / d1)
                    {
                        t -= 2.25f / d1;
                        return n1 * t * t + 0.9375f;
                    }

                    t -= 2.625f / d1;
                    return n1 * t * t + 0.984375f;
                }

                default:
                    return t;
            }
        }
    }
}
