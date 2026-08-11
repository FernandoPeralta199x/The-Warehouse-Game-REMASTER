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
            data.selectedCharacterId = string.IsNullOrWhiteSpace(data.selectedCharacterId) ? "john" : data.selectedCharacterId;
            if (string.IsNullOrWhiteSpace(data.lastUnlockedLevel)
                || data.lastUnlockedLevel == "prototype-001"
                || data.lastUnlockedLevel == "tw08-s01-001")
            {
                data.lastUnlockedLevel = "TW08_Level01_FirstShift";
            }
            data.masterVolume = 1f;
            data.musicVolume = 0.8f;
            data.sfxVolume = 1f;
            data.EnsureDefaults();
            return data;
        }
    }
}
