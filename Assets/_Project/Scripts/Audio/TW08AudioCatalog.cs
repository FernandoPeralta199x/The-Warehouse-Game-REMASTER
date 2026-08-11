using UnityEngine;

namespace TW08.Audio
{
    [CreateAssetMenu(fileName = "TW08AudioCatalog", menuName = "TW08/Audio/Production Catalog")]
    public sealed class TW08AudioCatalog : ScriptableObject
    {
        [Header("UI")]
        [SerializeField] private AudioEvent uiConfirm;

        [Header("Puzzle")]
        [SerializeField] private AudioEvent puzzleStep;
        [SerializeField] private AudioEvent puzzlePush;
        [SerializeField] private AudioEvent puzzleSuccess;
        [SerializeField] private AudioEvent puzzleError;

        [Header("Race")]
        [SerializeField] private AudioEvent raceCountdown;
        [SerializeField] private AudioEvent raceFinish;

        [Header("Music")]
        [SerializeField] private MusicTrack menuMusic;
        [SerializeField] private MusicTrack puzzleMusic;
        [SerializeField] private MusicTrack raceMusic;

        public AudioEvent UiConfirm => uiConfirm;
        public AudioEvent PuzzleStep => puzzleStep;
        public AudioEvent PuzzlePush => puzzlePush;
        public AudioEvent PuzzleSuccess => puzzleSuccess;
        public AudioEvent PuzzleError => puzzleError;
        public AudioEvent RaceCountdown => raceCountdown;
        public AudioEvent RaceFinish => raceFinish;
        public MusicTrack MenuMusic => menuMusic;
        public MusicTrack PuzzleMusic => puzzleMusic;
        public MusicTrack RaceMusic => raceMusic;
    }
}
