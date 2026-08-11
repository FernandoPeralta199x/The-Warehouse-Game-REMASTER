#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TW08.PowerUps;
using TW08.Presentation;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    internal static class TW08MegaContentSetup
    {
        internal const string GraphicsProfilePath = "Assets/_Project/ScriptableObjects/Presentation/TW08GraphicsProfile.asset";
        internal const string RacePowerUpRoot = "Assets/_Project/ScriptableObjects/Race/PowerUps";
        internal const string RacePowerUpTablePath = RacePowerUpRoot + "/RacePowerUpTable.asset";

        internal sealed class MegaContentData
        {
            public TW08GraphicsProfile GraphicsProfile { get; init; }
            public WeightedPowerUpTable RacePowerUpTable { get; init; }
        }

        internal static MegaContentData EnsureAll()
        {
            TW08ProductionSceneUtility.EnsureFolder("Assets/_Project/ScriptableObjects/Presentation");
            TW08ProductionSceneUtility.EnsureFolder("Assets/_Project/ScriptableObjects/Race");
            TW08ProductionSceneUtility.EnsureFolder(RacePowerUpRoot);

            TW08GraphicsProfile graphics = EnsureGraphicsProfile();
            WeightedPowerUpTable table = EnsureRacePowerUps();
            AssetDatabase.SaveAssets();

            graphics = AssetDatabase.LoadAssetAtPath<TW08GraphicsProfile>(GraphicsProfilePath);
            table = AssetDatabase.LoadAssetAtPath<WeightedPowerUpTable>(RacePowerUpTablePath);
            if (graphics == null || table == null)
            {
                throw new InvalidOperationException("TW08 mega content assets could not be reloaded after SaveAssets.");
            }

            return new MegaContentData
            {
                GraphicsProfile = graphics,
                RacePowerUpTable = table
            };
        }

        private static TW08GraphicsProfile EnsureGraphicsProfile()
        {
            TW08GraphicsProfile profile = LoadOrCreate<TW08GraphicsProfile>(GraphicsProfilePath);
            SerializedObject serialized = new(profile);
            SetInt(serialized, "targetFrameRate", 60);
            SetInt(serialized, "vSyncCount", 0);
            SetInt(serialized, "antiAliasing", 0);
            SetBool(serialized, "pixelSnap", true);
            SetFloat(serialized, "pixelsPerUnit", 32f);
            SetFloat(serialized, "cameraSmoothTime", 0.095f);
            SetFloat(serialized, "lookAheadTime", 0.15f);
            SetFloat(serialized, "maximumLookAhead", 1.8f);
            SetFloat(serialized, "baseOrthographicSize", 6.65f);
            SetFloat(serialized, "maximumSpeedZoomOut", 0.95f);
            SetFloat(serialized, "zoomSmoothTime", 0.17f);
            SetFloat(serialized, "defaultImpactShake", 0.11f);
            SetFloat(serialized, "shakeFrequency", 30f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static WeightedPowerUpTable EnsureRacePowerUps()
        {
            PowerUpDefinition nitro = EnsurePowerUp(
                "HydraulicNitro", "nitro-hidraulico", "NITRO HIDRÁULICO",
                PowerUpType.HydraulicNitro, 1.48f, 1.25f, 0f);
            PowerUpDefinition barrier = EnsurePowerUp(
                "SafetyBarrier", "barreira-seguranca", "BARREIRA DE SEGURANÇA",
                PowerUpType.SafetyBarrier, 1f, 0f, 0f);
            PowerUpDefinition stabilizer = EnsurePowerUp(
                "CargoStabilizer", "estabilizador-carga", "ESTABILIZADOR DE CARGA",
                PowerUpType.CargoStabilizer, 1.65f, 4.5f, 0f);
            PowerUpDefinition abs = EnsurePowerUp(
                "AbsBrake", "freio-abs-n8", "FREIO ABS N-8",
                PowerUpType.AbsBrake, 1.8f, 3.2f, 0f);
            PowerUpDefinition scanner = EnsurePowerUp(
                "RouteScanner", "scanner-rota", "SCANNER DE ROTA",
                PowerUpType.RouteScanner, 1f, 4.5f, 0f);
            PowerUpDefinition horn = EnsurePowerUp(
                "IndustrialHorn", "buzina-industrial", "BUZINA INDUSTRIAL",
                PowerUpType.IndustrialHorn, 0.62f, 1.5f, 4.2f);
            PowerUpDefinition repair = EnsurePowerUp(
                "RepairKit", "kit-reparo", "KIT DE REPARO",
                PowerUpType.RepairKit, 28f, 0f, 0f);
            PowerUpDefinition suspension = EnsurePowerUp(
                "ReinforcedSuspension", "suspensao-reforcada", "SUSPENSÃO REFORÇADA",
                PowerUpType.ReinforcedSuspension, 0.38f, 5f, 0f);

            WeightedPowerUpTable table = LoadOrCreate<WeightedPowerUpTable>(RacePowerUpTablePath);
            SerializedObject serialized = new(table);
            SerializedProperty entries = serialized.FindProperty("entries");
            if (entries == null)
            {
                throw new InvalidOperationException("WeightedPowerUpTable.entries could not be found.");
            }

            List<EntrySpec> specs = new()
            {
                new(nitro, 0.18f, 1f, 2.0f),
                new(barrier, 0.05f, 1f, 1.1f),
                new(stabilizer, 0f, 1f, 1.25f),
                new(abs, 0f, 0.78f, 1.1f),
                new(scanner, 0.12f, 1f, 0.8f),
                new(horn, 0.22f, 1f, 1.25f),
                new(repair, 0.30f, 1f, 0.85f),
                new(suspension, 0.12f, 1f, 0.9f)
            };

            entries.arraySize = specs.Count;
            for (int i = 0; i < specs.Count; i++)
            {
                SerializedProperty item = entries.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("definition").objectReferenceValue = specs[i].Definition;
                item.FindPropertyRelative("minimumRank").floatValue = specs[i].MinRank;
                item.FindPropertyRelative("maximumRank").floatValue = specs[i].MaxRank;
                item.FindPropertyRelative("weight").floatValue = specs[i].Weight;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(table);
            return table;
        }

        private static PowerUpDefinition EnsurePowerUp(
            string assetName,
            string id,
            string displayName,
            PowerUpType type,
            float magnitude,
            float duration,
            float radius)
        {
            string path = RacePowerUpRoot + "/" + assetName + ".asset";
            PowerUpDefinition definition = LoadOrCreate<PowerUpDefinition>(path);
            SerializedObject serialized = new(definition);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("type").enumValueIndex = (int)type;
            serialized.FindProperty("effect").objectReferenceValue = null;
            serialized.FindProperty("magnitude").floatValue = Mathf.Max(0f, magnitude);
            serialized.FindProperty("duration").floatValue = Mathf.Max(0f, duration);
            serialized.FindProperty("radius").floatValue = Mathf.Max(0f, radius);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetFloat(SerializedObject serialized, string name, float value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null) property.floatValue = value;
        }

        private static void SetInt(SerializedObject serialized, string name, int value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null) property.intValue = value;
        }

        private static void SetBool(SerializedObject serialized, string name, bool value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null) property.boolValue = value;
        }

        private readonly struct EntrySpec
        {
            public EntrySpec(PowerUpDefinition definition, float minRank, float maxRank, float weight)
            {
                Definition = definition;
                MinRank = minRank;
                MaxRank = maxRank;
                Weight = weight;
            }

            public PowerUpDefinition Definition { get; }
            public float MinRank { get; }
            public float MaxRank { get; }
            public float Weight { get; }
        }
    }
}
#endif
