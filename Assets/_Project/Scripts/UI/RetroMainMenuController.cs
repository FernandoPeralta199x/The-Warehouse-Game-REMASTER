using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class RetroMainMenuController : MonoBehaviour
    {
        [SerializeField] private string firstLevelScene = "TW08_Level01_FirstShift";
        [SerializeField] private Button continueButton;
        [SerializeField] private Text versionText;

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.interactable = false;
            }

            if (versionText != null)
            {
                versionText.text = $"BUILD {Application.version} // UNITY 6.3 LTS";
            }
        }

        public void StartNewShift()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(firstLevelScene, LoadSceneMode.Single);
        }

        public void ContinueShift()
        {
            // Intentionally disabled until campaign progress is persisted and validated.
        }

        public void OpenOptions()
        {
            Debug.Log("Options screen is not part of the first vertical slice yet.");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
