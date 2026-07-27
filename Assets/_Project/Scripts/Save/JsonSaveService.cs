using System;
using System.IO;
using TW08.Data;
using UnityEngine;

namespace TW08.Save
{
    public sealed class JsonSaveService
    {
        private readonly string savePath;
        private readonly string backupPath;
        private readonly int currentVersion;
        private readonly SaveMigrationPipeline migrations;

        public JsonSaveService(GameConfig config, SaveMigrationPipeline migrations = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            currentVersion = config.SaveVersion;
            savePath = Path.Combine(Application.persistentDataPath, config.SaveFileName);
            backupPath = savePath + ".backup";
            this.migrations = migrations ?? new SaveMigrationPipeline(Array.Empty<ISaveMigration>());
        }

        public SaveGameData Load()
        {
            SaveGameData data = TryLoad(savePath) ?? TryLoad(backupPath) ?? new SaveGameData();
            if (data.version < currentVersion)
            {
                data = migrations.MigrateTo(data, currentVersion);
            }

            data.version = currentVersion;
            return data;
        }

        public void Save(SaveGameData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.version = currentVersion;
            string payload = JsonUtility.ToJson(data, true);
            SaveEnvelope envelope = new()
            {
                payload = payload,
                checksum = SaveIntegrity.ComputeChecksum(payload)
            };

            string json = JsonUtility.ToJson(envelope, true);
            string temporaryPath = savePath + ".tmp";
            Directory.CreateDirectory(Path.GetDirectoryName(savePath) ?? Application.persistentDataPath);
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(savePath))
            {
                File.Copy(savePath, backupPath, true);
                File.Delete(savePath);
            }

            File.Move(temporaryPath, savePath);
        }

        private static SaveGameData TryLoad(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                string json = File.ReadAllText(path);
                SaveEnvelope envelope = JsonUtility.FromJson<SaveEnvelope>(json);
                if (envelope == null || !SaveIntegrity.IsValid(envelope.payload, envelope.checksum))
                {
                    Debug.LogWarning($"Save integrity validation failed for '{path}'.");
                    return null;
                }

                return JsonUtility.FromJson<SaveGameData>(envelope.payload);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Unable to load save '{path}': {exception.Message}");
                return null;
            }
        }
    }
}
