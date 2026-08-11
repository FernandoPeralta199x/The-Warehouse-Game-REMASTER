#if UNITY_EDITOR
using System;
using TW08.Presentation;
using TW08.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW08.Editor
{
    public static class TW08ProductionSceneUpgrader
    {
        private static readonly string[] PuzzleScenePaths =
        {
            "Assets/_Project/Scenes/VerticalSlice/TW08_Level01_FirstShift.unity",
            "Assets/_Project/Scenes/VerticalSlice/TW08_Level02_TightCorridor.unity",
            "Assets/_Project/Scenes/VerticalSlice/TW08_Level03_CrossLoad.unity"
        };

        private static readonly Color PuzzleBackground = new(0.018f, 0.023f, 0.026f, 1f);

        [MenuItem("Tools/TW08/Production/Upgrade Vertical Slice Presentation")]
        public static void UpgradeVerticalSlicePresentation()
        {
            TW08ArtCatalog catalog = TW08ProductionArtSetup.EnsureProductionArtAssets();
            string previouslyOpenScene = SceneManager.GetActiveScene().path;
            int upgraded = 0;
            int skipped = 0;

            foreach (string scenePath in PuzzleScenePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    Debug.LogWarning("TW08: scene not found, skipping production upgrade: " + scenePath);
                    skipped++;
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                PuzzleRuntime runtime = UnityEngine.Object.FindFirstObjectByType<PuzzleRuntime>();
                GameObject john = GameObject.Find("John");

                if (runtime == null || john == null)
                {
                    Debug.LogError("TW08: production upgrade could not resolve PuzzleRuntime/John in " + scenePath);
                    skipped++;
                    continue;
                }

                ConfigureCamera();
                ApplyEnvironmentSkin(catalog);
                ConfigureJohn(john, runtime, catalog);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                upgraded++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!string.IsNullOrWhiteSpace(previouslyOpenScene) && AssetDatabase.LoadAssetAtPath<SceneAsset>(previouslyOpenScene) != null)
            {
                EditorSceneManager.OpenScene(previouslyOpenScene, OpenSceneMode.Single);
            }
            else
            {
                string menuPath = "Assets/_Project/Scenes/VerticalSlice/TW08_MainMenu.unity";
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(menuPath) != null)
                {
                    EditorSceneManager.OpenScene(menuPath, OpenSceneMode.Single);
                }
            }

            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Production",
                "Presentation upgrade concluído.\n\n" +
                "Fases atualizadas: " + upgraded + "\n" +
                "Fases ignoradas: " + skipped + "\n\n" +
                "Starter pixel-art N-8 aplicado quando o catálogo possui sprite. " +
                "O movimento lógico continua determinístico por célula; animação e interpolação permanecem apenas na apresentação.",
                "OK");
        }

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                return;
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = PuzzleBackground;
            camera.orthographic = true;
            EditorUtility.SetDirty(camera);
        }

        private static void ConfigureJohn(GameObject john, PuzzleRuntime runtime, TW08ArtCatalog catalog)
        {
            SpriteRenderer renderer = john.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = john.AddComponent<SpriteRenderer>();
            }

            DirectionalSpriteAnimator animator = john.GetComponent<DirectionalSpriteAnimator>();
            if (animator == null)
            {
                animator = john.AddComponent<DirectionalSpriteAnimator>();
            }
            animator.Configure(renderer, catalog.John);

            PuzzleCharacterAnimationBinder binder = john.GetComponent<PuzzleCharacterAnimationBinder>();
            if (binder == null)
            {
                binder = john.AddComponent<PuzzleCharacterAnimationBinder>();
            }
            binder.Configure(runtime, animator);

            Sprite starterJohn = catalog.John != null ? catalog.John.GetIdle(FacingDirection.Down) : null;
            if (starterJohn != null)
            {
                RemovePrototypeRenderer(john);
                john.transform.localScale = Vector3.one;
                renderer.sprite = starterJohn;
                renderer.color = Color.white;
                renderer.sortingOrder = 5;
            }

            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(binder);
        }

        private static void ApplyEnvironmentSkin(TW08ArtCatalog catalog)
        {
            PrototypeSpriteRenderer[] prototypes = UnityEngine.Object.FindObjectsByType<PrototypeSpriteRenderer>(FindObjectsSortMode.None);
            foreach (PrototypeSpriteRenderer prototype in prototypes)
            {
                if (prototype == null)
                {
                    continue;
                }

                GameObject target = prototype.gameObject;
                Sprite sprite = ResolveProductionSprite(target, catalog, out int sortingOrder);
                if (sprite == null)
                {
                    continue;
                }

                SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = target.AddComponent<SpriteRenderer>();
                }

                UnityEngine.Object.DestroyImmediate(prototype);
                target.transform.localScale = Vector3.one;
                renderer.sprite = sprite;
                renderer.color = Color.white;
                renderer.sortingOrder = sortingOrder;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static Sprite ResolveProductionSprite(GameObject target, TW08ArtCatalog catalog, out int sortingOrder)
        {
            sortingOrder = 0;
            string objectName = target.name ?? string.Empty;

            if (objectName.StartsWith("Floor ", StringComparison.Ordinal))
            {
                sortingOrder = -2;
                return IsEvenFloorCell(objectName) ? catalog.FloorPrimary : catalog.FloorSecondary;
            }

            if (objectName.StartsWith("Wall ", StringComparison.Ordinal))
            {
                sortingOrder = 1;
                return catalog.Wall;
            }

            if (objectName.StartsWith("Goal ", StringComparison.Ordinal))
            {
                sortingOrder = 2;
                return catalog.Goal;
            }

            PuzzleEntityView entity = target.GetComponent<PuzzleEntityView>();
            if (entity != null && entity.Kind == PuzzleEntityKind.Crate)
            {
                sortingOrder = 4;
                return catalog.CrateDefault;
            }

            return null;
        }

        private static bool IsEvenFloorCell(string objectName)
        {
            string coordinate = objectName.Substring("Floor ".Length);
            string[] parts = coordinate.Split(',');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int x) || !int.TryParse(parts[1], out int y))
            {
                return true;
            }

            return (x + y) % 2 == 0;
        }

        private static void RemovePrototypeRenderer(GameObject target)
        {
            PrototypeSpriteRenderer prototype = target.GetComponent<PrototypeSpriteRenderer>();
            if (prototype != null)
            {
                UnityEngine.Object.DestroyImmediate(prototype);
            }
        }
    }
}
#endif