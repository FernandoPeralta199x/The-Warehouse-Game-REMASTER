using System.Collections.Generic;

namespace TW08.Save
{
    /// <summary>
    /// v2 → v3: introduz a Oficina N-8 (Créditos de Turno, estoque de ferramentas)
    /// e o ranking limpo por fase.
    ///
    /// Saves antigos não sabiam distinguir turno limpo de assistido. Como nada
    /// nesses saves foi jogado com ferramentas — elas não existiam —, o melhor
    /// resultado já registrado é promovido a recorde limpo.
    /// </summary>
    public sealed class SaveMigrationV2ToV3 : ISaveMigration
    {
        public int FromVersion => 2;
        public int ToVersion => 3;

        public SaveGameData Migrate(SaveGameData data)
        {
            data ??= new SaveGameData();
            data.version = ToVersion;
            data.ownedTools ??= new List<ToolStackRecord>();
            data.equippedTools ??= new List<string>();
            data.levels ??= new List<LevelProgressRecord>();

            foreach (LevelProgressRecord record in data.levels)
            {
                if (record == null)
                {
                    continue;
                }

                if (record.completed)
                {
                    record.bestCleanMoves = record.bestMoves;
                    record.cleanMedal = record.medal;
                    record.attempts = record.attempts > 0 ? record.attempts : 1;
                }
            }

            data.EnsureDefaults();
            return data;
        }
    }
}
