#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TW08.Input;
using TW08.Presentation;
using TW08.Race;
using TW08.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.Editor
{
    internal static class TW08RaceSceneBuilder
    {
        internal const string SceneRoot = "Assets/_Project/Scenes/Production/Race";
        // Contagem de pistas deriva da RaceCampaign (dinâmica).

        internal static List<string> BuildAll(TW08ExpansionDataSetup.ExpansionData data)
        {
            TW08ProductionSceneUtility.EnsureFolder("Assets/_Project/Scenes/Production");
            TW08ProductionSceneUtility.EnsureFolder(SceneRoot);

            List<RaceSceneSpec> specs = LoadBuildSpecs();
            if (specs.Count == 0)
            {
                throw new InvalidOperationException(
                    "Race scene builder resolved zero stable track specs from the campaign.");
            }

            List<string> paths = new();
            foreach (RaceSceneSpec spec in specs)
            {
                RaceTrackDefinition track = AssetDatabase.LoadAssetAtPath<RaceTrackDefinition>(spec.TrackAssetPath);
                ForkliftStats stats = AssetDatabase.LoadAssetAtPath<ForkliftStats>(TW08ExpansionDataSetup.ForkliftStatsPath);

                if (track == null)
                {
                    throw new InvalidOperationException(
                        $"Race scene builder could not reload track asset '{spec.TrackAssetPath}'.");
                }
                if (stats == null)
                {
                    throw new InvalidOperationException(
                        $"Race scene builder could not reload forklift stats '{TW08ExpansionDataSetup.ForkliftStatsPath}'.");
                }

                string path = SceneRoot + "/" + spec.SceneName + ".unity";
                Build(track, stats, path);
                paths.Add(path);
            }

            if (paths.Count != specs.Count)
            {
                throw new InvalidOperationException(
                    $"Race scene builder produced {paths.Count}/{specs.Count} scene paths.");
            }

            return paths;
        }

        private static List<RaceSceneSpec> LoadBuildSpecs()
        {
            RaceCampaignDefinition campaign =
                AssetDatabase.LoadAssetAtPath<RaceCampaignDefinition>(TW08ExpansionDataSetup.RaceCampaignPath);
            if (campaign == null)
            {
                throw new InvalidOperationException(
                    $"Race campaign could not be loaded from '{TW08ExpansionDataSetup.RaceCampaignPath}'.");
            }

            List<RaceSceneSpec> specs = new(campaign.Tracks.Count);
            for (int i = 0; i < campaign.Tracks.Count; i++)
            {
                RaceTrackDefinition track = campaign.Tracks[i];
                if (track == null)
                {
                    throw new InvalidOperationException($"Race campaign track {i + 1:00} is null.");
                }

                string trackPath = AssetDatabase.GetAssetPath(track);
                if (string.IsNullOrWhiteSpace(trackPath))
                {
                    throw new InvalidOperationException(
                        $"Race campaign track {i + 1:00} does not resolve to a persistent asset path.");
                }
                if (string.IsNullOrWhiteSpace(track.SceneName))
                {
                    throw new InvalidOperationException(
                        $"Race campaign track {i + 1:00} has no scene name.");
                }

                specs.Add(new RaceSceneSpec(trackPath, track.SceneName));
            }

            return specs;
        }

        private static void Build(RaceTrackDefinition track, ForkliftStats stats, string path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameInput input = new GameObject("Game Input").AddComponent<GameInput>();
            RaceManager manager = new GameObject("Race Manager").AddComponent<RaceManager>();
            RaceCountdown countdown = manager.gameObject.AddComponent<RaceCountdown>();
            SetPrivateBool(manager, "autoStart", false);

            BuildTrackVisuals(track);
            List<RaceCheckpoint> checkpoints = CreateCheckpoints(manager);
            manager.Configure(checkpoints, track.Laps);
            EditorUtility.SetDirty(manager);

            CreateVehicle(input, manager, stats, track, out ArcadeForkliftController2D controller, out RacerProgress progress);
            RaceSessionController session = new GameObject("Race Session").AddComponent<RaceSessionController>();
            session.Configure(track, manager, countdown, controller, progress);

            CreateCamera();
            CreateHud(track, session);

            EditorUtility.SetDirty(input);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(progress);
            EditorUtility.SetDirty(session);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new InvalidOperationException($"Unity failed to save generated race scene '{path}'.");
            }
        }

        private static void BuildTrackVisuals(RaceTrackDefinition track)
        {
            Sprite floor = TW08ExpansionStarterArt.LoadRaceSprite("TrackFloor");
            Sprite barrier = TW08ExpansionStarterArt.LoadRaceSprite("Barrier");
            Sprite boost = TW08ExpansionStarterArt.LoadRaceSprite("Boost");
            Sprite ice = TW08ExpansionStarterArt.LoadRaceSprite("Ice");
            Sprite oil = TW08ExpansionStarterArt.LoadRaceSprite("Oil");

            for (int y = -5; y <= 5; y++)
            for (int x = -9; x <= 9; x++)
                TW08ProductionSceneUtility.CreateSprite($"Track Floor {x},{y}", new Vector3(x, y, 0f), floor, -20, Color.white);

            for (int x = -10; x <= 10; x++)
            {
                CreateBarrier(new Vector2(x, -6), barrier);
                CreateBarrier(new Vector2(x, 6), barrier);
            }
            for (int y = -5; y <= 5; y++)
            {
                CreateBarrier(new Vector2(-10, y), barrier);
                CreateBarrier(new Vector2(10, y), barrier);
            }

            for (int y = -2; y <= 2; y++)
            for (int x = -5; x <= 5; x++)
                CreateBarrier(new Vector2(x, y), barrier);

            CreateBoost(new Vector2(-1.5f, -4f), boost);
            CreateBoost(new Vector2(2f, 4f), boost);

            if (track.TrackId == "industrial-corridor")
            {
                CreateBarrier(new Vector2(-2, -4), barrier);
                CreateBarrier(new Vector2(-2, -3), barrier);
                CreateBarrier(new Vector2(2, 3), barrier);
                CreateBarrier(new Vector2(2, 4), barrier);
                CreateGripPatch(new Vector2(7f, 0f), new Vector2(2.5f, 2.2f), 0.68f, oil, new Color(1f, 1f, 1f, 0.85f));
            }

            if (track.TrackId == "frozen-route")
            {
                CreateGripPatch(new Vector2(0f, -4f), new Vector2(7f, 2.2f), track.SurfaceGrip, ice, new Color(0.78f, 0.95f, 1f, 0.8f));
                CreateGripPatch(new Vector2(0f, 4f), new Vector2(7f, 2.2f), track.SurfaceGrip, ice, new Color(0.78f, 0.95f, 1f, 0.8f));
                CreateGripPatch(new Vector2(7f, 0f), new Vector2(2.2f, 3.5f), 0.35f, ice, new Color(0.70f, 0.93f, 1f, 0.8f));
            }
        }

        private static void CreateBarrier(Vector2 position, Sprite sprite)
        {
            GameObject go = TW08ProductionSceneUtility.CreateSprite("Barrier", position, sprite, 10, Color.white);
            TW08ProductionSceneUtility.AddBoxCollider(go, Vector2.one, false);
        }

        private static void CreateBoost(Vector2 position, Sprite sprite)
        {
            GameObject go = TW08ProductionSceneUtility.CreateSprite("Boost Pad", position, sprite, 2, Color.white, new Vector3(1.7f, 1f, 1f));
            TW08ProductionSceneUtility.AddBoxCollider(go, new Vector2(1f, 0.8f), true);
            go.AddComponent<BoostPad>();
        }

        private static void CreateGripPatch(Vector2 position, Vector2 size, float grip, Sprite sprite, Color tint)
        {
            GameObject go = TW08ProductionSceneUtility.CreateSprite("Grip Zone", position, sprite, -5, tint, new Vector3(size.x, size.y, 1f));
            TW08ProductionSceneUtility.AddBoxCollider(go, Vector2.one, true);
            SurfaceGripZone zone = go.AddComponent<SurfaceGripZone>();
            SerializedObject serialized = new(zone);
            serialized.FindProperty("gripMultiplier").floatValue = Mathf.Clamp(grip, 0.15f, 2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(zone);
        }

        private static List<RaceCheckpoint> CreateCheckpoints(RaceManager manager)
        {
            Sprite sprite = TW08ExpansionStarterArt.LoadRaceSprite("Checkpoint");
            List<RaceCheckpoint> result = new();
            result.Add(CreateCheckpoint(manager, 0, new Vector2(-6.5f, -4f), new Vector2(1.35f, 0.7f), sprite));
            result.Add(CreateCheckpoint(manager, 1, new Vector2(7.5f, -4f), new Vector2(0.7f, 3f), sprite));
            result.Add(CreateCheckpoint(manager, 2, new Vector2(7.5f, 4f), new Vector2(3f, 0.7f), sprite));
            result.Add(CreateCheckpoint(manager, 3, new Vector2(-7.5f, 4f), new Vector2(0.7f, 3f), sprite));
            return result;
        }

        private static RaceCheckpoint CreateCheckpoint(RaceManager manager, int index, Vector2 position, Vector2 size, Sprite sprite)
        {
            GameObject go = TW08ProductionSceneUtility.CreateSprite($"Checkpoint {index}", position, sprite, 1, TW08ProductionSceneUtility.Green, new Vector3(size.x, size.y, 1f));
            TW08ProductionSceneUtility.AddBoxCollider(go, Vector2.one, true);
            RaceCheckpoint checkpoint = go.AddComponent<RaceCheckpoint>();
            checkpoint.Configure(manager, index);
            EditorUtility.SetDirty(checkpoint);
            return checkpoint;
        }

        private static GameObject CreateVehicle(
            GameInput input,
            RaceManager manager,
            ForkliftStats stats,
            RaceTrackDefinition track,
            out ArcadeForkliftController2D controller,
            out RacerProgress progress)
        {
            Sprite john = TW08ExpansionStarterArt.LoadRaceSprite("Forklift_John");
            Sprite duda = TW08ExpansionStarterArt.LoadRaceSprite("Forklift_Duda");
            GameObject go = TW08ProductionSceneUtility.CreateSprite("Player Forklift", new Vector3(-8.15f, -4f, 0f), john, 30, Color.white);
            go.transform.rotation = Quaternion.Euler(0f, 0f, -90f);

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.mass = 1.4f;
            body.linearDamping = 0.35f;
            body.angularDamping = 2f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            BoxCollider2D collider = TW08ProductionSceneUtility.AddBoxCollider(go, new Vector2(0.72f, 1.1f), false);
            collider.offset = new Vector2(0f, 0.06f);

            controller = go.AddComponent<ArcadeForkliftController2D>();
            controller.Configure(input, stats, true);
            controller.SetSurfaceGripMultiplier(track.SurfaceGrip);
            go.AddComponent<ForkliftDamage>();
            progress = go.AddComponent<RacerProgress>();
            progress.Configure(manager, "player");

            RaceSelectedVehiclePresenter presenter = go.AddComponent<RaceSelectedVehiclePresenter>();
            presenter.Configure(go.GetComponent<SpriteRenderer>(), john, duda);
            EditorUtility.SetDirty(presenter);
            EditorUtility.SetDirty(progress);
            EditorUtility.SetDirty(controller);
            return go;
        }

        private static void CreateCamera()
        {
            Camera camera = TW08ProductionSceneUtility.CreateCamera(new Vector3(0f, 0f, -10f), 6.65f);
            camera.backgroundColor = new Color(0.011f, 0.016f, 0.018f, 1f);
        }

        private static void CreateHud(RaceTrackDefinition track, RaceSessionController session)
        {
            Canvas canvas = TW08ProductionSceneUtility.CreateCanvas();
            EventSystem eventSystem = TW08ProductionSceneUtility.CreateEventSystem();

            Image top = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Race HUD Top", TW08ProductionSceneUtility.Panel);
            TW08ProductionSceneUtility.SetRect(top.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -104f), new Vector2(-18f, -18f));
            Text trackText = TW08ProductionSceneUtility.CreateText(top.transform, "Track", track.DisplayName.ToUpperInvariant(), 24, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleLeft);
            TW08ProductionSceneUtility.SetRect(trackText.rectTransform, new Vector2(0f, 0.5f), new Vector2(520f, 42f), new Vector2(26f, 18f));
            Text timer = TW08ProductionSceneUtility.CreateText(top.transform, "Timer", "00:00.000", 26, TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(timer.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(320f, 48f), new Vector2(0f, 12f));
            Text lap = TW08ProductionSceneUtility.CreateText(top.transform, "Lap", "VOLTA 01/03", 16, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(lap.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(300f, 34f), new Vector2(0f, -28f));
            Text pilot = TW08ProductionSceneUtility.CreateText(top.transform, "Pilot", "PILOTO // JOHN", 15, TW08ProductionSceneUtility.Cyan, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(pilot.rectTransform, new Vector2(1f, 0.5f), new Vector2(380f, 34f), new Vector2(-28f, 22f));
            Text best = TW08ProductionSceneUtility.CreateText(top.transform, "Best", "BEST --:--.---", 14, TW08ProductionSceneUtility.TextMuted, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(best.rectTransform, new Vector2(1f, 0.5f), new Vector2(380f, 32f), new Vector2(-28f, -20f));

            Text status = TW08ProductionSceneUtility.CreateText(canvas.transform, "Race Status", "3", 46, TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(status.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(580f, 80f), new Vector2(0f, 180f));

            Image bottom = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "Race HUD Bottom", TW08ProductionSceneUtility.Panel);
            TW08ProductionSceneUtility.SetRect(bottom.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 18f), new Vector2(-18f, 92f));
            Text controls = TW08ProductionSceneUtility.CreateText(bottom.transform, "Controls", "W/S ACELERAR/FREAR   A/D DIREÇÃO   SHIFT DRIFT   SPACE POWER-UP", 14, TW08ProductionSceneUtility.TextMuted, TextAnchor.MiddleLeft);
            TW08ProductionSceneUtility.SetRect(controls.rectTransform, new Vector2(0f, 0.5f), new Vector2(950f, 40f), new Vector2(26f, 0f));
            Button restart = TW08ProductionSceneUtility.CreateButton(bottom.transform, "Restart Race", "REINICIAR", TW08ProductionSceneUtility.Amber, 14);
            Button exit = TW08ProductionSceneUtility.CreateButton(bottom.transform, "Exit Race", "SAIR", TW08ProductionSceneUtility.Cyan, 14);
            TW08ProductionSceneUtility.SetRect((RectTransform)restart.transform, new Vector2(1f, 0.5f), new Vector2(170f, 48f), new Vector2(-280f, 0f));
            TW08ProductionSceneUtility.SetRect((RectTransform)exit.transform, new Vector2(1f, 0.5f), new Vector2(150f, 48f), new Vector2(-90f, 0f));

            RaceHudController hud = new GameObject("Race HUD Controller").AddComponent<RaceHudController>();
            hud.Configure(session, trackText, timer, lap, best, pilot, status, restart, exit, "TW08_RaceSelect");
            TW08ProductionSceneUtility.Select(eventSystem, restart);
            EditorUtility.SetDirty(hud);
        }

        private static void SetPrivateBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serialized = new(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(target);
        }

        private readonly struct RaceSceneSpec
        {
            public RaceSceneSpec(string trackAssetPath, string sceneName)
            {
                TrackAssetPath = trackAssetPath;
                SceneName = sceneName;
            }

            public string TrackAssetPath { get; }
            public string SceneName { get; }
        }
    }
}
#endif
