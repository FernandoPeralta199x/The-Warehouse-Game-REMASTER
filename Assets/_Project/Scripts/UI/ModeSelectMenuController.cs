using TW08.Core;
using UnityEngine;

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

        public void OpenCampaign() => Load(campaignScene, "campanha");
        public void OpenRace() => Load(raceScene, "corrida");
        public void OpenOperators() => Load(operatorsScene, "operadores");
        public void OpenSettings() => Load(settingsScene, "configurações");
        public void OpenCredits() => Load(creditsScene, "créditos");
        public void BackToMainMenu() => Load(mainMenuScene, "menu principal");

        private static void Load(string sceneName, string context)
        {
            SceneLoader.TryLoadImmediate(sceneName, context);
        }
    }
}
