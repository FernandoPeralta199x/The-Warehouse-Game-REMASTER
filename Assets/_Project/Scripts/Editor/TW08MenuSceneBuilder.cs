#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TW08.Audio;
using TW08.Core;
using TW08.Data;
using TW08.Motion;
using TW08.Puzzle;
using TW08.Race;
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
    internal static class TW08MenuSceneBuilder
    {
        internal const string MainMenuPath = "Assets/_Project/Scenes/VerticalSlice/TW08_MainMenu.unity";
        internal const string SceneRoot = "Assets/_Project/Scenes/Production/Menus";
        internal const string ModePath = SceneRoot + "/TW08_ModeSelect.unity";
        internal const string OperatorPath = SceneRoot + "/TW08_OperatorSelect.unity";
        internal const string PuzzleSelectPath = SceneRoot + "/TW08_PuzzleSelect.unity";
        internal const string SecretSelectPath = SceneRoot + "/TW08_SecretSelect.unity";
        internal const string RaceSelectPath = SceneRoot + "/TW08_RaceSelect.unity";
        internal const string SettingsPath = SceneRoot + "/TW08_Settings.unity";
        internal const string CreditsPath = SceneRoot + "/TW08_Credits.unity";
        private const string GameConfigPath = "Assets/_Project/ScriptableObjects/Core/GameConfig.asset";

        internal static List<string> BuildAll(TW08ExpansionDataSetup.ExpansionData data)
        {
            TW08ProductionSceneUtility.EnsureFolder("Assets/_Project/Scenes/Production");
            TW08ProductionSceneUtility.EnsureFolder(SceneRoot);
            TW08ProductionSceneUtility.EnsureFolder("Assets/_Project/Scenes/VerticalSlice");

            BuildMainMenu();
            BuildModeSelect();
            BuildOperatorSelect(data.Roster);
            BuildPuzzleSelect(data.PuzzleCampaign);
            BuildRaceSelect(data.RaceCampaign);
            BuildSettings();
            BuildCredits();

            List<string> paths = new()
            {
                MainMenuPath, ModePath, OperatorPath, PuzzleSelectPath, RaceSelectPath, SettingsPath, CreditsPath
            };
            if (System.IO.File.Exists(SecretSelectPath))
            {
                paths.Add(SecretSelectPath);
            }

            return paths;
        }

        private static void BuildMainMenu()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            TW08ProductionSceneUtility.CreateCamera(new Vector3(0f, 0f, -10f), 5f);
            Canvas canvas = TW08ProductionSceneUtility.CreateCanvas();
            EventSystem eventSystem = TW08ProductionSceneUtility.CreateEventSystem();
            Image backdrop = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Backdrop", TW08ProductionSceneUtility.Background);
            TW08ProductionSceneUtility.Stretch(backdrop.rectTransform);
            CreateAnimatedBackdrop(canvas);

            Image shell = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Terminal Shell", TW08ProductionSceneUtility.Panel);
            TW08ProductionSceneUtility.SetRect(shell.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(880f, 700f), Vector2.zero);

            Text eyebrow = TW08ProductionSceneUtility.CreateText(shell.transform, "Eyebrow", "N-8 LOGISTICS // MANUAL RECOVERY TERMINAL", 18, TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(760f, 42f), new Vector2(0f, -58f));
            Text title = TW08ProductionSceneUtility.CreateText(shell.transform, "Title", "THE WAREHOUSE\nNº 08", 62, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            TW08ProductionSceneUtility.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(760f, 160f), new Vector2(0f, -165f));
            Text sub = TW08ProductionSceneUtility.CreateText(shell.transform, "Subtitle", "SETOR 08 // OPERAÇÃO MANUAL NECESSÁRIA", 19, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(700f, 40f), new Vector2(0f, -285f));

            RetroMainMenuController controller = new GameObject("Main Menu Controller").AddComponent<RetroMainMenuController>();
            Button start = TW08ProductionSceneUtility.CreateButton(shell.transform, "New Shift", "ENTRAR NO TERMINAL", TW08ProductionSceneUtility.Green, 20);
            Button cont = TW08ProductionSceneUtility.CreateButton(shell.transform, "Continue", "CONTINUAR [RESERVADO]", TW08ProductionSceneUtility.TextMuted, 17);
            Button quit = TW08ProductionSceneUtility.CreateButton(shell.transform, "Quit", "ENCERRAR TERMINAL", TW08ProductionSceneUtility.Amber, 17);
            TW08ProductionSceneUtility.SetRect((RectTransform)start.transform, new Vector2(0.5f, 0.5f), new Vector2(470f, 62f), new Vector2(0f, 45f));
            TW08ProductionSceneUtility.SetRect((RectTransform)cont.transform, new Vector2(0.5f, 0.5f), new Vector2(470f, 58f), new Vector2(0f, -30f));
            TW08ProductionSceneUtility.SetRect((RectTransform)quit.transform, new Vector2(0.5f, 0.5f), new Vector2(470f, 58f), new Vector2(0f, -102f));
            cont.interactable = false;
            UnityEventTools.AddPersistentListener(start.onClick, controller.StartNewShift);
            UnityEventTools.AddPersistentListener(quit.onClick, controller.QuitGame);
            Text version = TW08ProductionSceneUtility.CreateText(shell.transform, "Version", "BUILD // UNITY 6.3 LTS", 13, TW08ProductionSceneUtility.TextMuted, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(version.rectTransform, new Vector2(0.5f, 0f), new Vector2(720f, 36f), new Vector2(0f, 38f));
            controller.Configure(start, cont, version);
            TW08ProductionSceneUtility.Select(eventSystem, start);

            CreateLoadingOverlay(canvas);
            MenuShellRefs refs = new()
            {
                Canvas = canvas,
                Eyebrow = eyebrow,
                Title = title,
                Subtitle = sub
            };
            ApplyMenuMotion(
                refs,
                shell.transform,
                new Component[] { start, cont, quit, version },
                markerWidth: 500f,
                markerHeight: 78f);

            CreatePersistentServices();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuPath);
        }

        private static void BuildModeSelect()
        {
            Scene scene = CreateMenuShell(
                "CENTRAL DE OPERAÇÕES",
                "SELECIONE O MODO DE TRABALHO",
                out Transform shell,
                out EventSystem eventSystem,
                out MenuShellRefs refs);
            ModeSelectMenuController controller = new GameObject("Mode Select Controller").AddComponent<ModeSelectMenuController>();
            Button campaign = MenuButton(shell, "Campaign", "CAMPANHA // RECUPERAÇÃO MANUAL", TW08ProductionSceneUtility.Green, 155f);
            Button race = MenuButton(shell, "Race", "N-8 LOGISTICS RUSH // CORRIDA", TW08ProductionSceneUtility.Amber, 80f);
            Button shop = MenuButton(shell, "Shop", "OFICINA N-8 // FERRAMENTAS", TW08ProductionSceneUtility.Amber, 5f);
            Button operators = MenuButton(shell, "Operators", "OPERADORES", TW08ProductionSceneUtility.Cyan, -70f);
            Button settings = MenuButton(shell, "Settings", "CONFIGURAÇÕES", TW08ProductionSceneUtility.TextMuted, -145f);
            Button credits = MenuButton(shell, "Credits", "CRÉDITOS", TW08ProductionSceneUtility.TextMuted, -220f);
            Button back = MenuButton(shell, "Back", "VOLTAR AO TERMINAL", TW08ProductionSceneUtility.TextMuted, -295f);
            UnityEventTools.AddPersistentListener(campaign.onClick, controller.OpenCampaign);
            UnityEventTools.AddPersistentListener(race.onClick, controller.OpenRace);
            UnityEventTools.AddPersistentListener(shop.onClick, controller.OpenShop);
            UnityEventTools.AddPersistentListener(operators.onClick, controller.OpenOperators);
            UnityEventTools.AddPersistentListener(settings.onClick, controller.OpenSettings);
            UnityEventTools.AddPersistentListener(credits.onClick, controller.OpenCredits);
            UnityEventTools.AddPersistentListener(back.onClick, controller.BackToMainMenu);
            TW08ProductionSceneUtility.Select(eventSystem, campaign);
            ApplyMenuMotion(
                refs,
                shell,
                new Component[] { campaign, race, shop, operators, settings, credits, back });
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ModePath);
        }

        private static void BuildOperatorSelect(CharacterRoster roster)
        {
            Scene scene = CreateMenuShell(
                "OPERADORES N-8",
                "JOHN // DUDA // ROBERT",
                out Transform shell,
                out EventSystem eventSystem,
                out MenuShellRefs refs);
            Image portraitFrame = TW08ProductionSceneUtility.CreatePanel(shell, "Portrait Frame", TW08ProductionSceneUtility.PanelLight);
            TW08ProductionSceneUtility.SetRect(portraitFrame.rectTransform, new Vector2(0f, 0.5f), new Vector2(390f, 470f), new Vector2(65f, -20f));

            // O fantasma nasce antes do retrato para ficar atrás dele: é a imagem
            // que sai durante o cross-fade de troca de operador.
            GameObject ghostGo = new("Portrait Ghost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ghostGo.transform.SetParent(portraitFrame.transform, false);
            Image portraitGhost = ghostGo.GetComponent<Image>();
            portraitGhost.color = Color.white;
            portraitGhost.preserveAspect = true;
            portraitGhost.raycastTarget = false;
            portraitGhost.enabled = false;
            TW08ProductionSceneUtility.SetRect(portraitGhost.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(300f, 390f), Vector2.zero);

            GameObject portraitGo = new("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portraitGo.transform.SetParent(portraitFrame.transform, false);
            Image portrait = portraitGo.GetComponent<Image>();
            portrait.color = Color.white;
            portrait.preserveAspect = true;
            TW08ProductionSceneUtility.SetRect(portrait.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(300f, 390f), Vector2.zero);

            Text name = TW08ProductionSceneUtility.CreateText(shell, "Name", "JOHN MILLER", 30, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleLeft);
            Text role = TW08ProductionSceneUtility.CreateText(shell, "Role", "OPERADOR MANUAL", 18, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleLeft);
            Text description = TW08ProductionSceneUtility.CreateText(shell, "Description", "", 16, TW08ProductionSceneUtility.TextMuted, TextAnchor.UpperLeft);
            Text status = TW08ProductionSceneUtility.CreateText(shell, "Status", "OPERADOR ATIVO", 18, TW08ProductionSceneUtility.Green, TextAnchor.MiddleLeft);
            TW08ProductionSceneUtility.SetRect(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(690f, 54f), new Vector2(500f, 155f));
            TW08ProductionSceneUtility.SetRect(role.rectTransform, new Vector2(0f, 0.5f), new Vector2(690f, 40f), new Vector2(500f, 105f));
            TW08ProductionSceneUtility.SetRect(description.rectTransform, new Vector2(0f, 0.5f), new Vector2(700f, 165f), new Vector2(500f, 60f));
            TW08ProductionSceneUtility.SetRect(status.rectTransform, new Vector2(0f, 0.5f), new Vector2(500f, 40f), new Vector2(500f, -120f));

            Image accentBar = TW08ProductionSceneUtility.CreatePanel(shell, "Accent Bar", TW08ProductionSceneUtility.Amber);
            accentBar.raycastTarget = false;
            TW08ProductionSceneUtility.SetRect(accentBar.rectTransform, new Vector2(0f, 0.5f), new Vector2(690f, 4f), new Vector2(500f, 132f));

            Button previous = TW08ProductionSceneUtility.CreateButton(shell, "Previous", "< ANTERIOR", TW08ProductionSceneUtility.Cyan, 16);
            Button next = TW08ProductionSceneUtility.CreateButton(shell, "Next", "PRÓXIMO >", TW08ProductionSceneUtility.Cyan, 16);
            Button confirm = TW08ProductionSceneUtility.CreateButton(shell, "Confirm", "ATIVAR OPERADOR", TW08ProductionSceneUtility.Green, 17);
            Button back = TW08ProductionSceneUtility.CreateButton(shell, "Back", "VOLTAR", TW08ProductionSceneUtility.TextMuted, 15);
            TW08ProductionSceneUtility.SetRect((RectTransform)previous.transform, new Vector2(0f, 0.5f), new Vector2(200f, 52f), new Vector2(500f, -200f));
            TW08ProductionSceneUtility.SetRect((RectTransform)next.transform, new Vector2(0f, 0.5f), new Vector2(200f, 52f), new Vector2(720f, -200f));
            TW08ProductionSceneUtility.SetRect((RectTransform)confirm.transform, new Vector2(0f, 0.5f), new Vector2(280f, 52f), new Vector2(940f, -200f));
            TW08ProductionSceneUtility.SetRect((RectTransform)back.transform, new Vector2(0f, 0f), new Vector2(180f, 50f), new Vector2(65f, 48f));

            CharacterSelectController controller = new GameObject("Character Select Controller").AddComponent<CharacterSelectController>();
            controller.Configure(
                roster, name, role, description, status, portrait, previous, next, confirm, back,
                "TW08_ModeSelect", portraitGhost, accentBar);
            TW08ProductionSceneUtility.Select(eventSystem, confirm);
            ApplyMenuMotion(
                refs,
                shell,
                new Component[] { portraitFrame, name, accentBar, role, description, status, previous, next, confirm, back },
                markerWidth: 300f,
                markerHeight: 68f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, OperatorPath);
        }

        private static void BuildPuzzleSelect(PuzzleCampaignDefinition campaign)
        {
            // Mesma proteção do BuildRaceSelect: re-resolve o asset pelo caminho.
            PuzzleCampaignDefinition reloaded = AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(
                TW08ExpansionDataSetup.PuzzleCampaignPath);
            if (reloaded != null)
            {
                campaign = reloaded;
            }

            PuzzleCampaignDefinition secretCampaign =
                AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(
                    TW08CampaignExpansionImporter.SecretCampaignPath);

            BuildLevelGridSelect(
                campaign,
                PuzzleSelectPath,
                "CAMPANHA // ROTAS",
                $"{campaign.Levels.Count} FASES OPERACIONAIS",
                "TW08_ModeSelect",
                secretCampaign != null ? "TW08_SecretSelect" : null);

            if (secretCampaign != null)
            {
                BuildLevelGridSelect(
                    secretCampaign,
                    SecretSelectPath,
                    "ARQUIVO SECRETO // N-8",
                    $"{secretCampaign.Levels.Count} REGISTROS OCULTOS",
                    "TW08_PuzzleSelect",
                    null);
            }
        }

        private static void BuildLevelGridSelect(
            PuzzleCampaignDefinition campaign,
            string scenePath,
            string title,
            string subtitle,
            string backSceneName,
            string secretSceneName)
        {
            // Captura o caminho ANTES de criar a cena (NewScene invalida wrappers
            // nativos) e re-resolve o asset logo depois do shell.
            string campaignPath = campaign != null ? AssetDatabase.GetAssetPath(campaign) : null;

            Scene scene = CreateMenuShell(
                title, subtitle, out Transform shell, out EventSystem eventSystem, out MenuShellRefs refs);

            if (!string.IsNullOrEmpty(campaignPath))
            {
                PuzzleCampaignDefinition reloadedCampaign =
                    AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(campaignPath);
                if (reloadedCampaign != null)
                {
                    campaign = reloadedCampaign;
                }
            }

            if (campaign == null)
            {
                throw new InvalidOperationException(
                    $"BuildLevelGridSelect: campanha de puzzle indisponível após reload ({scenePath}).");
            }

            Text operatorText = TW08ProductionSceneUtility.CreateText(shell, "Operator", "OPERADOR // JOHN", 16, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(operatorText.rectTransform, new Vector2(1f, 1f), new Vector2(470f, 40f), new Vector2(-58f, -118f));

            // Viewport rolável: suporta qualquer quantidade de fases.
            var scrollGo = new GameObject("Level Scroll",
                typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D), typeof(ScrollToSelected));
            scrollGo.transform.SetParent(shell, false);
            var scrollRt = (RectTransform)scrollGo.transform;
            TW08ProductionSceneUtility.SetRect(scrollRt, new Vector2(0.5f, 0.5f), new Vector2(1270f, 470f), new Vector2(0f, 10f));

            var contentGo = new GameObject("Content",
                typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(scrollGo.transform, false);
            var contentRt = (RectTransform)contentGo.transform;
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var grid = contentGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(400f, 118f);
            grid.spacing = new Vector2(18f, 16f);
            grid.padding = new RectOffset(8, 8, 6, 6);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperCenter;

            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scrollGo.GetComponent<ScrollRect>();
            scrollRect.content = contentRt;
            scrollRect.viewport = scrollRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 42f;

            // Cascata só por alfa: o GridLayoutGroup é dono do anchoredPosition dos
            // cartões e o MenuFocusAnimator é dono do localScale deles. Qualquer
            // estilo de entrada que mexa em posição ou escala disputaria com os dois.
            UIEntranceAnimator gridEntrance = contentGo.AddComponent<UIEntranceAnimator>();
            gridEntrance.Configure(EntranceStyle.Fade, 0.34f, 0.16f, true, 0.026f, 0f);

            List<Button> buttons = new();
            for (int i = 0; i < campaign.Levels.Count; i++)
            {
                Button button = TW08ProductionSceneUtility.CreateButton(
                    contentGo.transform,
                    "Level " + (i + 1),
                    $"{i + 1:00} // ROTA",
                    i < 3 ? TW08ProductionSceneUtility.Green : TW08ProductionSceneUtility.Cyan,
                    15);
                buttons.Add(button);
            }

            Text hint = TW08ProductionSceneUtility.CreateText(
                shell, "Hint", "PRÓXIMA ROTA // 01", 16,
                TW08ProductionSceneUtility.Amber, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(1100f, 34f), new Vector2(0f, 108f));

            Button back = TW08ProductionSceneUtility.CreateButton(shell, "Back", "VOLTAR", TW08ProductionSceneUtility.TextMuted, 15);
            TW08ProductionSceneUtility.SetRect((RectTransform)back.transform, new Vector2(0f, 0f), new Vector2(180f, 50f), new Vector2(60f, 44f));

            if (!string.IsNullOrEmpty(secretSceneName))
            {
                Button secretButton = TW08ProductionSceneUtility.CreateButton(
                    shell, "Secret Archive", "ARQUIVO SECRETO", TW08ProductionSceneUtility.Amber, 15);
                TW08ProductionSceneUtility.SetRect(
                    (RectTransform)secretButton.transform, new Vector2(1f, 0f), new Vector2(260f, 50f), new Vector2(-70f, 44f));
                SceneNavButton nav = secretButton.gameObject.AddComponent<SceneNavButton>();
                nav.Configure(secretSceneName, "arquivo secreto");
                UnityEventTools.AddPersistentListener(secretButton.onClick, nav.Navigate);
            }

            PuzzleLevelSelectController controller = new GameObject("Puzzle Level Select Controller").AddComponent<PuzzleLevelSelectController>();
            controller.Configure(campaign, buttons, operatorText, back, backSceneName, hint);
            TW08ProductionSceneUtility.Select(eventSystem, buttons.Count > 0 ? buttons[0] : back);

            // O marcador mora dentro da viewport rolável para o RectMask2D recortá-lo
            // junto com os cartões: fora dela ele flutuaria por cima do painel.
            ApplyMenuMotion(
                refs,
                shell,
                new Component[] { operatorText, scrollRt, hint, back },
                scrollGo.transform,
                markerWidth: 412f,
                markerHeight: 128f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void BuildRaceSelect(RaceCampaignDefinition campaign)
        {
            Scene scene = CreateMenuShell(
                "N-8 LOGISTICS RUSH",
                "SELEÇÃO DE PISTA",
                out Transform shell,
                out EventSystem eventSystem,
                out MenuShellRefs refs);

            // CreateMenuShell (NewScene) invalida objetos nativos capturados antes;
            // re-resolve a campanha do AssetDatabase DEPOIS de criar a cena.
            RaceCampaignDefinition reloaded = AssetDatabase.LoadAssetAtPath<RaceCampaignDefinition>(
                TW08ExpansionDataSetup.RaceCampaignPath);
            if (reloaded != null)
            {
                campaign = reloaded;
            }

            if (campaign == null || campaign.Tracks.Count == 0)
            {
                throw new InvalidOperationException(
                    "BuildRaceSelect: campanha de corrida indisponível ou sem pistas após reload.");
            }
            Text pilot = TW08ProductionSceneUtility.CreateText(shell, "Pilot", "PILOTO // JOHN", 16, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(pilot.rectTransform, new Vector2(1f, 1f), new Vector2(470f, 40f), new Vector2(-58f, -118f));
            List<Button> buttons = new();
            int trackCount = campaign != null ? campaign.Tracks.Count : 0;
            float buttonHeight = trackCount > 3 ? 88f : 105f;
            float spacing = trackCount > 3 ? 112f : 145f;
            float top = spacing * (trackCount - 1) * 0.5f;
            for (int i = 0; i < trackCount; i++)
            {
                string trackName = campaign.Tracks[i] != null
                    ? campaign.Tracks[i].DisplayName.ToUpperInvariant()
                    : $"PISTA {i + 1:00}";
                Button button = TW08ProductionSceneUtility.CreateButton(shell, "Track " + (i + 1), $"{i + 1:00} // {trackName}", i == 0 ? TW08ProductionSceneUtility.Green : TW08ProductionSceneUtility.Cyan, 18);
                TW08ProductionSceneUtility.SetRect((RectTransform)button.transform, new Vector2(0.5f, 0.5f), new Vector2(780f, buttonHeight), new Vector2(0f, top - i * spacing));
                buttons.Add(button);
            }
            Button back = TW08ProductionSceneUtility.CreateButton(shell, "Back", "VOLTAR", TW08ProductionSceneUtility.TextMuted, 15);
            TW08ProductionSceneUtility.SetRect((RectTransform)back.transform, new Vector2(0f, 0f), new Vector2(180f, 50f), new Vector2(60f, 44f));
            RaceTrackSelectController controller = new GameObject("Race Track Select Controller").AddComponent<RaceTrackSelectController>();
            controller.Configure(campaign, buttons, pilot, back, "TW08_ModeSelect");
            TW08ProductionSceneUtility.Select(eventSystem, buttons[0]);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, RaceSelectPath);
        }

        private static void BuildSettings()
        {
            Scene scene = CreateMenuShell(
                "CONFIGURAÇÕES",
                "ÁUDIO // PERFIL LOCAL",
                out Transform shell,
                out EventSystem eventSystem,
                out MenuShellRefs refs);
            Text masterLabel = Label(shell, "MASTER", 150f);
            Text musicLabel = Label(shell, "MÚSICA", 35f);
            Text sfxLabel = Label(shell, "SFX", -80f);
            Text masterValue = Value(shell, "100%", 150f);
            Text musicValue = Value(shell, "80%", 35f);
            Text sfxValue = Value(shell, "100%", -80f);
            Slider master = CreateSlider(shell, "Master Slider", 150f);
            Slider music = CreateSlider(shell, "Music Slider", 35f);
            Slider sfx = CreateSlider(shell, "SFX Slider", -80f);
            Button back = TW08ProductionSceneUtility.CreateButton(shell, "Back", "SALVAR E VOLTAR", TW08ProductionSceneUtility.Green, 16);
            TW08ProductionSceneUtility.SetRect((RectTransform)back.transform, new Vector2(0.5f, 0.5f), new Vector2(360f, 56f), new Vector2(0f, -225f));
            SettingsMenuController controller = new GameObject("Settings Controller").AddComponent<SettingsMenuController>();
            controller.Configure(master, music, sfx, masterValue, musicValue, sfxValue, back, "TW08_ModeSelect");

            // Os sliders entram junto com os rótulos: separar as duas colunas
            // faria a linha do MASTER surgir sem o controle que ela nomeia.
            ApplyMenuMotion(
                refs,
                shell,
                new Component[]
                {
                    masterLabel, master, masterValue,
                    musicLabel, music, musicValue,
                    sfxLabel, sfx, sfxValue,
                    back
                },
                markerWidth: 620f,
                markerHeight: 66f);

            TW08ProductionSceneUtility.Select(eventSystem, back);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, SettingsPath);
        }

        private static void BuildCredits()
        {
            Scene scene = CreateMenuShell(
                "CRÉDITOS",
                "THE WAREHOUSE Nº 08",
                out Transform shell,
                out EventSystem eventSystem,
                out MenuShellRefs refs);
            Text body = TW08ProductionSceneUtility.CreateText(
                shell, "Credits Body",
                "DIREÇÃO / PRODUÇÃO\nFERNANDO PERALTA\n\nDESENVOLVIMENTO\nUNITY 6.3 LTS // C#\n\nDIREÇÃO VISUAL\nINDUSTRIAL RETRO-FUTURISTA N-8\n\nPERSONAGENS\nJOHN MILLER // MARIA EDUARDA 'DUDA' // ROBERT 'BIG ROB' HAYES\n\nPROJETO ORIGINAL // THE WAREHOUSE Nº 08",
                20, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(960f, 520f), new Vector2(0f, 20f));
            Button back = TW08ProductionSceneUtility.CreateButton(shell, "Back", "VOLTAR", TW08ProductionSceneUtility.Green, 16);
            TW08ProductionSceneUtility.SetRect((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(260f, 54f), new Vector2(0f, 48f));
            // Máscara: o texto rola continuamente e precisa ser cortado na
            // moldura, senão invade o cabeçalho e o botão de voltar.
            Image viewport = TW08ProductionSceneUtility.CreatePanel(shell, "Credits Viewport", new Color(0f, 0f, 0f, 0f));
            TW08ProductionSceneUtility.SetRect(viewport.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(980f, 520f), new Vector2(0f, 20f));
            viewport.gameObject.AddComponent<RectMask2D>();
            body.rectTransform.SetParent(viewport.transform, false);

            CreditsScreenController controller = new GameObject("Credits Screen Controller")
                .AddComponent<CreditsScreenController>();
            controller.Configure(body.rectTransform, back, "TW08_ModeSelect", 700f);

            ApplyMenuMotion(
                refs,
                shell,
                new Component[] { viewport, back },
                markerWidth: 320f,
                markerHeight: 62f);

            TW08ProductionSceneUtility.Select(eventSystem, back);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, CreditsPath);
        }

        /// <summary>Referências do cabeçalho usadas pela entrada em cascata.</summary>
        private struct MenuShellRefs
        {
            public Canvas Canvas;
            public Text Eyebrow;
            public Text Title;
            public Text Subtitle;
        }

        private static Scene CreateMenuShell(
            string titleText,
            string subtitleText,
            out Transform shell,
            out EventSystem eventSystem,
            out MenuShellRefs refs)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            TW08ProductionSceneUtility.CreateCamera(new Vector3(0f, 0f, -10f), 5f);
            Canvas canvas = TW08ProductionSceneUtility.CreateCanvas();
            eventSystem = TW08ProductionSceneUtility.CreateEventSystem();
            Image backdrop = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Backdrop", TW08ProductionSceneUtility.Background);
            TW08ProductionSceneUtility.Stretch(backdrop.rectTransform);
            CreateAnimatedBackdrop(canvas);
            Image panel = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Terminal Shell", TW08ProductionSceneUtility.Panel);
            TW08ProductionSceneUtility.SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1480f, 860f), Vector2.zero);
            shell = panel.transform;
            Text eyebrow = TW08ProductionSceneUtility.CreateText(shell, "Eyebrow", "N-8 LOGISTICS // CENTRAL", 15, TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(900f, 35f), new Vector2(0f, -42f));
            Text title = TW08ProductionSceneUtility.CreateText(shell, "Title", titleText, 34, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(1100f, 58f), new Vector2(0f, -88f));
            Text subtitle = TW08ProductionSceneUtility.CreateText(shell, "Subtitle", subtitleText, 16, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(1100f, 38f), new Vector2(0f, -135f));
            CreateLoadingOverlay(canvas);

            refs = new MenuShellRefs
            {
                Canvas = canvas,
                Eyebrow = eyebrow,
                Title = title,
                Subtitle = subtitle
            };
            return scene;
        }

        /// <summary>
        /// Grade do terminal com folga nas bordas e a varredura que desce a tela.
        ///
        /// O nome "Terminal Grid Overlay" é obrigatório: o passo de polimento de
        /// produção procura a grade por esse nome e, se não encontrar, cria uma
        /// segunda esticada exatamente no canvas — duas grades sobrepostas.
        /// </summary>
        internal static void CreateAnimatedBackdrop(Canvas canvas)
        {
            GameObject gridGo = new(
                "Terminal Grid Overlay",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TerminalGridGraphic),
                typeof(MenuBackdropAnimator));
            gridGo.transform.SetParent(canvas.transform, false);
            var gridRect = (RectTransform)gridGo.transform;

            // A folga de 90 px existe porque a grade deriva devagar: sem ela a
            // deriva revelaria uma faixa vazia na borda da tela.
            TW08ProductionSceneUtility.SetRect(
                gridRect, Vector2.zero, Vector2.one, new Vector2(-90f, -90f), new Vector2(90f, 90f));

            TerminalGridGraphic grid = gridGo.GetComponent<TerminalGridGraphic>();
            grid.Configure(new Color(0.20f, 0.90f, 0.47f, 0.055f), 72f, 9f, 1f);

            Image sweep = TW08ProductionSceneUtility.CreatePanel(
                gridGo.transform, "Scanline Sweep", new Color(0.25f, 0.95f, 0.58f, 0.05f));
            sweep.raycastTarget = false;
            TW08ProductionSceneUtility.SetRect(
                sweep.rectTransform, new Vector2(0.5f, 1f), new Vector2(2400f, 96f), new Vector2(0f, 130f));

            gridGo.GetComponent<MenuBackdropAnimator>().Configure(gridRect, grid, sweep.rectTransform, sweep);
            EditorUtility.SetDirty(grid);
        }

        /// <summary>
        /// Sobreposição de carregamento em estilo terminal, sempre o último filho
        /// do canvas. A saída animada de menu a preserva de propósito: é o que o
        /// jogador deve ver quando o resto apaga.
        /// </summary>
        internal static void CreateLoadingOverlay(Canvas canvas)
        {
            Image panel = TW08ProductionSceneUtility.CreatePanel(
                canvas.transform, "Loading Overlay", new Color(0.012f, 0.017f, 0.020f, 0.985f));
            TW08ProductionSceneUtility.Stretch(panel.rectTransform);
            panel.transform.SetAsLastSibling();

            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Text heading = TW08ProductionSceneUtility.CreateText(
                panel.transform, "Heading", "N-8 LOGISTICS // TRANSFERINDO SETOR", 18,
                TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                heading.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1100f, 36f), new Vector2(0f, 118f));

            Text status = TW08ProductionSceneUtility.CreateText(
                panel.transform, "Status", "INICIALIZANDO SETOR...", 24,
                TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                status.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1200f, 44f), new Vector2(0f, 58f));

            Image barBackground = TW08ProductionSceneUtility.CreatePanel(
                panel.transform, "Bar Background", TW08ProductionSceneUtility.PanelLight);
            TW08ProductionSceneUtility.SetRect(
                barBackground.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(900f, 26f), new Vector2(0f, 0f));

            Image fill = TW08ProductionSceneUtility.CreatePanel(
                barBackground.transform, "Bar Fill", TW08ProductionSceneUtility.Green);
            // Âncora direita em 0: o apresentador move anchorMax.x de 0 até 1 e a
            // barra cresce sem depender do tamanho do pai.
            TW08ProductionSceneUtility.SetRect(
                fill.rectTransform, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 3f), new Vector2(0f, -3f));

            Text percent = TW08ProductionSceneUtility.CreateText(
                panel.transform, "Percent", "000%", 20,
                TW08ProductionSceneUtility.Amber, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                percent.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(300f, 34f), new Vector2(0f, -44f));

            Text cursor = TW08ProductionSceneUtility.CreateText(
                panel.transform, "Cursor", "_", 26,
                TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(
                cursor.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(60f, 34f), new Vector2(0f, -84f));

            LoadingScreenPresenter presenter = panel.gameObject.AddComponent<LoadingScreenPresenter>();
            presenter.Configure(group, fill.rectTransform, status, percent, cursor);
            EditorUtility.SetDirty(presenter);
        }

        private static Image CreateFocusMarker(Transform parent, Vector2 size)
        {
            Color accent = TW08ProductionSceneUtility.Green;
            Image marker = TW08ProductionSceneUtility.CreatePanel(
                parent, "Focus Marker", new Color(accent.r, accent.g, accent.b, 0.18f));
            marker.raycastTarget = false;
            TW08ProductionSceneUtility.SetRect(
                marker.rectTransform, new Vector2(0.5f, 0.5f), size, Vector2.zero);
            // Atrás dos botões: o marcador é fundo de destaque, não moldura por cima.
            marker.rectTransform.SetAsFirstSibling();
            return marker;
        }

        /// <summary>
        /// Instala foco expressivo, entrada em cascata e pulso de confirmação numa
        /// tela já montada.
        /// </summary>
        private static void ApplyMenuMotion(
            MenuShellRefs refs,
            Transform shell,
            IEnumerable<Component> cascade,
            Transform markerParent = null,
            float markerWidth = 700f,
            float markerHeight = 74f)
        {
            Transform host = markerParent != null ? markerParent : shell;
            Image marker = CreateFocusMarker(host, new Vector2(markerWidth, markerHeight));

            Color accent = TW08ProductionSceneUtility.Green;
            MenuFocusAnimator focus = shell.gameObject.GetComponent<MenuFocusAnimator>();
            if (focus == null)
            {
                focus = shell.gameObject.AddComponent<MenuFocusAnimator>();
            }

            focus.Configure(shell, marker.rectTransform, marker, new Color(accent.r, accent.g, accent.b, 0.22f));

            MenuScreenAnimator entrance = shell.gameObject.GetComponent<MenuScreenAnimator>();
            if (entrance == null)
            {
                entrance = shell.gameObject.AddComponent<MenuScreenAnimator>();
            }

            entrance.Configure(refs.Eyebrow, refs.Title, refs.Subtitle, cascade);

            if (refs.Canvas != null)
            {
                foreach (Button button in refs.Canvas.GetComponentsInChildren<Button>(true))
                {
                    if (button != null && button.GetComponent<MenuPressFeedback>() == null)
                    {
                        button.gameObject.AddComponent<MenuPressFeedback>();
                    }
                }

                // Som ao mudar o foco: navegar em silêncio parece tela travada.
                if (refs.Canvas.GetComponent<MenuNavigationAudio>() == null)
                {
                    refs.Canvas.gameObject.AddComponent<MenuNavigationAudio>();
                }
            }

            EditorUtility.SetDirty(focus);
            EditorUtility.SetDirty(entrance);
        }

        private static Button MenuButton(Transform shell, string name, string label, Color accent, float y)
        {
            Button button = TW08ProductionSceneUtility.CreateButton(shell, name, label, accent, 18);
            TW08ProductionSceneUtility.SetRect((RectTransform)button.transform, new Vector2(0.5f, 0.5f), new Vector2(680f, 58f), new Vector2(0f, y));
            return button;
        }

        private static Text Label(Transform shell, string text, float y)
        {
            Text label = TW08ProductionSceneUtility.CreateText(shell, text + " Label", text, 18, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleLeft);
            TW08ProductionSceneUtility.SetRect(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(260f, 46f), new Vector2(250f, y));
            return label;
        }

        private static Text Value(Transform shell, string text, float y)
        {
            Text value = TW08ProductionSceneUtility.CreateText(shell, text + " Value", text, 16, TW08ProductionSceneUtility.Green, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(value.rectTransform, new Vector2(1f, 0.5f), new Vector2(180f, 46f), new Vector2(-250f, y));
            return value;
        }

        private static Slider CreateSlider(Transform shell, string name, float y)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(shell, false);
            Slider slider = root.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.direction = Slider.Direction.LeftToRight;
            TW08ProductionSceneUtility.SetRect((RectTransform)root.transform, new Vector2(0.5f, 0.5f), new Vector2(660f, 42f), new Vector2(0f, y));

            Image background = TW08ProductionSceneUtility.CreatePanel(root.transform, "Background", TW08ProductionSceneUtility.PanelLight);
            TW08ProductionSceneUtility.Stretch(background.rectTransform);
            GameObject fillArea = new("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            RectTransform fillAreaRect = (RectTransform)fillArea.transform;
            fillAreaRect.anchorMin = new Vector2(0f, 0.2f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.8f);
            fillAreaRect.offsetMin = new Vector2(8f, 0f);
            fillAreaRect.offsetMax = new Vector2(-8f, 0f);
            Image fill = TW08ProductionSceneUtility.CreatePanel(fillArea.transform, "Fill", TW08ProductionSceneUtility.Green);
            TW08ProductionSceneUtility.Stretch(fill.rectTransform);
            GameObject handleArea = new("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(root.transform, false);
            TW08ProductionSceneUtility.Stretch((RectTransform)handleArea.transform);
            Image handle = TW08ProductionSceneUtility.CreatePanel(handleArea.transform, "Handle", TW08ProductionSceneUtility.Amber);
            TW08ProductionSceneUtility.SetRect(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(24f, 48f), Vector2.zero);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            return slider;
        }

        private static void CreatePersistentServices()
        {
            TW08ProductionSceneUtility.EnsureFolder("Assets/_Project/ScriptableObjects/Core");
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(config, GameConfigPath);
            }
            SerializedObject configSerialized = new(config);
            configSerialized.FindProperty("saveVersion").intValue = 3;
            configSerialized.FindProperty("mainMenuScene").stringValue = "TW08_MainMenu";
            configSerialized.FindProperty("firstPuzzleScene").stringValue = "TW08_Level01_FirstShift";
            configSerialized.FindProperty("firstRaceScene").stringValue = "TW08_Race01_ReceivingLoop";
            configSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);

            GameObject bootstrapRoot = new("Production Bootstrap");
            GameBootstrap bootstrap = bootstrapRoot.AddComponent<GameBootstrap>();
            SaveManager save = bootstrapRoot.AddComponent<SaveManager>();
            bootstrapRoot.AddComponent<MusicService>();
            SerializedObject bootstrapSerialized = new(bootstrap);
            bootstrapSerialized.FindProperty("config").objectReferenceValue = config;
            bootstrapSerialized.FindProperty("persistAcrossScenes").boolValue = true;
            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject saveSerialized = new(save);
            saveSerialized.FindProperty("config").objectReferenceValue = config;
            saveSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
            EditorUtility.SetDirty(save);

            new GameObject("Audio Service").AddComponent<AudioService>();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
