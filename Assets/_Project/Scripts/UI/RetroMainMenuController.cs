using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class RetroMainMenuController : MonoBehaviour
    {
        [SerializeField] private string firstLevelScene = "TW08_Level01_FirstShift";
        [SerializeField] private Button firstSelectedButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text versionText;

        public void Configure(Button firstSelected, Button continueControl, Text versionLabel = null)
        {
            firstSelectedButton = firstSelected;
            continueButton = continueControl;
            versionText = versionLabel;
        }

        private void Awake()
        {
            ResolveSceneReferences();

            if (continueButton != null)
            {
                continueButton.interactable = false;
            }

            if (versionText != null)
            {
                versionText.text = $"BUILD {Application.version} // UNITY 6.3 LTS";
            }
        }

        private void Start()
        {
            SelectInitialButton();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                SelectInitialButton();
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

        private void ResolveSceneReferences()
        {
            if (firstSelectedButton == null)
            {
                firstSelectedButton = FindButton("New Shift");
            }

            if (continueButton == null)
            {
                continueButton = FindButton("Continue");
            }
        }

        private static Button FindButton(string objectName)
        {
            GameObject candidate = GameObject.Find(objectName);
            return candidate != null ? candidate.GetComponent<Button>() : null;
        }

        private void SelectInitialButton()
        {
            if (firstSelectedButton == null || EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }
}
