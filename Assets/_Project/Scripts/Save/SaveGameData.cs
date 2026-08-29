using System;
using System.Collections.Generic;
using System.Linq;

namespace TW08.Save
{
    [Serializable]
    public sealed class LevelProgressRecord
    {
        public string levelId;
        public int bestMoves;
        public float bestTimeSeconds;
        public int medal;
        public bool completed;

        /// <summary>Quantas vezes a fase foi iniciada — alimenta o bônus de primeira tentativa.</summary>
        public int attempts;

        /// <summary>Melhor resultado obtido sem ferramentas nem dicas (ranking competitivo).</summary>
        public int bestCleanMoves;

        /// <summary>Medalha do melhor turno limpo. Ferramentas não contam aqui.</summary>
        public int cleanMedal;
    }

    /// <summary>Quantidade possuída de uma ferramenta da Oficina N-8.</summary>
    [Serializable]
    public sealed class ToolStackRecord
    {
        public string toolId;
        public int count;
    }

    [Serializable]
    public sealed class RaceProgressRecord
    {
        public string trackId;
        public float bestTimeSeconds;
        public int medal;
        public bool completed;
    }

    [Serializable]
    public sealed class SaveGameData
    {
        public const int CurrentVersion = 3;

        public int version = CurrentVersion;
        public string selectedCharacterId = "john";
        public string lastUnlockedLevel = "TW08_Level01_FirstShift";

        /// <summary>Créditos de Turno — moeda da Oficina N-8.</summary>
        public int credits;

        /// <summary>Ferramentas compradas e ainda não gastas.</summary>
        public List<ToolStackRecord> ownedTools = new();

        /// <summary>Ferramentas levadas para a próxima fase (limitado pelos slots do catálogo).</summary>
        public List<string> equippedTools = new();

        public List<string> unlockedCharacters = new() { "john", "duda" };
        public List<LevelProgressRecord> levels = new();
        public List<RaceProgressRecord> races = new();
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;

        public LevelProgressRecord GetOrCreateLevel(string levelId)
        {
            levels ??= new List<LevelProgressRecord>();
            LevelProgressRecord record = levels.FirstOrDefault(item => item != null && item.levelId == levelId);
            if (record != null)
            {
                return record;
            }

            record = new LevelProgressRecord { levelId = levelId };
            levels.Add(record);
            return record;
        }

        public RaceProgressRecord GetOrCreateRace(string trackId)
        {
            races ??= new List<RaceProgressRecord>();
            RaceProgressRecord record = races.FirstOrDefault(item => item != null && item.trackId == trackId);
            if (record != null)
            {
                return record;
            }

            record = new RaceProgressRecord { trackId = trackId };
            races.Add(record);
            return record;
        }

        /// <summary>Quantidade possuída de uma ferramenta (0 quando nunca comprada).</summary>
        public int GetToolCount(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId) || ownedTools == null)
            {
                return 0;
            }

            ToolStackRecord record = ownedTools.FirstOrDefault(
                item => item != null && string.Equals(item.toolId, toolId, StringComparison.OrdinalIgnoreCase));
            return record != null ? Math.Max(0, record.count) : 0;
        }

        /// <summary>Soma <paramref name="delta"/> ao estoque e devolve o total resultante.</summary>
        public int AddToolCount(string toolId, int delta)
        {
            if (string.IsNullOrWhiteSpace(toolId))
            {
                return 0;
            }

            ownedTools ??= new List<ToolStackRecord>();
            ToolStackRecord record = ownedTools.FirstOrDefault(
                item => item != null && string.Equals(item.toolId, toolId, StringComparison.OrdinalIgnoreCase));

            if (record == null)
            {
                record = new ToolStackRecord { toolId = toolId, count = 0 };
                ownedTools.Add(record);
            }

            record.count = Math.Max(0, record.count + delta);
            return record.count;
        }

        public void EnsureDefaults()
        {
            selectedCharacterId = string.IsNullOrWhiteSpace(selectedCharacterId) ? "john" : selectedCharacterId;
            lastUnlockedLevel = string.IsNullOrWhiteSpace(lastUnlockedLevel) ? "TW08_Level01_FirstShift" : lastUnlockedLevel;
            levels ??= new List<LevelProgressRecord>();
            races ??= new List<RaceProgressRecord>();
            unlockedCharacters ??= new List<string>();
            ownedTools ??= new List<ToolStackRecord>();
            equippedTools ??= new List<string>();
            credits = Math.Max(0, credits);
            ownedTools.RemoveAll(item => item == null || string.IsNullOrWhiteSpace(item.toolId) || item.count <= 0);
            equippedTools.RemoveAll(string.IsNullOrWhiteSpace);
            if (!unlockedCharacters.Contains("john")) unlockedCharacters.Add("john");
            if (!unlockedCharacters.Contains("duda")) unlockedCharacters.Add("duda");
            masterVolume = Clamp01(masterVolume);
            musicVolume = Clamp01(musicVolume);
            sfxVolume = Clamp01(sfxVolume);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
