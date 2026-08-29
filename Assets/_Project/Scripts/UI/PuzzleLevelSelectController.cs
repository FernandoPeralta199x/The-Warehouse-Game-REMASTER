using System.Collections.Generic;
using System.Linq;
using TW08.Core;
using TW08.Puzzle;
using TW08.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class PuzzleLevelSelectController : MonoBehaviour
    {
        /// <summary>Cartão sem fase associada.</summary>
        public static readonly Color EmptyTint = new(0.30f, 0.36f, 0.34f, 1f);

        /// <summary>Rota ainda trancada.</summary>
        public static readonly Color LockedTint = new(0.34f, 0.44f, 0.40f, 1f);

        /// <summary>Próxima rota a jogar — destaque âmbar do terminal.</summary>
        public static readonly Color CurrentTint = new(1f, 0.63f, 0.12f, 1f);

        /// <summary>Rota liberada e ainda sem registro.</summary>
        public static readonly Color AvailableTint = new(0.26f, 0.84f, 0.92f, 1f);

        public static readonly Color BronzeTint = new(0.86f, 0.60f, 0.36f, 1f);
        public static readonly Color GoldTint = new(1f, 0.84f, 0.32f, 1f);
        public static readonly Color PlatinumTint = new(0.66f, 0.96f, 1f, 1f);

        [SerializeField] private PuzzleCampaignDefinition campaign;
        [SerializeField] private List<Button> levelButtons = new();
        [SerializeField] private Text operatorText;
        [SerializeField] private Text hintText;
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";

        private readonly List<bool> unlockedCache = new();
        private bool bound;

        /// <summary>Rótulo do cartão de fase. Regra pura, testável sem cena.</summary>
        public static string FormatLevelLabel(
            int index, string displayName, bool hasEntry, bool unlocked, int bestMoves, int medal)
        {
            if (!hasEntry)
            {
                return "--";
            }

            string suffix = unlocked
                ? (bestMoves > 0 ? $"\nBEST {bestMoves:000} // M{medal}" : "\nROTA DISPONÍVEL")
                : "\nBLOQUEADO";
            string name = string.IsNullOrWhiteSpace(displayName)
                ? $"ROTA {index + 1:00}"
                : displayName.ToUpperInvariant();
            return $"{index + 1:00} // {name}{suffix}";
        }

        /// <summary>Cor do cartão por estado de progresso. Regra pura, testável.</summary>
        public static Color LabelTint(bool hasEntry, bool unlocked, int medal, bool isNext)
        {
            if (!hasEntry) return EmptyTint;
            if (!unlocked) return LockedTint;
            if (medal >= 3) return PlatinumTint;
            if (medal == 2) return GoldTint;
            if (medal == 1) return BronzeTint;
            return isNext ? CurrentTint : AvailableTint;
        }

        public void Configure(
            PuzzleCampaignDefinition campaignDefinition,
            IEnumerable<Button> buttons,
            Text operatorLabel,
            Button back,
            string backSceneName,
            Text hintLabel = null)
        {
            Unbind();
            campaign = campaignDefinition;
            levelButtons = buttons?.Where(button => button != null).ToList() ?? new List<Button>();
            operatorText = operatorLabel;
            hintText = hintLabel;
            backButton = back;
            backScene = backSceneName;
            Bind();
            Refresh();
            MarkDirtyInEditor();
        }

        private void OnEnable()
        {
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Back()
        {
            MenuTransition.Go(backScene, "menu de modos");
        }

        private void Bind()
        {
            if (bound)
            {
                return;
            }

            for (int i = 0; i < levelButtons.Count; i++)
            {
                int captured = i;
                levelButtons[i].onClick.AddListener(() => LoadLevel(captured));
            }

            backButton?.onClick.AddListener(Back);
            bound = true;
        }

        private void Unbind()
        {
            if (!bound)
            {
                return;
            }

            foreach (Button button in levelButtons)
            {
                button?.onClick.RemoveAllListeners();
            }

            backButton?.onClick.RemoveListener(Back);
            bound = false;
        }

        private void LoadLevel(int index)
        {
            if (campaign == null || index < 0 || index >= campaign.Levels.Count)
            {
                return;
            }

            // O cartão trancado continua selecionável de propósito: a navegação por
            // teclado atravessa a grade inteira e o jogador recebe uma recusa
            // explícita em vez de um botão morto.
            if (!IsUnlockedAt(index))
            {
                Deny(index, "ROTA BLOQUEADA // CONCLUA A FASE ANTERIOR");
                return;
            }

            PuzzleCampaignEntry entry = campaign.Levels[index];
            if (entry == null || string.IsNullOrWhiteSpace(entry.SceneName))
            {
                Debug.LogError($"TW08 campaign entry {index} has no scene name.");
                Deny(index, "ROTA SEM DESTINO REGISTRADO");
                return;
            }

            if (MenuTransition.Go(entry.SceneName, $"fase {index + 1:00}"))
            {
                ShowHint($"ABRINDO ROTA {index + 1:00}...");
            }
        }

        private void Deny(int index, string message)
        {
            if (index >= 0 && index < levelButtons.Count)
            {
                MenuFeedback.Denied(levelButtons[index]);
            }

            ShowHint(message);
        }

        private void ShowHint(string message)
        {
            if (hintText != null)
            {
                hintText.text = message;
            }
        }

        private bool IsUnlockedAt(int index)
        {
            return index >= 0 && index < unlockedCache.Count
                ? unlockedCache[index]
                : PuzzleProgressStore.IsUnlocked(campaign, index);
        }

        private void Refresh()
        {
            if (operatorText != null)
            {
                operatorText.text = "OPERADOR // " + CharacterSelectionState.SelectedCharacterId.ToUpperInvariant();
            }

            unlockedCache.Clear();

            if (campaign == null)
            {
                return;
            }

            int nextIndex = -1;
            for (int i = 0; i < levelButtons.Count; i++)
            {
                bool hasEntry = i < campaign.Levels.Count
                                && campaign.Levels[i] != null
                                && campaign.Levels[i].Level != null;
                bool unlocked = hasEntry && PuzzleProgressStore.IsUnlocked(campaign, i);
                unlockedCache.Add(unlocked);

                if (nextIndex < 0 && unlocked
                    && !PuzzleProgressStore.IsCompleted(campaign.Levels[i].Level.LevelId))
                {
                    nextIndex = i;
                }
            }

            for (int i = 0; i < levelButtons.Count; i++)
            {
                Button button = levelButtons[i];
                if (button == null)
                {
                    continue;
                }

                bool hasEntry = i < campaign.Levels.Count
                                && campaign.Levels[i] != null
                                && campaign.Levels[i].Level != null;
                bool unlocked = i < unlockedCache.Count && unlockedCache[i];
                button.interactable = hasEntry;

                Text label = button.GetComponentInChildren<Text>();
                if (label == null)
                {
                    continue;
                }

                if (!hasEntry)
                {
                    label.text = FormatLevelLabel(i, null, false, false, 0, 0);
                    label.color = EmptyTint;
                    continue;
                }

                PuzzleLevelDefinition level = campaign.Levels[i].Level;
                int medal = PuzzleProgressStore.GetMedal(level.LevelId);
                int best = PuzzleProgressStore.GetBestMoves(level.LevelId);
                label.text = FormatLevelLabel(i, level.DisplayName, true, unlocked, best, medal);
                label.color = LabelTint(true, unlocked, medal, i == nextIndex);
            }

            ShowHint(nextIndex >= 0
                ? $"PRÓXIMA ROTA // {nextIndex + 1:00}"
                : "TODAS AS ROTAS DESTE SETOR ESTÃO CONCLUÍDAS.");

            // O foco guarda a cor base dos rótulos; sem este aviso ele puxaria a
            // medalha recém-pintada de volta para a cor antiga.
            MenuFocusAnimator.RefreshAll();
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}
