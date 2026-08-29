using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TW08.Economy
{
    /// <summary>Catálogo da Oficina N-8: tudo que a loja pode oferecer.</summary>
    [CreateAssetMenu(menuName = "TW08/Economy/Puzzle Tool Catalog", fileName = "TW08_ToolCatalog")]
    public sealed class PuzzleToolCatalog : ScriptableObject
    {
        [SerializeField] private List<PuzzleToolDefinition> tools = new();
        [SerializeField, Min(1)] private int equipSlots = 2;

        public IReadOnlyList<PuzzleToolDefinition> Tools => tools;

        /// <summary>Quantas ferramentas o jogador pode levar por turno (MVP: 2).</summary>
        public int EquipSlots => Mathf.Max(1, equipSlots);

        public PuzzleToolDefinition Find(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId))
            {
                return null;
            }

            return tools.FirstOrDefault(
                tool => tool != null && string.Equals(tool.ToolId, toolId, System.StringComparison.OrdinalIgnoreCase));
        }

        public PuzzleToolDefinition Find(PuzzleToolKind kind)
        {
            return tools.FirstOrDefault(tool => tool != null && tool.Kind == kind);
        }
    }
}
