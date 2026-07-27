using TW08.Core;
using UnityEngine;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class LoadingScreenPresenter : MonoBehaviour
    {
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private CanvasGroup root;
        [SerializeField] private RectTransform progressFill;

        private void OnEnable()
        {
            if (sceneLoader == null)
            {
                return;
            }

            sceneLoader.ProgressChanged += OnProgress;
            sceneLoader.SceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (sceneLoader == null)
            {
                return;
            }

            sceneLoader.ProgressChanged -= OnProgress;
            sceneLoader.SceneLoaded -= OnSceneLoaded;
        }

        private void OnProgress(float progress)
        {
            if (root != null)
            {
                root.alpha = 1f;
                root.blocksRaycasts = true;
            }

            if (progressFill != null)
            {
                progressFill.anchorMax = new Vector2(Mathf.Clamp01(progress), progressFill.anchorMax.y);
            }
        }

        private void OnSceneLoaded(string _)
        {
            if (root != null)
            {
                root.alpha = 0f;
                root.blocksRaycasts = false;
            }
        }
    }
}
