using System.Collections.Generic;
using System.Linq;
using TW08.Economy;
using TW08.Motion;
using TW08.Save;
using TW08.UI.Menus;
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

        /// <summary>Painel da linha — alvo do pulso de compra e do tremor de recusa.</summary>
        public RectTransform rowRoot;
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
        /// <summary>Formato do saldo. Usado também pelo contador animado.</summary>
        public const string CreditsFormat = "CRÉDITOS DE TURNO // {0}";

        public static readonly Color EquippedTint = new(0.25f, 0.95f, 0.58f, 1f);
        public static readonly Color EquipAvailableTint = new(0.26f, 0.84f, 0.92f, 1f);
        public static readonly Color AffordableTint = new(0.25f, 0.95f, 0.58f, 1f);
        public static readonly Color UnaffordableTint = new(0.96f, 0.42f, 0.36f, 1f);

        [SerializeField] private PuzzleToolCatalog catalog;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private List<ShopRow> rows = new();
        [SerializeField] private Text creditsText;
        [SerializeField] private Text slotsText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button backButton;
        [SerializeField] private string backSceneName = "TW08_ModeSelect";

        private readonly List<MotionHandle> handles = new();
        private MotionHandle creditsHandle;
        private int shownCredits;
        private bool creditsInitialized;

        // ----------------------------------------------------- Regras puras --

        public static bool CanAfford(int credits, int price) => credits >= price;

        public static int MissingCredits(int credits, int price) => Mathf.Max(0, price - credits);

        public static string FormatCredits(int credits) => string.Format(CreditsFormat, credits);

        public static string FormatSlots(int used, int total) =>
            $"SLOTS DE FERRAMENTA // {used}/{Mathf.Max(1, total)}";

        public static string EquipLabel(bool equipped) => equipped ? "EQUIPADA" : "EQUIPAR";

        public static string BuyLabel(int price) => $"COMPRAR // {price}";

        public static string DetailLine(string description, int owned, int price) =>
            $"{description}\nESTOQUE {owned}   PREÇO {price}";

        /// <summary>Equipar exige estoque, ou já estar equipada (para desequipar).</summary>
        public static bool CanEquipRow(int owned, bool equipped) => owned > 0 || equipped;

        public static bool HasFreeSlot(int equippedCount, int slots) => equippedCount < Mathf.Max(1, slots);

        // ---------------------------------------------------------- Ciclo --

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
            creditsInitialized = false;
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

            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();
            creditsHandle?.Complete();
            creditsHandle = null;
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
                Feedback($"Créditos insuficientes: faltam {MissingCredits(saveManager.Data.credits, tool.Price)}.");
                DenyRow(index);
                Refresh();
                return;
            }

            Feedback($"{tool.DisplayName} adquirida.");
            PunchRow(index);
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
                    DenyRow(index);
                    return;
                }

                if (!HasFreeSlot(equipped.Count, catalog.EquipSlots))
                {
                    Feedback($"Slots cheios ({catalog.EquipSlots}). Remova uma ferramenta primeiro.");
                    DenyRow(index);
                    return;
                }

                equipped.Add(tool.ToolId);
                Feedback($"{tool.DisplayName} pronta para o turno.");
            }

            saveManager.SetEquippedTools(equipped, catalog.EquipSlots);
            PunchRow(index);
            Refresh();
        }

        private PuzzleToolDefinition ToolAt(int index)
        {
            return index >= 0 && index < rows.Count ? rows[index]?.tool : null;
        }

        private void PunchRow(int index)
        {
            if (index < 0 || index >= rows.Count || rows[index] == null)
            {
                return;
            }

            Transform target = rows[index].rowRoot != null
                ? rows[index].rowRoot
                : rows[index].buyButton != null ? rows[index].buyButton.transform : null;
            if (target != null)
            {
                Track(UIMotion.Punch(target, 0.05f, 0.3f));
            }
        }

        /// <summary>
        /// Guarda o handle e descarta os que já terminaram. Sem a poda, uma sessão
        /// longa de loja acumularia handles mortos até sair da cena.
        /// </summary>
        private void Track(MotionHandle handle)
        {
            handles.RemoveAll(item => item == null || !item.IsPlaying);
            if (handle != null && handle.IsPlaying)
            {
                handles.Add(handle);
            }
        }

        private void DenyRow(int index)
        {
            if (index < 0 || index >= rows.Count || rows[index] == null)
            {
                return;
            }

            RectTransform target = rows[index].rowRoot;
            if (target != null)
            {
                Track(UIMotion.Shake(target, 11f, 0.34f));
            }
            else
            {
                MenuFeedback.Denied(rows[index].buyButton);
            }

            if (creditsText != null)
            {
                Track(UIMotion.Shake(creditsText.rectTransform, 7f, 0.3f));
            }
        }

        private void Refresh()
        {
            SaveGameData data = saveManager?.Data;
            int credits = data?.credits ?? 0;

            if (creditsText != null)
            {
                if (!creditsInitialized)
                {
                    // Primeira exibição: o saldo sobe do zero, como um mostrador ligando.
                    creditsInitialized = true;
                    shownCredits = 0;
                }

                if (shownCredits != credits)
                {
                    creditsHandle?.Complete();
                    creditsHandle = UIMotion.CountTo(
                        creditsText, shownCredits, credits, 0.55f, CreditsFormat, Ease.OutCubic);
                    shownCredits = credits;
                }
                else
                {
                    creditsText.text = FormatCredits(credits);
                }
            }

            if (slotsText != null && catalog != null)
            {
                int used = data?.equippedTools?.Count ?? 0;
                slotsText.text = FormatSlots(used, catalog.EquipSlots);
            }

            foreach (ShopRow row in rows)
            {
                RefreshRow(row, data);
            }

            // O foco guarda a cor base dos rótulos; sem este aviso ele puxaria
            // "EQUIPADA" de volta para o tom antigo.
            MenuFocusAnimator.RefreshAll();
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
            bool affordable = data != null && CanAfford(data.credits, row.tool.Price);

            if (row.nameText != null)
            {
                row.nameText.text = row.tool.DisplayName.ToUpperInvariant();
            }

            if (row.detailText != null)
            {
                row.detailText.text = DetailLine(row.tool.Description, owned, row.tool.Price);
            }

            if (row.buyButton != null)
            {
                row.buyButton.interactable = affordable;
                Text label = row.buyButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = BuyLabel(row.tool.Price);
                    label.color = affordable ? AffordableTint : UnaffordableTint;
                }
            }

            if (row.equipButton != null)
            {
                row.equipButton.interactable = CanEquipRow(owned, equipped);
                Text label = row.equipButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    // A cor é escrita antes do RefreshAll para o foco adotá-la como
                    // base. Um flash em cima dela seria absorvido como cor "normal".
                    label.text = EquipLabel(equipped);
                    label.color = equipped ? EquippedTint : EquipAvailableTint;
                }
            }
        }

        private void Feedback(string message)
        {
            if (feedbackText == null)
            {
                return;
            }

            Track(UIMotion.Typewriter(feedbackText, message, 78f));
        }

        public void GoBack()
        {
            MenuTransition.Go(backSceneName, "central de operações");
        }
    }
}
