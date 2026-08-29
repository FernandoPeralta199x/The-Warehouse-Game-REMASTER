using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Core;
using TW08.Data;
using TW08.Economy;
using TW08.Puzzle;
using TW08.Race;
using UnityEngine;

namespace TW08.Save
{
    /// <summary>Resultado consolidado de um turno, pronto para exibição.</summary>
    public struct PuzzleShiftReport
    {
        public PuzzleRunSummary Summary;
        public int CreditsEarned;
        public int CreditBalance;
        public IReadOnlyList<CreditEntry> Statement;
    }

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

            SaveMigrationPipeline migrations = new(new ISaveMigration[]
            {
                new SaveMigrationV1ToV2(),
                new SaveMigrationV2ToV3()
            });
            service = new JsonSaveService(config, migrations);
            Data = service.Load();
            Data.EnsureDefaults();
            CharacterSelectionState.Select(Data.selectedCharacterId);
            ApplyAudioSettingsToRuntime();
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

        /// <summary>
        /// Registra a entrada numa fase. O contador de tentativas alimenta o
        /// bônus de "primeira tentativa" e precisa subir antes do turno começar.
        /// </summary>
        public int RegisterPuzzleAttempt(string levelId)
        {
            if (Data == null || string.IsNullOrWhiteSpace(levelId)) return 0;
            LevelProgressRecord record = Data.GetOrCreateLevel(levelId);
            record.attempts = Mathf.Max(0, record.attempts) + 1;

            // Marca onde o jogador está para o "Continuar" do menu principal.
            // O campo existia desde o começo e nunca era escrito: ficava no
            // valor padrão para sempre, e continuar levaria sempre à fase 01.
            //
            // Guarda o nome da CENA, não o id da fase: as nove fases originais
            // têm cena com nome diferente do id, e é a cena que se carrega.
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                Data.lastUnlockedLevel = sceneName;
            }

            Save();
            return record.attempts;
        }

        /// <summary>
        /// Fecha o turno: grava recordes (separando limpo de assistido),
        /// credita os Créditos de Turno e devolve o extrato para a HUD.
        /// </summary>
        public PuzzleShiftReport CommitPuzzleShift(PuzzleLevelDefinition level, PuzzleRunSummary summary)
        {
            if (Data == null || level == null)
            {
                return default;
            }

            LevelProgressRecord record = Data.GetOrCreateLevel(level.LevelId);
            bool personalBest = record.bestMoves <= 0 || summary.Moves < record.bestMoves;

            summary.Medal = PuzzleProgressStore.EvaluateMedal(level, summary.Moves);
            summary.PersonalBest = personalBest;
            summary.FirstAttempt = record.attempts <= 1;

            record.completed = true;
            if (personalBest) record.bestMoves = Mathf.Max(0, summary.Moves);
            record.medal = Mathf.Max(record.medal, summary.Medal);

            // Ranking competitivo: só turnos sem ferramenta e sem dica entram.
            if (summary.IsClean)
            {
                if (record.bestCleanMoves <= 0 || summary.Moves < record.bestCleanMoves)
                {
                    record.bestCleanMoves = Mathf.Max(0, summary.Moves);
                }

                record.cleanMedal = Mathf.Max(record.cleanMedal, summary.Medal);
            }

            int earned = ShiftCredits.Evaluate(summary);
            Data.credits = Mathf.Max(0, Data.credits + earned);
            Save();

            return new PuzzleShiftReport
            {
                Summary = summary,
                CreditsEarned = earned,
                CreditBalance = Data.credits,
                Statement = ShiftCredits.BuildStatement(summary)
            };
        }

        /// <summary>Compra uma ferramenta se houver saldo. Devolve false sem gastar nada.</summary>
        public bool TryPurchaseTool(PuzzleToolDefinition tool)
        {
            if (Data == null || tool == null || Data.credits < tool.Price)
            {
                return false;
            }

            Data.credits -= tool.Price;
            Data.AddToolCount(tool.ToolId, 1);
            Save();
            return true;
        }

        /// <summary>Consome uma unidade do estoque ao acionar a ferramenta numa fase.</summary>
        public bool TryConsumeTool(string toolId)
        {
            if (Data == null || Data.GetToolCount(toolId) <= 0)
            {
                return false;
            }

            Data.AddToolCount(toolId, -1);
            Save();
            return true;
        }

        /// <summary>Define o loadout do próximo turno, respeitando o limite de slots.</summary>
        public void SetEquippedTools(IEnumerable<string> toolIds, int slots)
        {
            if (Data == null) return;
            Data.equippedTools = (toolIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(Mathf.Max(1, slots))
                .ToList();
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
            ApplyAudioSettingsToRuntime();
            Save();
        }

        public void Save()
        {
            if (Data == null) return;
            Data.EnsureDefaults();
            service?.Save(Data);
        }

        private void ApplyAudioSettingsToRuntime()
        {
            AudioListener.volume = Data.masterVolume;
            PlayerPrefs.SetFloat("tw08.audio.master", Data.masterVolume);
            PlayerPrefs.SetFloat("tw08.audio.music", Data.musicVolume);
            PlayerPrefs.SetFloat("tw08.audio.sfx", Data.sfxVolume);
            PlayerPrefs.Save();
        }
    }
}
