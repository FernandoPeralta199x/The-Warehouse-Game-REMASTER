using System;
using System.Collections;
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
    }
}
