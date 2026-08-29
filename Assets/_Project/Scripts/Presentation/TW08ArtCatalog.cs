using UnityEngine;

namespace TW08.Presentation
{
    [CreateAssetMenu(fileName = "TW08ArtCatalog", menuName = "TW08/Art/Production Art Catalog")]
    public sealed class TW08ArtCatalog : ScriptableObject
    {
        [Header("Characters")]
        [SerializeField] private DirectionalSpriteSet john;

        [Header("Puzzle Environment")]
        [SerializeField] private Sprite floorPrimary;
        [SerializeField] private Sprite floorSecondary;
        [SerializeField] private Sprite wall;
        [SerializeField] private Sprite goal;
        [SerializeField] private Sprite crateDefault;
        [Tooltip("Seta assimétrica. Rotacionar o sprite de alvo não indicava direção alguma.")]
        [SerializeField] private Sprite directionArrow;
        [Tooltip("Bloco cinza. Base de tudo que recebe tinta — tinta é multiplicativa.")]
        [SerializeField] private Sprite neutralBlock;

        [Header("UI")]
        [SerializeField] private Sprite terminalFrame;
        [SerializeField] private Sprite warningIcon;

        public DirectionalSpriteSet John => john;
        public Sprite FloorPrimary => floorPrimary;
        public Sprite FloorSecondary => floorSecondary;
        public Sprite Wall => wall;
        public Sprite Goal => goal;
        public Sprite CrateDefault => crateDefault;

        /// <summary>Seta de direção; volta ao alvo se a arte ainda não existir.</summary>
        public Sprite DirectionArrow => directionArrow != null ? directionArrow : goal;

        /// <summary>Bloco neutro para tingir; volta à parede se ainda não existir.</summary>
        public Sprite NeutralBlock => neutralBlock != null ? neutralBlock : wall;
        public Sprite TerminalFrame => terminalFrame;
        public Sprite WarningIcon => warningIcon;
    }
}
