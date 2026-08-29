using TW08.UI.Menus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class RetroMainMenuController : MonoBehaviour
    {
        [SerializeField] private string firstLevelScene = "TW08_ModeSelect";
        [SerializeField] private Button firstSelectedButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text versionText;

        public void Configure(Button firstSelected, Button continueControl, Text versionLabel = null)
        {
            firstSelectedButton = firstSelected;
            continueButton = continueControl;
            versionText = versionLabel;

#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            ResolveSceneReferences();
            if (continueButton != null) continueButton.interactable = false;
            if (versionText != null) versionText.text = $"BUILD {Application.version} // UNITY 6.3 LTS";
        }

        private void Start() => SelectInitialButton();

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null) SelectInitialButton();
        }

        public void StartNewShift()
        {
            MenuTransition.Go(firstLevelScene, "central de operações");
        }

        public void ContinueShift()
        {
            // Reservado para retomada direta. Até lá o botão recusa com tremor em
            // vez de não fazer nada — silêncio parece bug.
            MenuFeedback.Denied(continueButton);
        }

        public void OpenOptions() => Debug.Log("Options shell is reserved for the settings milestone.");

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
            if (firstSelectedButton == null) firstSelectedButton = FindButton("New Shift");
            if (continueButton == null) continueButton = FindButton("Continue");
        }

        private static Button FindButton(string objectName)
        {
            GameObject candidate = GameObject.Find(objectName);
            return candidate != null ? candidate.GetComponent<Button>() : null;
        }

        private void SelectInitialButton()
        {
            if (firstSelectedButton == null || EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }
}
