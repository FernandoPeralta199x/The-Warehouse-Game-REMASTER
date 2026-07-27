using UnityEngine;

namespace TW08.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "TW08/Core/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [SerializeField] private int saveVersion = 1;
        [SerializeField] private string saveFileName = "tw08-save.json";
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string firstPuzzleScene = "PuzzlePrototype";
        [SerializeField] private string firstRaceScene = "RacePrototype";

        public int SaveVersion => Mathf.Max(1, saveVersion);
        public string SaveFileName => string.IsNullOrWhiteSpace(saveFileName) ? "tw08-save.json" : saveFileName;
        public string MainMenuScene => mainMenuScene;
        public string FirstPuzzleScene => firstPuzzleScene;
        public string FirstRaceScene => firstRaceScene;
    }
}
