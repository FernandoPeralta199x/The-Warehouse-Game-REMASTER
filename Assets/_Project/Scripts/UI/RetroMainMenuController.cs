using TW08.Save;
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
            RefreshContinueButton();
            if (versionText != null) versionText.text = $"BUILD {Application.version} // UNITY 6.3 LTS";
        }

        /// <summary>
        /// Continuar só existe quando há turno para retomar. Um botão aceso que
        /// não leva a lugar nenhum é pior do que um botão apagado.
        /// </summary>
        private void RefreshContinueButton()
        {
            SaveManager saveManager = FindFirstObjectByType<SaveManager>();
            SaveGameData data = saveManager != null ? saveManager.Data : null;
            bool hasProgress = data != null
                               && data.levels != null
                               && data.levels.Exists(record => record != null && record.completed);

            if (continueButton != null)
            {
                continueButton.interactable = hasProgress;
                Text label = continueButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = hasProgress ? "CONTINUAR TURNO" : "CONTINUAR [SEM REGISTRO]";
                }
            }
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
            SaveManager saveManager = FindFirstObjectByType<SaveManager>();
            string target = saveManager?.Data?.lastUnlockedLevel;

            // Sem registro não há o que retomar: recusa com tremor em vez de
            // carregar uma cena que não existe.
            if (string.IsNullOrWhiteSpace(target))
            {
                MenuFeedback.Denied(continueButton);
                return;
            }

            MenuTransition.Go(target, "último turno");
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
