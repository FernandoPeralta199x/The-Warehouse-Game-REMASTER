using UnityEngine;

namespace TW08.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "TW08/Core/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [SerializeField] private int saveVersion = 2;
        [SerializeField] private string saveFileName = "tw08-save.json";
        [SerializeField] private string mainMenuScene = "TW08_MainMenu";
        [SerializeField] private string firstPuzzleScene = "TW08_Level01_FirstShift";
        [SerializeField] private string firstRaceScene = "TW08_Race01_ReceivingLoop";

        public int SaveVersion => Mathf.Max(1, saveVersion);
        public string SaveFileName => string.IsNullOrWhiteSpace(saveFileName) ? "tw08-save.json" : saveFileName;
        public string MainMenuScene => mainMenuScene;
        public string FirstPuzzleScene => firstPuzzleScene;
        public string FirstRaceScene => firstRaceScene;
    }
}
