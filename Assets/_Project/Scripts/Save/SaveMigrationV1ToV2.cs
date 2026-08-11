namespace TW08.Save
{
    public sealed class SaveMigrationV1ToV2 : ISaveMigration
    {
        public int FromVersion => 1;
        public int ToVersion => 2;

        public SaveGameData Migrate(SaveGameData data)
        {
            data ??= new SaveGameData();
            data.version = ToVersion;
            data.EnsureDefaults();
            return data;
        }
    }
}
