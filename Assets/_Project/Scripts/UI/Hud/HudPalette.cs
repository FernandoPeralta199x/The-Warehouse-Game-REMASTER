using UnityEngine;

namespace TW08.UI.Hud
{
    /// <summary>
    /// Paleta do HUD disponível em tempo de execução.
    ///
    /// Os builders de cena usam <c>TW08ProductionSceneUtility</c>, que vive na
    /// assembly de editor e não existe no player. Estes valores repetem os
    /// mesmos tons para que a animação em runtime volte exatamente à cor que a
    /// cena construiu.
    /// </summary>
    public static class HudPalette
    {
        public static readonly Color Panel = new(0.035f, 0.050f, 0.055f, 0.97f);
        public static readonly Color PanelLight = new(0.055f, 0.075f, 0.080f, 0.98f);
        public static readonly Color Green = new(0.25f, 0.95f, 0.58f, 1f);
        public static readonly Color Amber = new(1f, 0.63f, 0.12f, 1f);
        public static readonly Color Cyan = new(0.26f, 0.84f, 0.92f, 1f);
        public static readonly Color Red = new(0.96f, 0.28f, 0.22f, 1f);
        public static readonly Color TextPrimary = new(0.87f, 0.96f, 0.91f, 1f);
        public static readonly Color TextMuted = new(0.47f, 0.64f, 0.57f, 1f);

        /// <summary>Cor da medalha conquistada: 3 platina, 2 ouro, 1 bronze.</summary>
        public static Color Medal(int medal)
        {
            return medal switch
            {
                3 => new Color(0.78f, 0.92f, 1f, 1f),
                2 => new Color(1f, 0.84f, 0.32f, 1f),
                1 => new Color(0.86f, 0.58f, 0.34f, 1f),
                _ => TextMuted
            };
        }

        /// <summary>A mesma cor com alfa trocado — usado para preparar um fade de entrada.</summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
