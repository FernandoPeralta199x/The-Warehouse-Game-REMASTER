using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW08.Core
{
    [DisallowMultipleComponent]
    public sealed class SceneLoader : MonoBehaviour
    {
        public bool IsLoading { get; private set; }
        public event Action<float> ProgressChanged;
        public event Action<string> SceneLoaded;

        public static bool TryLoadImmediate(string sceneName, string context = "scene")
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"TW08 cannot load {context}: scene name is empty.");
                return false;
            }

            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                return true;
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                string editorPath = FindEditorScenePath(sceneName);
                if (!string.IsNullOrWhiteSpace(editorPath))
                {
                    Debug.LogWarning(
                        $"TW08 Editor fallback: '{sceneName}' was not available through the active Scene List; " +
                        $"loading '{editorPath}' directly for Play Mode validation. A player build must still register this scene.");
                    UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                        editorPath,
                        new LoadSceneParameters(LoadSceneMode.Single));
                    return true;
                }
            }
#endif

            Debug.LogError(
                $"TW08 cannot load {context} scene '{sceneName}'. The scene is not available in the active/shared Scene List" +
#if UNITY_EDITOR
                " and no matching Scene asset was found under Assets/_Project/Scenes. " +
                "Run Tools > TW08 > Production > Repair Runtime Scene Registration."
#else
                ". The player build is missing this scene from its build profile."
#endif
            );
            return false;
        }

        public void Load(string sceneName)
        {
            if (IsLoading || string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
#if UNITY_EDITOR
                string editorPath = FindEditorScenePath(sceneName);
                if (Application.isPlaying && !string.IsNullOrWhiteSpace(editorPath))
                {
                    UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                        editorPath,
                        new LoadSceneParameters(LoadSceneMode.Single));
                    IsLoading = false;
                    ProgressChanged?.Invoke(1f);
                    SceneLoaded?.Invoke(sceneName);
                    yield break;
                }
#endif
                IsLoading = false;
                Debug.LogError($"Unable to start loading scene '{sceneName}' because it is not registered for the player.");
                yield break;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                IsLoading = false;
                Debug.LogError($"Unable to start loading scene '{sceneName}'.");
                yield break;
            }

            while (!operation.isDone)
            {
                float normalized = Mathf.Clamp01(operation.progress / 0.9f);
                ProgressChanged?.Invoke(normalized);
                yield return null;
            }

            IsLoading = false;
            ProgressChanged?.Invoke(1f);
            SceneLoaded?.Invoke(sceneName);
        }

#if UNITY_EDITOR
        private static string FindEditorScenePath(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            // Generated TW08 scenes live in three deterministic roots. Resolve those first so editor
            // playtests do not depend on AssetDatabase search indexing being up-to-date.
            string[] candidatePaths =
            {
                $"Assets/_Project/Scenes/VerticalSlice/{sceneName}.unity",
                $"Assets/_Project/Scenes/Production/Race/{sceneName}.unity",
                $"Assets/_Project/Scenes/Production/Menus/{sceneName}.unity"
            };

            foreach (string candidate in candidatePaths)
            {
                if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(candidate) != null)
                {
                    return candidate;
                }
            }

            // AssetDatabase type filters use class names. Scene files are represented by SceneAsset;
            // using t:Scene can return no results even when the .unity file exists.
            string[] guids = UnityEditor.AssetDatabase.FindAssets(
                sceneName + " t:SceneAsset",
                new[] { "Assets/_Project/Scenes" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileNameWithoutExtension(path), sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            return null;
        }
#endif
    }
}