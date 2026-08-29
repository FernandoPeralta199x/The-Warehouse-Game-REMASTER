using System.Collections.Generic;
using System.Linq;
using TW08.Core;
using TW08.Economy;
using TW08.Save;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>Uma linha da vitrine: a ferramenta, o botão de compra e o de equipar.</summary>
    [System.Serializable]
    public sealed class ShopRow
    {
        public PuzzleToolDefinition tool;
        public Text nameText;
        public Text detailText;
        public Button buyButton;
        public Button equipButton;
    }

    /// <summary>
    /// Oficina N-8: compra de ferramentas com Créditos de Turno e escolha do
    /// loadout do próximo turno.
    ///
    /// Comprar e equipar são passos separados de propósito: ter a ferramenta no
    /// estoque não a coloca em campo, e o número de slots é o que segura o
    /// equilíbrio das fases.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopController : MonoBehaviour
    {
        [SerializeField] private PuzzleToolCatalog catalog;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private List<ShopRow> rows = new();
        [SerializeField] private Text creditsText;
        [SerializeField] private Text slotsText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button backButton;
        [SerializeField] private string backSceneName = "TW08_ModeSelect";

        public void Configure(
            PuzzleToolCatalog toolCatalog,
            IEnumerable<ShopRow> shopRows,
            Text credits,
            Text slots,
            Text feedback,
            Button back,
            string backScene)
        {
            catalog = toolCatalog;
            rows = new List<ShopRow>(shopRows ?? new List<ShopRow>());
            creditsText = credits;
            slotsText = slots;
            feedbackText = feedback;
            backButton = back;
            backSceneName = string.IsNullOrWhiteSpace(backScene) ? "TW08_ModeSelect" : backScene;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            if (saveManager == null)
            {
                saveManager = FindFirstObjectByType<SaveManager>();
            }

            for (int i = 0; i < rows.Count; i++)
            {
                int index = i;
                rows[i]?.buyButton?.onClick.AddListener(() => Buy(index));
                rows[i]?.equipButton?.onClick.AddListener(() => ToggleEquip(index));
            }

            backButton?.onClick.AddListener(GoBack);
            Refresh();
        }

        private void OnDisable()
        {
            foreach (ShopRow row in rows)
            {
                row?.buyButton?.onClick.RemoveAllListeners();
                row?.equipButton?.onClick.RemoveAllListeners();
            }

            backButton?.onClick.RemoveListener(GoBack);
        }

        private void Buy(int index)
        {
            PuzzleToolDefinition tool = ToolAt(index);
            if (tool == null || saveManager?.Data == null)
            {
                return;
            }

            if (!saveManager.TryPurchaseTool(tool))
            {
                Feedback($"Créditos insuficientes: faltam {tool.Price - saveManager.Data.credits}.");
                Refresh();
                return;
            }

            Feedback($"{tool.DisplayName} adquirida.");
            Refresh();
        }

        private void ToggleEquip(int index)
        {
            PuzzleToolDefinition tool = ToolAt(index);
            if (tool == null || saveManager?.Data == null || catalog == null)
            {
                return;
            }

            List<string> equipped = new(saveManager.Data.equippedTools);
            bool isEquipped = equipped.Any(
                id => string.Equals(id, tool.ToolId, System.StringComparison.OrdinalIgnoreCase));

            if (isEquipped)
            {
                equipped.RemoveAll(id => string.Equals(id, tool.ToolId, System.StringComparison.OrdinalIgnoreCase));
                Feedback($"{tool.DisplayName} removida do turno.");
            }
            else
            {
                if (saveManager.Data.GetToolCount(tool.ToolId) <= 0)
                {
                    Feedback("Compre a ferramenta antes de equipá-la.");
                    return;
                }

                if (equipped.Count >= catalog.EquipSlots)
                {
                    Feedback($"Slots cheios ({catalog.EquipSlots}). Remova uma ferramenta primeiro.");
                    return;
                }

                equipped.Add(tool.ToolId);
                Feedback($"{tool.DisplayName} pronta para o turno.");
            }

            saveManager.SetEquippedTools(equipped, catalog.EquipSlots);
            Refresh();
        }

        private PuzzleToolDefinition ToolAt(int index)
        {
            return index >= 0 && index < rows.Count ? rows[index]?.tool : null;
        }

        private void Refresh()
        {
            SaveGameData data = saveManager?.Data;
            if (creditsText != null)
            {
                creditsText.text = $"CRÉDITOS DE TURNO // {data?.credits ?? 0}";
            }

            if (slotsText != null && catalog != null)
            {
                int used = data?.equippedTools?.Count ?? 0;
                slotsText.text = $"SLOTS DE FERRAMENTA // {used}/{catalog.EquipSlots}";
            }

            foreach (ShopRow row in rows)
            {
                RefreshRow(row, data);
            }
        }

        private void RefreshRow(ShopRow row, SaveGameData data)
        {
            if (row?.tool == null)
            {
                return;
            }

            int owned = data?.GetToolCount(row.tool.ToolId) ?? 0;
            bool equipped = data?.equippedTools?.Any(
                id => string.Equals(id, row.tool.ToolId, System.StringComparison.OrdinalIgnoreCase)) ?? false;

            if (row.nameText != null)
            {
                row.nameText.text = row.tool.DisplayName.ToUpperInvariant();
            }

            if (row.detailText != null)
            {
                row.detailText.text = $"{row.tool.Description}\nESTOQUE {owned}   PREÇO {row.tool.Price}";
            }

            if (row.buyButton != null)
            {
                row.buyButton.interactable = data != null && data.credits >= row.tool.Price;
                Text label = row.buyButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = $"COMPRAR // {row.tool.Price}";
                }
            }

            if (row.equipButton != null)
            {
                row.equipButton.interactable = owned > 0 || equipped;
                Text label = row.equipButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = equipped ? "EQUIPADA" : "EQUIPAR";
                }
            }
        }

        private void Feedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }
        }

        public void GoBack()
        {
            SceneLoader.TryLoadImmediate(backSceneName, "central de operações");
        }
    }
}
