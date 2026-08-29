using System.Collections.Generic;
using TW08.Audio;
using TW08.Motion;
using TW08.Puzzle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TW08.UI.Menus
{
    /// <summary>Um ponto do mapa: a fase, o botão e os enfeites que reagem ao estado.</summary>
    [System.Serializable]
    public sealed class WarehouseMapNode
    {
        public Button button;
        public RectTransform root;
        public Image icon;
        public Image ring;
        public Text label;
        public Text medal;
        public int index;
    }

    /// <summary>
    /// Mapa de progressão da campanha: a planta do armazém, com uma marca por
    /// fase ligada pela trilha do turno.
    ///
    /// Substitui a grade de cartões. Além de ler melhor, é o que a própria
    /// história pede — a bíblia diz que as caixas "formavam um mapa", e o
    /// jogador atravessa o armazém em vez de escolher numa lista.
    ///
    /// A navegação anda pela trilha, não pela grade: esquerda e direita seguem a
    /// ordem das fases, que é a ordem em que o armazém é percorrido.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WarehouseMapController : MonoBehaviour
    {
        public static readonly Color LockedTint = new(0.30f, 0.36f, 0.34f, 1f);
        public static readonly Color AvailableTint = new(0.26f, 0.84f, 0.92f, 1f);
        public static readonly Color CurrentTint = new(1f, 0.63f, 0.12f, 1f);
        public static readonly Color BronzeTint = new(0.86f, 0.60f, 0.36f, 1f);
        public static readonly Color GoldTint = new(1f, 0.84f, 0.32f, 1f);
        public static readonly Color PlatinumTint = new(0.66f, 0.96f, 1f, 1f);

        [SerializeField] private PuzzleCampaignDefinition campaign;
        [SerializeField] private List<WarehouseMapNode> nodes = new();
        [SerializeField] private Text titleText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text operatorText;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private Button back;
        [SerializeField] private string backScene = "TW08_ModeSelect";

        private readonly List<MotionHandle> handles = new();
        private MotionHandle pulseHandle;
        private int selected = -1;
        private int nextUnlocked;

        // ------------------------------------------------------ Regras puras --

        /// <summary>Cor da marca por estado. O empate resolve pela medalha.</summary>
        public static Color NodeTint(bool unlocked, int medal, bool isNext)
        {
            if (!unlocked) return LockedTint;
            if (medal >= 3) return PlatinumTint;
            if (medal == 2) return GoldTint;
            if (medal == 1) return BronzeTint;
            return isNext ? CurrentTint : AvailableTint;
        }

        /// <summary>Marca da medalha. Fase sem medalha não mostra nada.</summary>
        public static string MedalMark(int medal)
        {
            return medal switch
            {
                3 => "◆◆◆",
                2 => "◆◆",
                1 => "◆",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Linha de detalhe da fase em foco.
        /// Mostra o melhor resultado quando existe, e o alvo de platina quando não.
        /// </summary>
        public static string DetailLine(
            string displayName, string sector, bool unlocked, int bestMoves, int platinum)
        {
            if (!unlocked)
            {
                return "ROTA BLOQUEADA // CONCLUA A ANTERIOR";
            }

            string name = string.IsNullOrWhiteSpace(displayName) ? "ROTA" : displayName.ToUpperInvariant();
            string head = $"{sector} // {name}";
            return bestMoves > 0
                ? $"{head}   —   MELHOR {bestMoves:000}"
                : $"{head}   —   PLATINA EM {platinum:000}";
        }

        // ----------------------------------------------------------- Ciclo --

        public void Configure(
            PuzzleCampaignDefinition campaignDefinition,
            IEnumerable<WarehouseMapNode> mapNodes,
            Text title,
            Text detail,
            Text operatorLabel,
            ScrollRect scrollRect,
            Button backButton,
            string backSceneName)
        {
            campaign = campaignDefinition;
            nodes = new List<WarehouseMapNode>(mapNodes ?? new List<WarehouseMapNode>());
            titleText = title;
            detailText = detail;
            operatorText = operatorLabel;
            scroll = scrollRect;
            back = backButton;
            backScene = string.IsNullOrWhiteSpace(backSceneName) ? "TW08_ModeSelect" : backSceneName;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                WarehouseMapNode node = nodes[i];
                if (node?.button == null)
                {
                    continue;
                }

                int index = i;
                node.button.onClick.RemoveAllListeners();
                node.button.onClick.AddListener(() => Enter(index));
            }

            back?.onClick.AddListener(GoBack);
            Refresh();
            FocusOnNextUnlocked();
        }

        private void OnDisable()
        {
            foreach (WarehouseMapNode node in nodes)
            {
                node?.button?.onClick.RemoveAllListeners();
            }

            back?.onClick.RemoveListener(GoBack);

            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();
            pulseHandle?.Complete();
        }

        private void Update()
        {
            // O foco pode mudar por mouse, teclado ou gamepad; ler do EventSystem
            // cobre os três sem um listener por marca.
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return;
            }

            GameObject current = eventSystem.currentSelectedGameObject;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i]?.button != null && nodes[i].button.gameObject == current && selected != i)
                {
                    selected = i;
                    OnNodeFocused(i);
                    return;
                }
            }
        }

        private void Refresh()
        {
            if (campaign == null)
            {
                return;
            }

            nextUnlocked = 0;
            for (int i = 0; i < campaign.Levels.Count; i++)
            {
                if (!PuzzleProgressStore.IsUnlocked(campaign, i))
                {
                    break;
                }

                PuzzleCampaignEntry entry = campaign.Levels[i];
                if (entry?.Level != null && !PuzzleProgressStore.IsCompleted(entry.Level.LevelId))
                {
                    nextUnlocked = i;
                    break;
                }

                nextUnlocked = i;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                RefreshNode(nodes[i], i);
            }

            if (operatorText != null)
            {
                operatorText.text = "OPERADOR // " + Core.CharacterSelectionState.SelectedCharacterId.ToUpperInvariant();
            }
        }

        private void RefreshNode(WarehouseMapNode node, int index)
        {
            if (node == null || campaign == null || index >= campaign.Levels.Count)
            {
                return;
            }

            PuzzleCampaignEntry entry = campaign.Levels[index];
            PuzzleLevelDefinition level = entry?.Level;
            bool unlocked = PuzzleProgressStore.IsUnlocked(campaign, index);
            int medal = level != null ? PuzzleProgressStore.GetMedal(level.LevelId) : 0;
            bool isNext = index == nextUnlocked;

            Color tint = NodeTint(unlocked, medal, isNext);

            if (node.icon != null) node.icon.color = tint;
            if (node.ring != null) node.ring.color = new Color(tint.r, tint.g, tint.b, isNext ? 0.55f : 0.18f);
            if (node.label != null) node.label.color = unlocked ? tint : LockedTint;
            if (node.medal != null)
            {
                node.medal.text = MedalMark(medal);
                node.medal.color = tint;
            }

            if (node.button != null)
            {
                node.button.interactable = unlocked;
            }

            // A fase da vez respira: numa planta com 28 marcas o jogador precisa
            // achar onde continuar sem ler todas.
            if (isNext && node.root != null && Application.isPlaying)
            {
                pulseHandle?.Kill();
                pulseHandle = UIMotion.Punch(node.root, 0.16f, 0.9f);
            }
        }

        private void OnNodeFocused(int index)
        {
            if (campaign == null || index < 0 || index >= campaign.Levels.Count)
            {
                return;
            }

            PuzzleCampaignEntry entry = campaign.Levels[index];
            PuzzleLevelDefinition level = entry?.Level;
            bool unlocked = PuzzleProgressStore.IsUnlocked(campaign, index);

            if (detailText != null && level != null)
            {
                detailText.text = DetailLine(
                    level.DisplayName,
                    level.SectorId,
                    unlocked,
                    PuzzleProgressStore.GetBestMoves(level.LevelId),
                    level.PlatinumMoveLimit);
            }

            if (nodes[index]?.root != null)
            {
                handles.Add(UIMotion.Punch(nodes[index].root, 0.10f, 0.22f));
            }

            ScrollTo(index);
        }

        /// <summary>Traz a marca em foco para o centro da viewport.</summary>
        private void ScrollTo(int index)
        {
            if (scroll == null || scroll.content == null || nodes[index]?.root == null)
            {
                return;
            }

            RectTransform content = scroll.content;
            float span = content.rect.width - scroll.viewport.rect.width;
            if (span <= 1f)
            {
                return;
            }

            float x = nodes[index].root.anchoredPosition.x;
            float target = Mathf.Clamp01((x - scroll.viewport.rect.width * 0.5f) / span);
            scroll.horizontalNormalizedPosition = Mathf.Lerp(
                scroll.horizontalNormalizedPosition, target, 0.35f);
        }

        private void FocusOnNextUnlocked()
        {
            if (EventSystem.current == null || nodes.Count == 0)
            {
                return;
            }

            int index = Mathf.Clamp(nextUnlocked, 0, nodes.Count - 1);
            if (nodes[index]?.button != null)
            {
                EventSystem.current.SetSelectedGameObject(nodes[index].button.gameObject);
            }
        }

        private void Enter(int index)
        {
            if (campaign == null || index < 0 || index >= campaign.Levels.Count)
            {
                return;
            }

            if (!PuzzleProgressStore.IsUnlocked(campaign, index))
            {
                MenuFeedback.Denied(nodes[index]?.button);
                return;
            }

            PuzzleCampaignEntry entry = campaign.Levels[index];
            string scene = entry != null && !string.IsNullOrWhiteSpace(entry.SceneName)
                ? entry.SceneName
                : entry?.Level != null ? entry.Level.LevelId : null;

            if (string.IsNullOrWhiteSpace(scene))
            {
                MenuFeedback.Denied(nodes[index]?.button);
                return;
            }

            GameAudio.Confirm();
            MenuTransition.Go(scene, "rota selecionada");
        }

        public void GoBack()
        {
            MenuTransition.Go(backScene, "central de operações");
        }
    }
}
