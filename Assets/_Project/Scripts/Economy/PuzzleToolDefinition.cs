using UnityEngine;

namespace TW08.Economy
{
    /// <summary>
    /// Ferramenta comprável na Oficina N-8. O preço e a raridade seguem a tabela
    /// da bíblia de design; <see cref="UsesPerLevel"/> é o limite por turno.
    /// </summary>
    [CreateAssetMenu(menuName = "TW08/Economy/Puzzle Tool", fileName = "PuzzleTool")]
    public sealed class PuzzleToolDefinition : ScriptableObject
    {
        [SerializeField] private string toolId = "rewind-move";
        [SerializeField] private PuzzleToolKind kind = PuzzleToolKind.RewindMove;
        [SerializeField] private PuzzleToolRarity rarity = PuzzleToolRarity.Common;
        [SerializeField] private string displayName = "Rebobinar Movimento";
        [SerializeField] private string shortLabel = "REBOBINAR";
        [SerializeField, TextArea(2, 4)] private string description =
            "Desfaz os últimos 3 movimentos de uma vez.";
        [SerializeField, Min(0)] private int price = 50;
        [SerializeField, Min(1)] private int usesPerLevel = 1;
        [SerializeField] private Sprite icon;

        public string ToolId => toolId;
        public PuzzleToolKind Kind => kind;
        public PuzzleToolRarity Rarity => rarity;
        public string DisplayName => displayName;
        public string ShortLabel => string.IsNullOrWhiteSpace(shortLabel) ? displayName : shortLabel;
        public string Description => description;
        public int Price => Mathf.Max(0, price);
        public int UsesPerLevel => Mathf.Max(1, usesPerLevel);
        public Sprite Icon => icon;
    }
}
