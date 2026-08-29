#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TW08.Economy;
using TW08.Save;
using TW08.UI;
using TW08.UI.Menus;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.Editor
{
    /// <summary>
    /// Materializa a Oficina N-8: os assets das ferramentas do MVP, o catálogo e
    /// a cena de loja.
    ///
    /// Preços, raridades e o conjunto de ferramentas vêm da tabela da bíblia de
    /// design (documento da Loja e Power Ups, seções 10 a 12). O MVP fica só com
    /// ferramentas que ajudam o jogador a pensar — nada que resolva a fase.
    /// </summary>
    public static class TW08ShopSetup
    {
        public const string EconomyRoot = "Assets/_Project/ScriptableObjects/Economy";
        public const string CatalogPath = EconomyRoot + "/TW08_ToolCatalog.asset";
        public const string ShopScenePath =
            TW08MenuSceneBuilder.SceneRoot + "/TW08_ShopN8.unity";

        private readonly struct ToolSpec
        {
            public ToolSpec(
                string id,
                PuzzleToolKind kind,
                PuzzleToolRarity rarity,
                string name,
                string shortLabel,
                string description,
                int price,
                int usesPerLevel)
            {
                Id = id;
                Kind = kind;
                Rarity = rarity;
                Name = name;
                ShortLabel = shortLabel;
                Description = description;
                Price = price;
                UsesPerLevel = usesPerLevel;
            }

            public string Id { get; }
            public PuzzleToolKind Kind { get; }
            public PuzzleToolRarity Rarity { get; }
            public string Name { get; }
            public string ShortLabel { get; }
            public string Description { get; }
            public int Price { get; }
            public int UsesPerLevel { get; }
        }

        private static readonly ToolSpec[] Specs =
        {
            new(
                "rewind-move",
                PuzzleToolKind.RewindMove,
                PuzzleToolRarity.Common,
                "Rebobinar Movimento",
                "REBOBINAR",
                "Desfaz os últimos 3 movimentos de uma vez.",
                50,
                1),
            new(
                "route-marker",
                PuzzleToolKind.RouteMarker,
                PuzzleToolRarity.Common,
                "Marcador de Rota",
                "MARCADOR",
                "Destaca no piso os alvos que ainda estão descobertos.",
                40,
                2),
            new(
                "logistics-scanner",
                PuzzleToolKind.LogisticsScanner,
                PuzzleToolRarity.Uncommon,
                "Scanner Logístico",
                "SCANNER",
                "Aponta a carga em situação mais crítica do setor.",
                80,
                1),
            new(
                "shift-assistant",
                PuzzleToolKind.ShiftAssistant,
                PuzzleToolRarity.Rare,
                "Assistente de Turno",
                "ASSISTENTE",
                "Dicas em camadas, da mais vaga à mais direta. Não entrega a solução.",
                150,
                3)
        };

        [MenuItem("Tools/TW08/Production/Build Shop N-8")]
        public static void BuildFromMenu()
        {
            int toolCount = EnsureCatalog().Tools.Count;
            BuildShopScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Oficina N-8",
                $"Loja materializada com {toolCount} ferramentas.",
                "OK");
        }

        /// <summary>Cria/atualiza os assets das ferramentas e o catálogo.</summary>
        public static PuzzleToolCatalog EnsureCatalog()
        {
            TW08ProductionSceneUtility.EnsureFolder(EconomyRoot);

            List<PuzzleToolDefinition> tools = new();
            foreach (ToolSpec spec in Specs)
            {
                tools.Add(EnsureTool(spec));
            }

            PuzzleToolCatalog catalog = LoadOrCreate<PuzzleToolCatalog>(CatalogPath);
            SerializedObject serialized = new(catalog);
            SerializedProperty list = serialized.FindProperty("tools");
            list.arraySize = tools.Count;
            for (int i = 0; i < tools.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = tools[i];
            }

            // MVP: 2 slots por fase, conforme a regra de equipamento antes da fase.
            serialized.FindProperty("equipSlots").intValue = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath<PuzzleToolCatalog>(CatalogPath);
        }

        private static PuzzleToolDefinition EnsureTool(ToolSpec spec)
        {
            string path = $"{EconomyRoot}/Tool_{spec.Id}.asset";
            PuzzleToolDefinition tool = LoadOrCreate<PuzzleToolDefinition>(path);
            SerializedObject serialized = new(tool);
            serialized.FindProperty("toolId").stringValue = spec.Id;
            serialized.FindProperty("kind").enumValueIndex = (int)spec.Kind;
            serialized.FindProperty("rarity").enumValueIndex = (int)spec.Rarity;
            serialized.FindProperty("displayName").stringValue = spec.Name;
            serialized.FindProperty("shortLabel").stringValue = spec.ShortLabel;
            serialized.FindProperty("description").stringValue = spec.Description;
            serialized.FindProperty("price").intValue = spec.Price;
            serialized.FindProperty("usesPerLevel").intValue = spec.UsesPerLevel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tool);
            return tool;
        }

        /// <summary>
        /// Constrói a cena da Oficina N-8.
        ///
        /// O catálogo é sempre resolvido do AssetDatabase aqui dentro, nunca
        /// recebido por parâmetro: qualquer refresh entre a criação do asset e
        /// esta chamada invalida o wrapper nativo, mesmo com o asset intacto no
        /// disco.
        /// </summary>
        public static string BuildShopScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PuzzleToolCatalog catalog = AssetDatabase.LoadAssetAtPath<PuzzleToolCatalog>(CatalogPath);
            if (catalog == null)
            {
                throw new InvalidOperationException(
                    $"Catálogo da Oficina N-8 ausente em {CatalogPath}. Rode EnsureCatalog antes.");
            }

            TW08ProductionSceneUtility.CreateCamera(new Vector3(0f, 0f, -10f), 5f);
            Canvas canvas = TW08ProductionSceneUtility.CreateCanvas();
            EventSystem eventSystem = TW08ProductionSceneUtility.CreateEventSystem();
            Image backdrop = TW08ProductionSceneUtility.CreatePanel(
                canvas.transform, "Backdrop", TW08ProductionSceneUtility.Background);
            TW08ProductionSceneUtility.Stretch(backdrop.rectTransform);

            Image panel = TW08ProductionSceneUtility.CreatePanel(
                canvas.transform, "Terminal Shell", TW08ProductionSceneUtility.Panel);
            TW08ProductionSceneUtility.SetRect(
                panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1480f, 860f), Vector2.zero);
            Transform shell = panel.transform;

            Text eyebrow = TW08ProductionSceneUtility.CreateText(
                shell, "Eyebrow", "N-8 LOGISTICS // MANUTENÇÃO", 15,
                TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(900f, 35f), new Vector2(0f, -42f));

            Text title = TW08ProductionSceneUtility.CreateText(
                shell, "Title", "OFICINA N-8", 34,
                TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                title.rectTransform, new Vector2(0.5f, 1f), new Vector2(1100f, 58f), new Vector2(0f, -88f));

            Text credits = TW08ProductionSceneUtility.CreateText(
                shell, "Credits", "CRÉDITOS DE TURNO // 0", 20,
                TW08ProductionSceneUtility.Amber, TextAnchor.MiddleLeft);
            TW08ProductionSceneUtility.SetRect(
                credits.rectTransform, new Vector2(0f, 1f), new Vector2(560f, 40f), new Vector2(70f, -140f));

            Text slots = TW08ProductionSceneUtility.CreateText(
                shell, "Slots", "SLOTS DE FERRAMENTA // 0/2", 20,
                TW08ProductionSceneUtility.Cyan, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(
                slots.rectTransform, new Vector2(1f, 1f), new Vector2(560f, 40f), new Vector2(-70f, -140f));

            List<ShopRow> rows = new();
            float top = -200f;
            const float rowHeight = 128f;

            for (int i = 0; i < catalog.Tools.Count; i++)
            {
                PuzzleToolDefinition tool = catalog.Tools[i];
                if (tool == null)
                {
                    continue;
                }

                float y = top - i * rowHeight;

                Image rowPanel = TW08ProductionSceneUtility.CreatePanel(
                    shell, $"Row {i + 1}", TW08ProductionSceneUtility.PanelLight);
                TW08ProductionSceneUtility.SetRect(
                    rowPanel.rectTransform, new Vector2(0.5f, 1f), new Vector2(1330f, 116f), new Vector2(0f, y));

                Text name = TW08ProductionSceneUtility.CreateText(
                    rowPanel.transform, "Name", tool.DisplayName.ToUpperInvariant(), 20,
                    TW08ProductionSceneUtility.TextPrimary, TextAnchor.UpperLeft);
                TW08ProductionSceneUtility.SetRect(
                    name.rectTransform, new Vector2(0f, 0.5f), new Vector2(700f, 30f), new Vector2(380f, 28f));

                Text detail = TW08ProductionSceneUtility.CreateText(
                    rowPanel.transform, "Detail", tool.Description, 15,
                    TW08ProductionSceneUtility.TextMuted, TextAnchor.UpperLeft);
                TW08ProductionSceneUtility.SetRect(
                    detail.rectTransform, new Vector2(0f, 0.5f), new Vector2(700f, 58f), new Vector2(380f, -18f));

                Button buy = TW08ProductionSceneUtility.CreateButton(
                    rowPanel.transform, "Buy", $"COMPRAR // {tool.Price}", TW08ProductionSceneUtility.Green, 16);
                TW08ProductionSceneUtility.SetRect(
                    (RectTransform)buy.transform, new Vector2(1f, 0.5f), new Vector2(240f, 52f), new Vector2(-290f, 0f));

                Button equip = TW08ProductionSceneUtility.CreateButton(
                    rowPanel.transform, "Equip", "EQUIPAR", TW08ProductionSceneUtility.Cyan, 16);
                TW08ProductionSceneUtility.SetRect(
                    (RectTransform)equip.transform, new Vector2(1f, 0.5f), new Vector2(240f, 52f), new Vector2(-30f, 0f));

                rows.Add(new ShopRow
                {
                    tool = tool,
                    nameText = name,
                    detailText = detail,
                    buyButton = buy,
                    equipButton = equip,
                    rowRoot = rowPanel.rectTransform
                });
            }

            Text feedback = TW08ProductionSceneUtility.CreateText(
                shell, "Feedback", "Selecione uma ferramenta para o próximo turno.", 17,
                TW08ProductionSceneUtility.Amber, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                feedback.rectTransform, new Vector2(0.5f, 0f), new Vector2(1200f, 40f), new Vector2(0f, 118f));

            Button back = TW08ProductionSceneUtility.CreateButton(
                shell, "Back", "VOLTAR", TW08ProductionSceneUtility.TextMuted, 15);
            TW08ProductionSceneUtility.SetRect(
                (RectTransform)back.transform, new Vector2(0f, 0f), new Vector2(180f, 50f), new Vector2(60f, 44f));

            ShopController controller = new GameObject("Shop Controller").AddComponent<ShopController>();
            controller.Configure(catalog, rows, credits, slots, feedback, back, "TW08_ModeSelect");
            UnityEventTools.AddPersistentListener(back.onClick, controller.GoBack);

            ApplyShopMotion(canvas, shell, eyebrow, title, credits, slots, rows, feedback, back);

            EnsureSaveManager();
            TW08ProductionSceneUtility.Select(eventSystem, rows.Count > 0 ? rows[0].buyButton : back);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ShopScenePath);
            return ShopScenePath;
        }

        /// <summary>
        /// Dá à Oficina a mesma linguagem de movimento das outras telas:
        /// cabeçalho digitado, linhas entrando em cascata, foco deslizante e
        /// pulso de confirmação nos botões.
        ///
        /// A cascata usa as linhas inteiras, e não os botões: a vitrine se lê
        /// por produto, então a unidade que entra em cena é a linha.
        /// </summary>
        private static void ApplyShopMotion(
            Canvas canvas,
            Transform shell,
            Text eyebrow,
            Text title,
            Text credits,
            Text slots,
            List<ShopRow> rows,
            Text feedback,
            Button back)
        {
            Color accent = TW08ProductionSceneUtility.Green;

            Image marker = TW08ProductionSceneUtility.CreatePanel(
                shell, "Focus Marker", new Color(accent.r, accent.g, accent.b, 0.18f));
            marker.raycastTarget = false;
            TW08ProductionSceneUtility.SetRect(
                marker.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(250f, 60f), Vector2.zero);
            marker.rectTransform.SetAsFirstSibling();

            MenuFocusAnimator focus = shell.gameObject.AddComponent<MenuFocusAnimator>();
            focus.Configure(shell, marker.rectTransform, marker, new Color(accent.r, accent.g, accent.b, 0.22f));

            List<Component> cascade = new() { credits, slots };
            foreach (ShopRow row in rows)
            {
                if (row?.rowRoot != null)
                {
                    cascade.Add(row.rowRoot);
                }
            }

            cascade.Add(feedback);
            cascade.Add(back);

            MenuScreenAnimator entrance = shell.gameObject.AddComponent<MenuScreenAnimator>();
            entrance.Configure(eyebrow, title, null, cascade);

            foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
            {
                if (button != null && button.GetComponent<MenuPressFeedback>() == null)
                {
                    button.gameObject.AddComponent<MenuPressFeedback>();
                }
            }

            if (canvas.GetComponent<MenuNavigationAudio>() == null)
            {
                canvas.gameObject.AddComponent<MenuNavigationAudio>();
            }

            EditorUtility.SetDirty(focus);
            EditorUtility.SetDirty(entrance);
        }

        /// <summary>
        /// A loja lê e grava o save, então precisa de um SaveManager na cena —
        /// entrar nela direto pelo menu não passa por nenhuma outra.
        /// </summary>
        private static void EnsureSaveManager()
        {
            var config = AssetDatabase.LoadAssetAtPath<TW08.Data.GameConfig>(
                "Assets/_Project/ScriptableObjects/Core/GameConfig.asset");
            SaveManager manager = new GameObject("Save Manager").AddComponent<SaveManager>();
            SerializedObject serialized = new(manager);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
