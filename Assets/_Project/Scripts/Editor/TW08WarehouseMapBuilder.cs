#if UNITY_EDITOR
using System.Collections.Generic;
using TW08.Puzzle;
using TW08.UI;
using TW08.UI.Menus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.Editor
{
    /// <summary>
    /// Monta a cena do mapa da campanha: a planta do armazém com uma marca por
    /// fase, ligadas pela trilha do turno.
    ///
    /// As marcas serpenteiam dentro de cada setor e o setor inteiro é um bloco
    /// rotulado — a leitura é "atravessei o Recebimento, entrei na Câmara Fria",
    /// e não "cliquei no cartão 14".
    /// </summary>
    internal static class TW08WarehouseMapBuilder
    {
        internal const string MapScenePath = TW08MenuSceneBuilder.SceneRoot + "/TW08_CampaignMap.unity";

        private const float NodeSpacingX = 190f;
        private const float RowOffsetY = 128f;
        private const float SectorGap = 120f;
        private const int NodesPerRow = 3;

        internal static string Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // NewScene invalida wrappers nativos; resolve a campanha depois dela.
            PuzzleCampaignDefinition campaign =
                AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(TW08ExpansionDataSetup.PuzzleCampaignPath);
            if (campaign == null)
            {
                throw new System.InvalidOperationException(
                    $"Campanha ausente em {TW08ExpansionDataSetup.PuzzleCampaignPath}.");
            }

            TW08ProductionSceneUtility.CreateCamera(new Vector3(0f, 0f, -10f), 5f);
            Canvas canvas = TW08ProductionSceneUtility.CreateCanvas();
            EventSystem eventSystem = TW08ProductionSceneUtility.CreateEventSystem();

            Image backdrop = TW08ProductionSceneUtility.CreatePanel(
                canvas.transform, "Backdrop", TW08ProductionSceneUtility.Background);
            TW08ProductionSceneUtility.Stretch(backdrop.rectTransform);

            Text eyebrow = TW08ProductionSceneUtility.CreateText(
                canvas.transform, "Eyebrow", "N-8 LOGISTICS // PLANTA DO ARMAZÉM", 15,
                TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(900f, 30f), new Vector2(0f, -34f));

            Text title = TW08ProductionSceneUtility.CreateText(
                canvas.transform, "Title", "ROTA DO TURNO", 34,
                TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                title.rectTransform, new Vector2(0.5f, 1f), new Vector2(1100f, 52f), new Vector2(0f, -76f));

            Text operatorText = TW08ProductionSceneUtility.CreateText(
                canvas.transform, "Operator", "OPERADOR // JOHN", 16,
                TW08ProductionSceneUtility.Amber, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(
                operatorText.rectTransform, new Vector2(1f, 1f), new Vector2(460f, 30f), new Vector2(-60f, -60f));

            ScrollRect scroll = BuildViewport(canvas, out RectTransform content);
            List<WarehouseMapNode> nodes = BuildNodes(campaign, content);

            Text detail = TW08ProductionSceneUtility.CreateText(
                canvas.transform, "Detail", "SELECIONE UMA ROTA", 18,
                TW08ProductionSceneUtility.Cyan, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                detail.rectTransform, new Vector2(0.5f, 0f), new Vector2(1300f, 34f), new Vector2(0f, 118f));

            Button back = TW08ProductionSceneUtility.CreateButton(
                canvas.transform, "Back", "VOLTAR", TW08ProductionSceneUtility.TextMuted, 15);
            TW08ProductionSceneUtility.SetRect(
                (RectTransform)back.transform, new Vector2(0f, 0f), new Vector2(180f, 50f), new Vector2(70f, 52f));

            PuzzleCampaignDefinition secret = AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(
                TW08CampaignExpansionImporter.SecretCampaignPath);
            if (secret != null)
            {
                Button secretButton = TW08ProductionSceneUtility.CreateButton(
                    canvas.transform, "Secret", "ARQUIVO SECRETO", TW08ProductionSceneUtility.Amber, 15);
                TW08ProductionSceneUtility.SetRect(
                    (RectTransform)secretButton.transform, new Vector2(1f, 0f), new Vector2(260f, 50f),
                    new Vector2(-80f, 52f));
                SceneNavButton nav = secretButton.gameObject.AddComponent<SceneNavButton>();
                nav.Configure("TW08_SecretSelect");
                UnityEditor.Events.UnityEventTools.AddPersistentListener(secretButton.onClick, nav.Navigate);
            }

            WarehouseMapController controller = new GameObject("Warehouse Map Controller")
                .AddComponent<WarehouseMapController>();
            controller.Configure(campaign, nodes, title, detail, operatorText, scroll, back, "TW08_ModeSelect");

            canvas.gameObject.AddComponent<MenuNavigationAudio>();
            foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
            {
                if (button.GetComponent<MenuPressFeedback>() == null)
                {
                    button.gameObject.AddComponent<MenuPressFeedback>();
                }
            }

            TW08ProductionSceneUtility.Select(eventSystem, back);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MapScenePath);
            return MapScenePath;
        }

        private static ScrollRect BuildViewport(Canvas canvas, out RectTransform content)
        {
            GameObject viewport = new("Map Viewport",
                typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D), typeof(Image));
            viewport.transform.SetParent(canvas.transform, false);
            viewport.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.03f, 0.6f);
            TW08ProductionSceneUtility.SetRect(
                (RectTransform)viewport.transform, new Vector2(0.5f, 0.5f), new Vector2(1700f, 680f), new Vector2(0f, -10f));

            GameObject contentObject = new("Map Content", typeof(RectTransform));
            contentObject.transform.SetParent(viewport.transform, false);
            content = (RectTransform)contentObject.transform;
            content.anchorMin = new Vector2(0f, 0.5f);
            content.anchorMax = new Vector2(0f, 0.5f);
            content.pivot = new Vector2(0f, 0.5f);
            content.anchoredPosition = Vector2.zero;

            ScrollRect scroll = viewport.GetComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 45f;
            return scroll;
        }

        private static List<WarehouseMapNode> BuildNodes(PuzzleCampaignDefinition campaign, RectTransform content)
        {
            List<WarehouseMapNode> nodes = new();
            string lastSector = null;
            float x = 140f;
            int rowIndex = 0;
            Vector2 previous = Vector2.zero;
            bool hasPrevious = false;

            for (int i = 0; i < campaign.Levels.Count; i++)
            {
                PuzzleCampaignEntry entry = campaign.Levels[i];
                PuzzleLevelDefinition level = entry?.Level;
                if (level == null)
                {
                    continue;
                }

                // Setor novo abre um bloco próprio: o jogador lê o mapa por área
                // do armazém, não por número de fase.
                if (level.SectorId != lastSector)
                {
                    if (lastSector != null)
                    {
                        x += SectorGap;
                    }

                    CreateSectorLabel(content, level.SectorId, x);
                    lastSector = level.SectorId;
                    rowIndex = 0;
                }

                // Serpentina vertical de três: aproveita a altura sem obrigar o
                // jogador a rolar em dois eixos.
                float y = RowOffsetY - rowIndex * RowOffsetY;
                Vector2 position = new(x, y);

                if (hasPrevious)
                {
                    CreateTrail(content, previous, position);
                }

                nodes.Add(CreateNode(content, position, i + 1, level));
                previous = position;
                hasPrevious = true;

                rowIndex++;
                if (rowIndex >= NodesPerRow)
                {
                    rowIndex = 0;
                    x += NodeSpacingX;
                }
                else
                {
                    x += NodeSpacingX * 0.55f;
                }
            }

            content.sizeDelta = new Vector2(x + 200f, 620f);
            return nodes;
        }

        private static void CreateSectorLabel(RectTransform content, string sector, float x)
        {
            Image band = TW08ProductionSceneUtility.CreatePanel(
                content, "Sector " + sector, new Color(0.06f, 0.09f, 0.10f, 0.85f));
            TW08ProductionSceneUtility.SetRect(
                band.rectTransform, new Vector2(0f, 0.5f), new Vector2(6f, 560f), new Vector2(x - 70f, 0f));
            band.raycastTarget = false;

            Text label = TW08ProductionSceneUtility.CreateText(
                content, "Sector Label " + sector, sector, 22,
                TW08ProductionSceneUtility.Green, TextAnchor.UpperLeft);
            TW08ProductionSceneUtility.SetRect(
                label.rectTransform, new Vector2(0f, 0.5f), new Vector2(120f, 30f), new Vector2(x - 20f, 250f));
        }

        /// <summary>Segmento da trilha entre duas marcas — é o caminho do turno.</summary>
        private static void CreateTrail(RectTransform content, Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            Image line = TW08ProductionSceneUtility.CreatePanel(
                content, "Trail", new Color(0.20f, 0.30f, 0.32f, 0.75f));
            line.raycastTarget = false;

            RectTransform rect = line.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(delta.magnitude, 4f);
            rect.anchoredPosition = from;
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            rect.SetAsFirstSibling();
        }

        private static WarehouseMapNode CreateNode(
            RectTransform content, Vector2 position, int number, PuzzleLevelDefinition level)
        {
            GameObject root = new($"Node {number:00}", typeof(RectTransform));
            root.transform.SetParent(content, false);
            RectTransform rect = (RectTransform)root.transform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(120f, 120f);
            rect.anchoredPosition = position;

            Image ring = TW08ProductionSceneUtility.CreatePanel(
                root.transform, "Ring", new Color(1f, 1f, 1f, 0.18f));
            TW08ProductionSceneUtility.SetRect(
                ring.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(104f, 104f), Vector2.zero);
            ring.raycastTarget = false;

            GameObject buttonObject = new("Marker",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(root.transform, false);
            Image icon = buttonObject.GetComponent<Image>();
            icon.color = TW08ProductionSceneUtility.Cyan;
            TW08ProductionSceneUtility.SetRect(
                (RectTransform)buttonObject.transform, new Vector2(0.5f, 0.5f), new Vector2(72f, 72f), Vector2.zero);

            Text number0 = TW08ProductionSceneUtility.CreateText(
                buttonObject.transform, "Number", number.ToString("00"), 22,
                new Color(0.02f, 0.04f, 0.04f, 1f), TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.Stretch(number0.rectTransform);

            Text label = TW08ProductionSceneUtility.CreateText(
                root.transform, "Label", ShortName(level.DisplayName), 12,
                TW08ProductionSceneUtility.TextMuted, TextAnchor.UpperCenter);
            TW08ProductionSceneUtility.SetRect(
                label.rectTransform, new Vector2(0.5f, 0f), new Vector2(150f, 30f), new Vector2(0f, -6f));

            Text medal = TW08ProductionSceneUtility.CreateText(
                root.transform, "Medal", string.Empty, 14,
                TW08ProductionSceneUtility.Amber, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                medal.rectTransform, new Vector2(0.5f, 1f), new Vector2(120f, 22f), new Vector2(0f, 8f));

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = icon;

            return new WarehouseMapNode
            {
                button = button,
                root = rect,
                icon = icon,
                ring = ring,
                label = label,
                medal = medal,
                index = number - 1
            };
        }

        /// <summary>Nome curto: a marca tem 150 px e o nome inteiro não cabe.</summary>
        private static string ShortName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return string.Empty;
            }

            string upper = displayName.ToUpperInvariant();
            return upper.Length <= 18 ? upper : upper.Substring(0, 17) + "…";
        }
    }
}
#endif
