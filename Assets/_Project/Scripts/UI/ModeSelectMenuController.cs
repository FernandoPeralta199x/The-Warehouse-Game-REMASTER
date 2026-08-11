using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class ModeSelectMenuController : MonoBehaviour
    {
        [SerializeField] private string campaignScene = "TW08_PuzzleSelect";
        [SerializeField] private string raceScene = "TW08_RaceSelect";
        [SerializeField] private string operatorsScene = "TW08_OperatorSelect";
        [SerializeField] private string settingsScene = "TW08_Settings";
        [SerializeField] private string creditsScene = "TW08_Credits";
        [SerializeField] private string mainMenuScene = "TW08_MainMenu";

        public void OpenCampaign() => Load(campaignScene);
        public void OpenRace() => Load(raceScene);
        public void OpenOperators() => Load(operatorsScene);
        public void OpenSettings() => Load(settingsScene);
        public void OpenCredits() => Load(creditsScene);
        public void BackToMainMenu() => Load(mainMenuScene);

        private static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("TW08 menu navigation received an empty scene name.");
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
