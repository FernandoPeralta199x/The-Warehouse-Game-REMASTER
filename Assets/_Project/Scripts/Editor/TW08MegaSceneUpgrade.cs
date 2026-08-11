#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TW08.Input;
using TW08.PowerUps;
using TW08.Presentation;
using TW08.Race;
using TW08.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.Editor
{
    internal static class TW08MegaSceneUpgrade
    {
        internal static void Apply(TW08MegaContentSetup.MegaContentData content)
        {
            if (content == null || content.GraphicsProfile == null || content.RacePowerUpTable == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            UpgradeMainGraphicsService(content.GraphicsProfile);
            UpgradeMenus();
            UpgradeRaceScenes(content.GraphicsProfile, content.RacePowerUpTable);
            AssetDatabase.SaveAssets();
        }

        private static void UpgradeMainGraphicsService(TW08GraphicsProfile profile)
        {
            string path = TW08MenuSceneBuilder.MainMenuPath;
            RequireScene(path);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            TW08GraphicsDirector director = UnityEngine.Object.FindFirstObjectByType<TW08GraphicsDirector>();
            if (director == null)
            {
                GameObject root = GameObject.Find("Production Bootstrap");
                if (root == null)
                {
                    root = new GameObject("Graphics Runtime");
                }

                director = root.GetComponent<TW08GraphicsDirector>();
                if (director == null)
                {
                    director = root.AddComponent<TW08GraphicsDirector>();
                }
            }

            director.Configure(profile, true);
            EditorUtility.SetDirty(director);
            Save(scene, path);
        }

        private static void UpgradeMenus()
        {
            foreach (string path in GetMenuPaths())
            {
                RequireScene(path);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                GameObject shell = GameObject.Find("Terminal Shell");
                if (shell == null)
                {
                    Debug.LogWarning($"TW08 mega UI: Terminal Shell not found in '{path}'.");
                    continue;
                }

                CanvasGroup group = shell.GetComponent<CanvasGroup>();
                if (group == null) group = shell.AddComponent<CanvasGroup>();

                ProfessionalMenuPresenter presenter = shell.GetComponent<ProfessionalMenuPresenter>();
                if (presenter == null) presenter = shell.AddComponent<ProfessionalMenuPresenter>();
                presenter.Configure(group, shell.transform as RectTransform);

                MenuFocusAnimator focus = shell.GetComponent<MenuFocusAnimator>();
                if (focus == null) focus = shell.AddComponent<MenuFocusAnimator>();
                focus.Configure(shell.transform);

                EditorUtility.SetDirty(group);
                EditorUtility.SetDirty(presenter);
                EditorUtility.SetDirty(focus);
                Save(scene, path);
            }
        }

        private static void UpgradeRaceScenes(TW08GraphicsProfile profile, WeightedPowerUpTable table)
        {
            RaceCampaignDefinition campaign = AssetDatabase.LoadAssetAtPath<RaceCampaignDefinition>(TW08ExpansionDataSetup.RaceCampaignPath);
            ForkliftStats stats = AssetDatabase.LoadAssetAtPath<ForkliftStats>(TW08ExpansionDataSetup.ForkliftStatsPath);
            TW08ArtCatalog artCatalog = AssetDatabase.LoadAssetAtPath<TW08ArtCatalog>(TW08ProductionArtSetup.CatalogPath);
            if (campaign == null || stats == null)
            {
                throw new InvalidOperationException("TW08 mega race upgrade requires the production race campaign and forklift stats.");
            }

            foreach (RaceTrackDefinition trackAsset in campaign.Tracks)
            {
                if (trackAsset == null || string.IsNullOrWhiteSpace(trackAsset.SceneName))
                {
                    throw new InvalidOperationException("Race campaign contains an invalid track while applying mega update.");
                }

                string trackPath = AssetDatabase.GetAssetPath(trackAsset);
                RaceTrackDefinition track = AssetDatabase.LoadAssetAtPath<RaceTrackDefinition>(trackPath);
                if (track == null)
                {
                    throw new InvalidOperationException($"Race track could not be reloaded from '{trackPath}'.");
                }

                string scenePath = TW08RaceSceneBuilder.SceneRoot + "/" + track.SceneName + ".unity";
                RequireScene(scenePath);

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                RaceManager manager = UnityEngine.Object.FindFirstObjectByType<RaceManager>();
                RaceSessionController session = UnityEngine.Object.FindFirstObjectByType<RaceSessionController>();
                GameInput input = UnityEngine.Object.FindFirstObjectByType<GameInput>();
                ArcadeForkliftController2D player = UnityEngine.Object.FindObjectsByType<ArcadeForkliftController2D>(FindObjectsSortMode.None)
                    .FirstOrDefault(vehicle => vehicle != null && vehicle.PlayerControlled);

                if (manager == null || session == null || player == null)
                {
                    throw new InvalidOperationException($"Race scene '{scenePath}' is missing manager, session or player vehicle.");
                }

                RacerProgress playerProgress = player.GetComponent<RacerProgress>();
                ForkliftDamage playerDamage = player.GetComponent<ForkliftDamage>();
                if (playerProgress == null || playerDamage == null)
                {
                    throw new InvalidOperationException($"Race scene '{scenePath}' player is missing progress or damage components.");
                }

                RaceCargoController playerCargo = EnsureCargo(player.gameObject, player, artCatalog);
                PowerUpInventory playerInventory = EnsureVehiclePowerUps(
                    player.gameObject, input, player, playerDamage, playerCargo, manager, playerProgress, false);
                session.ConfigureCargo(playerCargo);
                EnsureCamera(scene, player, profile);
                EnsureSceneGraphicsDirector(profile);
                EnsureAiField(manager, stats, track, table, artCatalog);
                EnsureItemBoxes(manager, table);
                UpgradeRaceHud(session, playerInventory);

                EditorUtility.SetDirty(session);
                Save(scene, scenePath);
            }
        }

        private static RaceCargoController EnsureCargo(
            GameObject vehicle,
            ArcadeForkliftController2D controller,
            TW08ArtCatalog artCatalog)
        {
            RaceCargoController cargo = vehicle.GetComponent<RaceCargoController>();
            if (cargo == null)
            {
                cargo = vehicle.AddComponent<RaceCargoController>();
            }
            cargo.Configure(controller);

            Transform visual = vehicle.transform.Find("Cargo Load");
            if (visual == null && artCatalog != null && artCatalog.CrateDefault != null)
            {
                GameObject cargoVisual = new("Cargo Load");
                cargoVisual.transform.SetParent(vehicle.transform, false);
                cargoVisual.transform.localPosition = new Vector3(0f, 0.48f, 0f);
                cargoVisual.transform.localRotation = Quaternion.identity;
                cargoVisual.transform.localScale = new Vector3(0.48f, 0.48f, 1f);
                SpriteRenderer renderer = cargoVisual.AddComponent<SpriteRenderer>();
                renderer.sprite = artCatalog.CrateDefault;
                renderer.sortingOrder = 31;
                renderer.color = new Color(1f, 0.88f, 0.62f, 1f);
                EditorUtility.SetDirty(renderer);
            }

            EditorUtility.SetDirty(cargo);
            return cargo;
        }

        private static PowerUpInventory EnsureVehiclePowerUps(
            GameObject vehicle,
            GameInput input,
            ArcadeForkliftController2D controller,
            ForkliftDamage damage,
            RaceCargoController cargo,
            RaceManager manager,
            RacerProgress progress,
            bool ai)
        {
            PowerUpInventory inventory = vehicle.GetComponent<PowerUpInventory>();
            if (inventory == null) inventory = vehicle.AddComponent<PowerUpInventory>();

            PowerUpExecutor executor = vehicle.GetComponent<PowerUpExecutor>();
            if (executor == null) executor = vehicle.AddComponent<PowerUpExecutor>();
            executor.Configure(ai ? null : input, inventory, controller, damage, cargo);

            RaceRouteScanner scanner = vehicle.GetComponent<RaceRouteScanner>();
            if (scanner == null) scanner = vehicle.AddComponent<RaceRouteScanner>();
            scanner.Configure(manager, progress);

            if (!ai)
            {
                RaceImpactFeedback feedback = vehicle.GetComponent<RaceImpactFeedback>();
                if (feedback == null) feedback = vehicle.AddComponent<RaceImpactFeedback>();
                feedback.Configure(controller);
                EditorUtility.SetDirty(feedback);
            }

            EditorUtility.SetDirty(inventory);
            EditorUtility.SetDirty(executor);
            EditorUtility.SetDirty(scanner);
            return inventory;
        }

        private static void EnsureAiField(
            RaceManager manager,
            ForkliftStats stats,
            RaceTrackDefinition track,
            WeightedPowerUpTable table,
            TW08ArtCatalog artCatalog)
        {
            RaceAiDriver[] existing = UnityEngine.Object.FindObjectsByType<RaceAiDriver>(FindObjectsSortMode.None);
            if (existing.Length >= 3)
            {
                return;
            }

            foreach (RaceAiDriver ai in existing)
            {
                if (ai != null) UnityEngine.Object.DestroyImmediate(ai.gameObject);
            }

            Sprite john = TW08ExpansionStarterArt.LoadRaceSprite("Forklift_John");
            Sprite duda = TW08ExpansionStarterArt.LoadRaceSprite("Forklift_Duda");
            CreateAiVehicle(manager, stats, track, table, artCatalog, "ai-duda", new Vector2(-8.65f, -3.35f), duda != null ? duda : john,
                new Color(0.86f, 0.95f, 1f, 1f), 0.91f, 0.70f);
            CreateAiVehicle(manager, stats, track, table, artCatalog, "ai-heavy", new Vector2(-8.85f, -4.65f), john,
                new Color(1f, 0.74f, 0.34f, 1f), 0.84f, 0.42f);
            CreateAiVehicle(manager, stats, track, table, artCatalog, "ai-elite", new Vector2(-9.25f, -3.85f), john,
                new Color(0.72f, 1f, 0.72f, 1f), 0.98f, 0.82f);
        }

        private static void CreateAiVehicle(
            RaceManager manager,
            ForkliftStats stats,
            RaceTrackDefinition track,
            WeightedPowerUpTable table,
            TW08ArtCatalog artCatalog,
            string id,
            Vector2 position,
            Sprite sprite,
            Color tint,
            float skill,
            float aggression)
        {
            GameObject go = TW08ProductionSceneUtility.CreateSprite("Rival " + id, position, sprite, 29, tint);
            go.transform.rotation = Quaternion.Euler(0f, 0f, -90f);

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.mass = 1.35f;
            body.linearDamping = 0.38f;
            body.angularDamping = 2.2f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            BoxCollider2D collider = TW08ProductionSceneUtility.AddBoxCollider(go, new Vector2(0.70f, 1.05f), false);
            collider.offset = new Vector2(0f, 0.05f);

            ArcadeForkliftController2D controller = go.AddComponent<ArcadeForkliftController2D>();
            controller.Configure(null, stats, false);
            controller.SetSurfaceGripMultiplier(track != null ? track.SurfaceGrip : 1f);
            ForkliftDamage damage = go.AddComponent<ForkliftDamage>();
            RacerProgress progress = go.AddComponent<RacerProgress>();
            progress.Configure(manager, id);
            RaceCargoController cargo = EnsureCargo(go, controller, artCatalog);

            RaceAiDriver ai = go.AddComponent<RaceAiDriver>();
            ai.Configure(manager, controller, progress, skill, aggression);
            PowerUpInventory inventory = EnsureVehiclePowerUps(go, null, controller, damage, cargo, manager, progress, true);
            PowerUpExecutor executor = go.GetComponent<PowerUpExecutor>();
            RaceAiPowerUpDriver itemAi = go.AddComponent<RaceAiPowerUpDriver>();
            itemAi.Configure(manager, progress, inventory, executor);

            EditorUtility.SetDirty(body);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(damage);
            EditorUtility.SetDirty(progress);
            EditorUtility.SetDirty(cargo);
            EditorUtility.SetDirty(ai);
            EditorUtility.SetDirty(itemAi);
        }

        private static void EnsureItemBoxes(RaceManager manager, WeightedPowerUpTable table)
        {
            PowerUpPickup[] existing = UnityEngine.Object.FindObjectsByType<PowerUpPickup>(FindObjectsSortMode.None);
            if (existing.Length >= 4)
            {
                foreach (PowerUpPickup pickup in existing)
                {
                    if (pickup != null) pickup.Configure(table, manager);
                }
                return;
            }

            foreach (PowerUpPickup pickup in existing)
            {
                if (pickup != null) UnityEngine.Object.DestroyImmediate(pickup.gameObject);
            }

            Sprite sprite = TW08ExpansionStarterArt.LoadRaceSprite("Boost");
            Vector2[] positions =
            {
                new(-2.4f, -4f),
                new(7.5f, 0f),
                new(2.4f, 4f),
                new(-7.5f, 0f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject go = TW08ProductionSceneUtility.CreateSprite(
                    $"Item Box {i + 1:00}", positions[i], sprite, 5,
                    new Color(0.35f, 0.92f, 1f, 0.92f), new Vector3(0.72f, 0.72f, 1f));
                TW08ProductionSceneUtility.AddBoxCollider(go, new Vector2(1.1f, 1.1f), true);
                PowerUpPickup pickup = go.AddComponent<PowerUpPickup>();
                pickup.Configure(table, manager);
                EditorUtility.SetDirty(pickup);
            }
        }

        private static void EnsureCamera(Scene scene, ArcadeForkliftController2D player, TW08GraphicsProfile profile)
        {
            Camera camera = FindComponentInScene<Camera>(scene);
            if (camera == null)
            {
                throw new InvalidOperationException($"Race scene '{scene.path}' has no camera.");
            }

            TW08CameraRig2D rig = camera.GetComponent<TW08CameraRig2D>();
            if (rig == null) rig = camera.gameObject.AddComponent<TW08CameraRig2D>();
            rig.Configure(player.transform, player.Body, player, profile);
            EditorUtility.SetDirty(rig);
        }

        private static void EnsureSceneGraphicsDirector(TW08GraphicsProfile profile)
        {
            TW08GraphicsDirector director = UnityEngine.Object.FindFirstObjectByType<TW08GraphicsDirector>();
            if (director == null)
            {
                director = new GameObject("Scene Graphics Director").AddComponent<TW08GraphicsDirector>();
            }
            director.Configure(profile, false);
            EditorUtility.SetDirty(director);
        }

        private static void UpgradeRaceHud(RaceSessionController session, PowerUpInventory inventory)
        {
            RaceHudController hud = UnityEngine.Object.FindFirstObjectByType<RaceHudController>();
            if (hud == null)
            {
                return;
            }

            GameObject top = GameObject.Find("Race HUD Top");
            GameObject bottom = GameObject.Find("Race HUD Bottom");
            if (top == null || bottom == null)
            {
                return;
            }

            Text position = FindChildText(top.transform, "Position");
            if (position == null)
            {
                position = TW08ProductionSceneUtility.CreateText(
                    top.transform, "Position", "POS 01/04", 16,
                    TW08ProductionSceneUtility.Amber, TextAnchor.MiddleLeft);
                TW08ProductionSceneUtility.SetRect(
                    position.rectTransform, new Vector2(0f, 0.5f), new Vector2(280f, 30f), new Vector2(26f, -22f));
            }

            Text item = FindChildText(bottom.transform, "Item");
            if (item == null)
            {
                item = TW08ProductionSceneUtility.CreateText(
                    bottom.transform, "Item", "ITEM // --", 14,
                    TW08ProductionSceneUtility.Cyan, TextAnchor.MiddleCenter);
                TW08ProductionSceneUtility.SetRect(
                    item.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(420f, 40f), new Vector2(70f, 0f));
            }

            Text cargo = FindChildText(bottom.transform, "Cargo");
            if (cargo == null)
            {
                cargo = TW08ProductionSceneUtility.CreateText(
                    bottom.transform, "Cargo", "CARGA // 100%", 14,
                    TW08ProductionSceneUtility.Green, TextAnchor.MiddleCenter);
                TW08ProductionSceneUtility.SetRect(
                    cargo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(320f, 40f), new Vector2(-300f, 0f));
            }

            hud.ConfigureArcadeOverlay(position, item, cargo, inventory);
            EditorUtility.SetDirty(hud);
        }

        private static Text FindChildText(Transform root, string name)
        {
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                if (text != null && string.Equals(text.gameObject.name, name, StringComparison.Ordinal))
                {
                    return text;
                }
            }
            return null;
        }

        private static IEnumerable<string> GetMenuPaths()
        {
            yield return TW08MenuSceneBuilder.MainMenuPath;
            yield return TW08MenuSceneBuilder.ModePath;
            yield return TW08MenuSceneBuilder.OperatorPath;
            yield return TW08MenuSceneBuilder.PuzzleSelectPath;
            yield return TW08MenuSceneBuilder.RaceSelectPath;
            yield return TW08MenuSceneBuilder.SettingsPath;
            yield return TW08MenuSceneBuilder.CreditsPath;
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static void RequireScene(string assetPath)
        {
            string fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                assetPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"TW08 mega update requires scene '{assetPath}'. Run Repair Runtime Scene Registration first.",
                    fullPath);
            }
        }

        private static void Save(Scene scene, string path)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new InvalidOperationException($"TW08 mega update could not save scene '{path}'.");
            }
        }
    }
}
#endif
