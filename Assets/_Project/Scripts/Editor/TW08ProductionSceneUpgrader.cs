#if UNITY_EDITOR
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
                PuzzleRuntime runtime = Object.FindFirstObjectByType<PuzzleRuntime>();
                GameObject john = GameObject.Find("John");

                if (runtime == null || john == null)
                {
                    Debug.LogError("TW08: production upgrade could not resolve PuzzleRuntime/John in " + scenePath);
                    skipped++;
                    continue;
                }

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

                EditorUtility.SetDirty(animator);
                EditorUtility.SetDirty(binder);
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
                "O movimento lógico continua por célula. A camada visual agora suporta interpolação curta e animação direcional do John via DirectionalSpriteSet.",
                "OK");
        }
    }
}
#endif
