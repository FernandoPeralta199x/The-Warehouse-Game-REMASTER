using UnityEngine;

namespace TW08.Audio
{
    [CreateAssetMenu(fileName = "TW08AudioCatalog", menuName = "TW08/Audio/Production Catalog")]
    public sealed class TW08AudioCatalog : ScriptableObject
    {
        [Header("UI")]
        [SerializeField] private AudioEvent uiConfirm;
        [SerializeField] private AudioEvent uiBack;
        [SerializeField] private AudioEvent uiFocus;
        [SerializeField] private AudioEvent uiDenied;
        [SerializeField] private AudioEvent terminalBoot;

        [Header("Puzzle")]
        [SerializeField] private AudioEvent puzzleStep;
        [SerializeField] private AudioEvent puzzlePush;
        [SerializeField] private AudioEvent puzzleSuccess;
        [SerializeField] private AudioEvent puzzleError;

        [Header("Carga")]
        [SerializeField] private AudioEvent cratePushHeavy;
        [SerializeField] private AudioEvent crateHit;
        [SerializeField] private AudioEvent crateOnGoal;

        [Header("Maquinário")]
        [SerializeField] private AudioEvent doorOpen;
        [SerializeField] private AudioEvent doorClose;
        [SerializeField] private AudioEvent sensorOn;
        [SerializeField] private AudioEvent sensorOff;
        [SerializeField] private AudioEvent conveyorLoop;
        [SerializeField] private AudioEvent lockdownAlarm;

        [Header("Ferramentas — Oficina N-8")]
        [SerializeField] private AudioEvent toolRewind;
        [SerializeField] private AudioEvent toolScanner;
        [SerializeField] private AudioEvent toolAssistant;
        [SerializeField] private AudioEvent toolMarker;
        [SerializeField] private AudioEvent shopPurchase;
        [SerializeField] private AudioEvent creditsTick;

        [Header("Medalhas")]
        [SerializeField] private AudioEvent medalBronze;
        [SerializeField] private AudioEvent medalGold;
        [SerializeField] private AudioEvent medalPlatinum;

        [Header("Narrativa")]
        [SerializeField] private AudioEvent voiceJohn;
        [SerializeField] private AudioEvent voiceDuda;
        [SerializeField] private AudioEvent voiceRobert;

        [Header("Race")]
        [SerializeField] private AudioEvent raceCountdown;
        [SerializeField] private AudioEvent raceFinish;
        [SerializeField] private AudioEvent forkliftEngine;
        [SerializeField] private AudioEvent forkliftReverse;
        [SerializeField] private AudioEvent forkliftImpact;

        [Header("Ambiência")]
        [SerializeField] private AudioEvent warehouseAmbience;
        [SerializeField] private AudioEvent freezerAmbience;

        [Header("Music")]
        [SerializeField] private MusicTrack menuMusic;
        [SerializeField] private MusicTrack puzzleMusic;
        [SerializeField] private MusicTrack raceMusic;

        public AudioEvent UiConfirm => uiConfirm;
        public AudioEvent UiBack => uiBack;
        public AudioEvent UiFocus => uiFocus;
        public AudioEvent UiDenied => uiDenied;
        public AudioEvent TerminalBoot => terminalBoot;

        public AudioEvent PuzzleStep => puzzleStep;
        public AudioEvent PuzzlePush => puzzlePush;
        public AudioEvent PuzzleSuccess => puzzleSuccess;
        public AudioEvent PuzzleError => puzzleError;

        public AudioEvent CratePushHeavy => cratePushHeavy;
        public AudioEvent CrateHit => crateHit;
        public AudioEvent CrateOnGoal => crateOnGoal;

        public AudioEvent DoorOpen => doorOpen;
        public AudioEvent DoorClose => doorClose;
        public AudioEvent SensorOn => sensorOn;
        public AudioEvent SensorOff => sensorOff;
        public AudioEvent ConveyorLoop => conveyorLoop;
        public AudioEvent LockdownAlarm => lockdownAlarm;

        public AudioEvent ToolRewind => toolRewind;
        public AudioEvent ToolScanner => toolScanner;
        public AudioEvent ToolAssistant => toolAssistant;
        public AudioEvent ToolMarker => toolMarker;
        public AudioEvent ShopPurchase => shopPurchase;
        public AudioEvent CreditsTick => creditsTick;

        public AudioEvent MedalBronze => medalBronze;
        public AudioEvent MedalGold => medalGold;
        public AudioEvent MedalPlatinum => medalPlatinum;

        public AudioEvent VoiceJohn => voiceJohn;
        public AudioEvent VoiceDuda => voiceDuda;
        public AudioEvent VoiceRobert => voiceRobert;

        public AudioEvent RaceCountdown => raceCountdown;
        public AudioEvent RaceFinish => raceFinish;
        public AudioEvent ForkliftEngine => forkliftEngine;
        public AudioEvent ForkliftReverse => forkliftReverse;
        public AudioEvent ForkliftImpact => forkliftImpact;

        public AudioEvent WarehouseAmbience => warehouseAmbience;
        public AudioEvent FreezerAmbience => freezerAmbience;

        public MusicTrack MenuMusic => menuMusic;
        public MusicTrack PuzzleMusic => puzzleMusic;
        public MusicTrack RaceMusic => raceMusic;

        /// <summary>Medalha 1/2/3 → stinger correspondente. Fora da faixa, nada toca.</summary>
        public AudioEvent MedalFor(int medal)
        {
            return medal switch
            {
                3 => medalPlatinum,
                2 => medalGold,
                1 => medalBronze,
                _ => null
            };
        }

        /// <summary>Marcador de fala do personagem. Desconhecido cai no do John.</summary>
        public AudioEvent VoiceFor(string speakerId)
        {
            if (string.IsNullOrWhiteSpace(speakerId))
            {
                return voiceJohn;
            }

            return speakerId.Trim().ToLowerInvariant() switch
            {
                "duda" => voiceDuda,
                "robert" => voiceRobert,
                _ => voiceJohn
            };
        }
    }
}
