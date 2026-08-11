#if UNITY_EDITOR
using System.Collections.Generic;
using TW08.Audio;
using TW08.Core;
using TW08.Data;
using TW08.Puzzle;
using TW08.Race;
using TW08.Save;
using TW08.UI;
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

            return new List<string>
            {
                MainMenuPath, ModePath, OperatorPath, PuzzleSelectPath, RaceSelectPath, SettingsPath, CreditsPath
            };
        }

        private static void BuildMainMenu()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            TW08ProductionSceneUtility.CreateCamera(new Vector3(0f, 0f, -10f), 5f);
            Canvas canvas = TW08ProductionSceneUtility.CreateCanvas();
            EventSystem eventSystem = TW08ProductionSceneUtility.CreateEventSystem();
            Image backdrop = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Backdrop", TW08ProductionSceneUtility.Background);
            TW08ProductionSceneUtility.Stretch(backdrop.rectTransform);

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

            CreatePersistentServices();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuPath);
        }

        private static void BuildModeSelect()
        {
            Scene scene = CreateMenuShell("CENTRAL DE OPERAÇÕES", "SELECIONE O MODO DE TRABALHO", out Transform shell, out EventSystem eventSystem);
            ModeSelectMenuController controller = new GameObject("Mode Select Controller").AddComponent<ModeSelectMenuController>();
            Button campaign = MenuButton(shell, "Campaign", "CAMPANHA // RECUPERAÇÃO MANUAL", TW08ProductionSceneUtility.Green, 120f);
            Button race = MenuButton(shell, "Race", "N-8 LOGISTICS RUSH // CORRIDA", TW08ProductionSceneUtility.Amber, 45f);
            Button operators = MenuButton(shell, "Operators", "OPERADORES", TW08ProductionSceneUtility.Cyan, -30f);
            Button settings = MenuButton(shell, "Settings", "CONFIGURAÇÕES", TW08ProductionSceneUtility.TextMuted, -105f);
            Button credits = MenuButton(shell, "Credits", "CRÉDITOS", TW08ProductionSceneUtility.TextMuted, -180f);
            Button back = MenuButton(shell, "Back", "VOLTAR AO TERMINAL", TW08ProductionSceneUtility.TextMuted, -255f);
            UnityEventTools.AddPersistentListener(campaign.onClick, controller.OpenCampaign);
            UnityEventTools.AddPersistentListener(race.onClick, controller.OpenRace);
            UnityEventTools.AddPersistentListener(operators.onClick, controller.OpenOperators);
            UnityEventTools.AddPersistentListener(settings.onClick, controller.OpenSettings);
            UnityEventTools.AddPersistentListener(credits.onClick, controller.OpenCredits);
            UnityEventTools.AddPersistentListener(back.onClick, controller.BackToMainMenu);
            TW08ProductionSceneUtility.Select(eventSystem, campaign);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ModePath);
        }

        private static void BuildOperatorSelect(CharacterRoster roster)
        {
            Scene scene = CreateMenuShell("OPERADORES N-8", "JOHN // DUDA // ROBERT", out Transform shell, out EventSystem eventSystem);
            Image portraitFrame = TW08ProductionSceneUtility.CreatePanel(shell, "Portrait Frame", TW08ProductionSceneUtility.PanelLight);
            TW08ProductionSceneUtility.SetRect(portraitFrame.rectTransform, new Vector2(0f, 0.5f), new Vector2(390f, 470f), new Vector2(65f, -20f));
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

            Button previous = TW08ProductionSceneUtility.CreateButton(shell, "Previous", "< ANTERIOR", TW08ProductionSceneUtility.Cyan, 16);
            Button next = TW08ProductionSceneUtility.CreateButton(shell, "Next", "PRÓXIMO >", TW08ProductionSceneUtility.Cyan, 16);
            Button confirm = TW08ProductionSceneUtility.CreateButton(shell, "Confirm", "ATIVAR OPERADOR", TW08ProductionSceneUtility.Green, 17);
            Button back = TW08ProductionSceneUtility.CreateButton(shell, "Back", "VOLTAR", TW08ProductionSceneUtility.TextMuted, 15);
            TW08ProductionSceneUtility.SetRect((RectTransform)previous.transform, new Vector2(0f, 0.5f), new Vector2(200f, 52f), new Vector2(500f, -200f));
            TW08ProductionSceneUtility.SetRect((RectTransform)next.transform, new Vector2(0f, 0.5f), new Vector2(200f, 52f), new Vector2(720f, -200f));
            TW08ProductionSceneUtility.SetRect((RectTransform)confirm.transform, new Vector2(0f, 0.5f), new Vector2(280f, 52f), new Vector2(940f, -200f));
            TW08ProductionSceneUtility.SetRect((RectTransform)back.transform, new Vector2(0f, 0f), new Vector2(180f, 50f), new Vector2(65f, 48f));

            CharacterSelectController controller = new GameObject("Character Select Controller").AddComponent<CharacterSelectController>();
            controller.Configure(roster, name, role, description, status, portrait, previous, next, confirm, back, "TW08_ModeSelect");
            TW08ProductionSceneUtility.Select(eventSystem, confirm);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, OperatorPath);
        }

        private static void BuildPuzzleSelect(PuzzleCampaignDefinition campaign)
        {
            Scene scene = CreateMenuShell("CAMPANHA // ROTAS", "9 FASES OPERACIONAIS", out Transform shell, out EventSystem eventSystem);
            Text operatorText = TW08ProductionSceneUtility.CreateText(shell, "Operator", "OPERADOR // JOHN", 16, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(operatorText.rectTransform, new Vector2(1f, 1f), new Vector2(470f, 40f), new Vector2(-58f, -118f));
            List<Button> buttons = new();
            for (int i = 0; i < 9; i++)
            {
                int col = i % 3;
                int row = i / 3;
                Button button = TW08ProductionSceneUtility.CreateButton(shell, "Level " + (i + 1), $"{i + 1:00} // ROTA", i < 3 ? TW08ProductionSceneUtility.Green : TW08ProductionSceneUtility.Cyan, 15);
                Vector2 pos = new(-410f + col * 410f, 125f - row * 155f);
                TW08ProductionSceneUtility.SetRect((RectTransform)button.transform, new Vector2(0.5f, 0.5f), new Vector2(360f, 120f), pos);
                buttons.Add(button);
            }
            Button back = TW08ProductionSceneUtility.CreateButton(shell, "Back", "VOLTAR", TW08ProductionSceneUtility.TextMuted, 15);
            TW08ProductionSceneUtility.SetRect((RectTransform)back.transform, new Vector2(0f, 0f), new Vector2(180f, 50f), new Vector2(60f, 44f));
            PuzzleLevelSelectController controller = new GameObject("Puzzle Level Select Controller").AddComponent<PuzzleLevelSelectController>();
            controller.Configure(campaign, buttons, operatorText, back, "TW08_ModeSelect");
            TW08ProductionSceneUtility.Select(eventSystem, buttons.Count > 0 ? buttons[0] : back);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, PuzzleSelectPath);
        }

        private static void BuildRaceSelect(RaceCampaignDefinition campaign)
        {
            Scene scene = CreateMenuShell("N-8 LOGISTICS RUSH", "SELEÇÃO DE PISTA", out Transform shell, out EventSystem eventSystem);
            Text pilot = TW08ProductionSceneUtility.CreateText(shell, "Pilot", "PILOTO // JOHN", 16, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(pilot.rectTransform, new Vector2(1f, 1f), new Vector2(470f, 40f), new Vector2(-58f, -118f));
            List<Button> buttons = new();
            string[] names = { "RECEIVING LOOP", "INDUSTRIAL CORRIDOR", "FROZEN ROUTE" };
            for (int i = 0; i < 3; i++)
            {
                Button button = TW08ProductionSceneUtility.CreateButton(shell, "Track " + (i + 1), $"{i + 1:00} // {names[i]}", i == 0 ? TW08ProductionSceneUtility.Green : TW08ProductionSceneUtility.Cyan, 18);
                TW08ProductionSceneUtility.SetRect((RectTransform)button.transform, new Vector2(0.5f, 0.5f), new Vector2(780f, 105f), new Vector2(0f, 150f - i * 145f));
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
            Scene scene = CreateMenuShell("CONFIGURAÇÕES", "ÁUDIO // PERFIL LOCAL", out Transform shell, out EventSystem eventSystem);
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
            TW08ProductionSceneUtility.Select(eventSystem, back);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, SettingsPath);
        }

        private static void BuildCredits()
        {
            Scene scene = CreateMenuShell("CRÉDITOS", "THE WAREHOUSE Nº 08", out Transform shell, out EventSystem eventSystem);
            Text body = TW08ProductionSceneUtility.CreateText(
                shell, "Credits Body",
                "DIREÇÃO / PRODUÇÃO\nFERNANDO PERALTA\n\nDESENVOLVIMENTO\nUNITY 6.3 LTS // C#\n\nDIREÇÃO VISUAL\nINDUSTRIAL RETRO-FUTURISTA N-8\n\nPERSONAGENS\nJOHN MILLER // MARIA EDUARDA 'DUDA' // ROBERT 'BIG ROB' HAYES\n\nPROJETO ORIGINAL // THE WAREHOUSE Nº 08",
                20, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(960f, 520f), new Vector2(0f, 20f));
            Button back = TW08ProductionSceneUtility.CreateButton(shell, "Back", "VOLTAR", TW08ProductionSceneUtility.Green, 16);
            TW08ProductionSceneUtility.SetRect((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(260f, 54f), new Vector2(0f, 48f));
            SimpleBackNavigationController controller = new GameObject("Credits Navigation").AddComponent<SimpleBackNavigationController>();
            controller.Configure(back, "TW08_ModeSelect");
            TW08ProductionSceneUtility.Select(eventSystem, back);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, CreditsPath);
        }

        private static Scene CreateMenuShell(string titleText, string subtitleText, out Transform shell, out EventSystem eventSystem)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            TW08ProductionSceneUtility.CreateCamera(new Vector3(0f, 0f, -10f), 5f);
            Canvas canvas = TW08ProductionSceneUtility.CreateCanvas();
            eventSystem = TW08ProductionSceneUtility.CreateEventSystem();
            Image backdrop = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Backdrop", TW08ProductionSceneUtility.Background);
            TW08ProductionSceneUtility.Stretch(backdrop.rectTransform);
            Image panel = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Terminal Shell", TW08ProductionSceneUtility.Panel);
            TW08ProductionSceneUtility.SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1480f, 860f), Vector2.zero);
            shell = panel.transform;
            Text eyebrow = TW08ProductionSceneUtility.CreateText(shell, "Eyebrow", "N-8 LOGISTICS // CENTRAL", 15, TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(900f, 35f), new Vector2(0f, -42f));
            Text title = TW08ProductionSceneUtility.CreateText(shell, "Title", titleText, 34, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(1100f, 58f), new Vector2(0f, -88f));
            Text subtitle = TW08ProductionSceneUtility.CreateText(shell, "Subtitle", subtitleText, 16, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(1100f, 38f), new Vector2(0f, -135f));
            return scene;
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
            configSerialized.FindProperty("saveVersion").intValue = 2;
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
