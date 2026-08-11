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

        [Header("UI")]
        [SerializeField] private Sprite terminalFrame;
        [SerializeField] private Sprite warningIcon;

        public DirectionalSpriteSet John => john;
        public Sprite FloorPrimary => floorPrimary;
        public Sprite FloorSecondary => floorSecondary;
        public Sprite Wall => wall;
        public Sprite Goal => goal;
        public Sprite CrateDefault => crateDefault;
        public Sprite TerminalFrame => terminalFrame;
        public Sprite WarningIcon => warningIcon;
    }
}
