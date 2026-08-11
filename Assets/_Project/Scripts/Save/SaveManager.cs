using TW08.Core;
using TW08.Data;
using TW08.Puzzle;
using TW08.Race;
using UnityEngine;

namespace TW08.Save
{
    [DisallowMultipleComponent]
    public sealed class SaveManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        private JsonSaveService service;

        public SaveGameData Data { get; private set; }

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("SaveManager requires GameConfig.", this);
                enabled = false;
                return;
            }

            SaveMigrationPipeline migrations = new(new ISaveMigration[] { new SaveMigrationV1ToV2() });
            service = new JsonSaveService(config, migrations);
            Data = service.Load();
            Data.EnsureDefaults();
            CharacterSelectionState.Select(Data.selectedCharacterId);
            AudioListener.volume = Data.masterVolume;
        }

        public void SelectCharacter(string characterId)
        {
            if (Data == null || string.IsNullOrWhiteSpace(characterId)) return;
            Data.selectedCharacterId = characterId.Trim().ToLowerInvariant();
            CharacterSelectionState.Select(Data.selectedCharacterId);
            Save();
        }

        public void RecordPuzzleCompletion(PuzzleLevelDefinition level, int moves)
        {
            if (Data == null || level == null) return;
            LevelProgressRecord record = Data.GetOrCreateLevel(level.LevelId);
            record.completed = true;
            if (record.bestMoves <= 0 || moves < record.bestMoves) record.bestMoves = Mathf.Max(0, moves);
            record.medal = Mathf.Max(record.medal, PuzzleProgressStore.EvaluateMedal(level, moves));
            Save();
        }

        public void RecordRaceCompletion(RaceTrackDefinition track, float timeSeconds, float cargoDamage = 0f)
        {
            if (Data == null || track == null || timeSeconds <= 0f) return;
            RaceProgressRecord record = Data.GetOrCreateRace(track.TrackId);
            record.completed = true;
            if (record.bestTimeSeconds <= 0f || timeSeconds < record.bestTimeSeconds) record.bestTimeSeconds = timeSeconds;
            record.medal = Mathf.Max(record.medal, track.GetMedal(timeSeconds, cargoDamage));
            Save();
        }

        public void UpdateAudioSettings(float master, float music, float sfx)
        {
            if (Data == null) return;
            Data.masterVolume = Mathf.Clamp01(master);
            Data.musicVolume = Mathf.Clamp01(music);
            Data.sfxVolume = Mathf.Clamp01(sfx);
            AudioListener.volume = Data.masterVolume;
            Save();
        }

        public void Save()
        {
            if (Data == null) return;
            Data.EnsureDefaults();
            service?.Save(Data);
        }
    }
}
