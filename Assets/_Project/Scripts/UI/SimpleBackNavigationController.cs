using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class SimpleBackNavigationController : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";

        public void Configure(Button button, string sceneName)
        {
            backButton = button;
            backScene = sceneName;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable() => backButton?.onClick.AddListener(Back);
        private void OnDisable() => backButton?.onClick.RemoveListener(Back);

        public void Back()
        {
            if (!string.IsNullOrWhiteSpace(backScene)) SceneManager.LoadScene(backScene, LoadSceneMode.Single);
        }
    }
}
