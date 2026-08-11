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
        private static bool runtimeTransitionActive;

        [SerializeField] private bool destroyAfterLoad;

        public bool IsLoading { get; private set; }
        public event Action<float> ProgressChanged;
        public event Action<string> SceneLoaded;
        public event Action<string> SceneLoadFailed;

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
                if (Application.isPlaying)
                {
                    return BeginRuntimeAsyncLoad(sceneName, context);
                }

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
                " and no matching .unity file was found under Assets/_Project/Scenes. " +
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

        private static bool BeginRuntimeAsyncLoad(string sceneName, string context)
        {
            if (runtimeTransitionActive)
            {
                Debug.LogWarning($"TW08 ignored duplicate {context} load request for '{sceneName}' because a transition is already running.");
                return false;
            }

            runtimeTransitionActive = true;
            GameObject host = new("TW08 Runtime Scene Loader");
            DontDestroyOnLoad(host);
            SceneLoader loader = host.AddComponent<SceneLoader>();
            loader.destroyAfterLoad = true;
            loader.SceneLoaded += _ => runtimeTransitionActive = false;
            loader.SceneLoadFailed += _ => runtimeTransitionActive = false;
            loader.Load(sceneName);
            return true;
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
                    CompleteSuccess(sceneName);
                    yield break;
                }
#endif
                CompleteFailure(sceneName, $"Unable to start loading scene '{sceneName}' because it is not registered for the player.");
                yield break;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                CompleteFailure(sceneName, $"Unable to start loading scene '{sceneName}'.");
                yield break;
            }

            while (!operation.isDone)
            {
                float normalized = Mathf.Clamp01(operation.progress / 0.9f);
                ProgressChanged?.Invoke(normalized);
                yield return null;
            }

            CompleteSuccess(sceneName);
        }

        private void CompleteSuccess(string sceneName)
        {
            IsLoading = false;
            ProgressChanged?.Invoke(1f);
            SceneLoaded?.Invoke(sceneName);
            if (destroyAfterLoad)
            {
                Destroy(gameObject);
            }
        }

        private void CompleteFailure(string sceneName, string message)
        {
            IsLoading = false;
            Debug.LogError(message);
            SceneLoadFailed?.Invoke(sceneName);
            if (destroyAfterLoad)
            {
                Destroy(gameObject);
            }
        }

#if UNITY_EDITOR
        private static string FindEditorScenePath(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            string[] candidatePaths =
            {
                $"Assets/_Project/Scenes/VerticalSlice/{sceneName}.unity",
                $"Assets/_Project/Scenes/Production/Race/{sceneName}.unity",
                $"Assets/_Project/Scenes/Production/Menus/{sceneName}.unity"
            };

            foreach (string candidate in candidatePaths)
            {
                if (EditorSceneFileExists(candidate))
                {
                    return candidate;
                }
            }

            string projectRoot = Directory.GetCurrentDirectory();
            string sceneRoot = Path.Combine(projectRoot, "Assets", "_Project", "Scenes");
            if (!Directory.Exists(sceneRoot))
            {
                return null;
            }

            foreach (string fullPath in Directory.EnumerateFiles(sceneRoot, "*.unity", SearchOption.AllDirectories))
            {
                if (!string.Equals(
                        Path.GetFileNameWithoutExtension(fullPath),
                        sceneName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string normalizedRoot = projectRoot.Replace('\\', '/').TrimEnd('/');
                string normalizedPath = fullPath.Replace('\\', '/');
                if (normalizedPath.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return normalizedPath.Substring(normalizedRoot.Length + 1);
                }
            }

            return null;
        }

        private static bool EditorSceneFileExists(string assetPath)
        {
            string fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                assetPath.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(fullPath);
        }
#endif
    }
}
