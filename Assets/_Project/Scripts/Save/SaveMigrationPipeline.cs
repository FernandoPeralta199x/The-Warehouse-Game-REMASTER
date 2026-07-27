using System;
using System.Collections.Generic;
using System.Linq;

namespace TW08.Save
{
    public interface ISaveMigration
    {
        int FromVersion { get; }
        int ToVersion { get; }
        SaveGameData Migrate(SaveGameData data);
    }

    public sealed class SaveMigrationPipeline
    {
        private readonly Dictionary<int, ISaveMigration> bySourceVersion;

        public SaveMigrationPipeline(IEnumerable<ISaveMigration> migrations)
        {
            bySourceVersion = (migrations ?? Array.Empty<ISaveMigration>())
                .ToDictionary(migration => migration.FromVersion);
        }

        public SaveGameData MigrateTo(SaveGameData data, int targetVersion)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            while (data.version < targetVersion)
            {
                if (!bySourceVersion.TryGetValue(data.version, out ISaveMigration migration))
                {
                    throw new InvalidOperationException($"No save migration registered from version {data.version}.");
                }

                data = migration.Migrate(data) ?? throw new InvalidOperationException("Save migration returned null.");
                data.version = migration.ToVersion;
            }

            return data;
        }
    }
}
