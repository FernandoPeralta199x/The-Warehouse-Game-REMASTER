#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TW08.Puzzle;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Importa fases de puzzle a partir de Docs/level-layouts-enriched.json
    /// (produzido pela pipeline Tools/puzzle: solver prova solvabilidade e
    /// deriva medalhas do custo ótimo real).
    ///
    /// Fases "main" entram em TW08_PuzzleCampaign (ordenadas por specIndex);
    /// fases "secret" entram em TW08_SecretCampaign.
    /// </summary>
    public static class TW08CampaignExpansionImporter
    {
        private const string EnrichedJsonPath = "Docs/level-layouts-enriched.json";
        public const string SecretCampaignPath =
            TW08ExpansionDataSetup.CampaignRoot + "/TW08_SecretCampaign.asset";

        // ------------------------------------------------------------ DTOs --

        [Serializable] private sealed class CoordDto { public int x; public int y; }

        [Serializable]
        private sealed class CrateDto { public int x; public int y; public string kind; }

        [Serializable]
        private sealed class GoalReqDto { public int x; public int y; public string kind; }

        [Serializable]
        private sealed class ConveyorDto { public int x; public int y; public string dir; }

        [Serializable]
        private sealed class PatrolDto { public string id; public List<CoordDto> route; }

        [Serializable]
        private sealed class TimedBlockDto { public int x; public int y; public int opensAfter; }

        [Serializable]
        private sealed class SwitchGroupDto
        {
            public string id;
            public List<CoordDto> sensors;
            public List<CoordDto> doors;
        }

        [Serializable]
        private sealed class LevelDto
        {
            public string id;
            public string displayName;
            public string sectorId;
            public string briefing;
            public List<string> gimmickTags;
            public string kind;
            public int specIndex;
            public int width;
            public int height;
            public CoordDto player;
            public List<CoordDto> walls;
            public List<CoordDto> goals;
            public List<CoordDto> costly;
            public List<CoordDto> ice;
            public List<ConveyorDto> conveyors;
            public List<CoordDto> fakeWalls;
            public List<PatrolDto> patrols;
            public List<CoordDto> directionButtons;
            public List<TimedBlockDto> timedBlocks;
            public string fogMode;
            public int fogRadius;
            public List<CrateDto> crates;
            public List<GoalReqDto> goalRequirements;
            public List<SwitchGroupDto> switchGroups;
            public int optimalCost;
            public int optimalPushes;
            public int goldMoveLimit;
            public int platinumMoveLimit;
            public bool allowPowerUps;
        }

        [Serializable] private sealed class RootDto { public List<LevelDto> levels; }

        // ----------------------------------------------------------- Entry --

        [MenuItem("Tools/TW08/Production/Import Campaign Levels From JSON")]
        public static void ImportMenu()
        {
            int count = ImportAll();
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08",
                $"Importação concluída: {count} fases criadas/atualizadas.",
                "OK");
        }

        /// <summary>Entrada para -executeMethod em batchmode.</summary>
        public static void ImportFromCommandLine()
        {
            try
            {
                int count = ImportAll();
                Debug.Log($"TW08CampaignExpansionImporter: {count} fases importadas com sucesso.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"TW08CampaignExpansionImporter FALHOU: {ex}");
                EditorApplication.Exit(1);
            }
        }

        public static int ImportAll()
        {
            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), EnrichedJsonPath);
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Arquivo não encontrado: {EnrichedJsonPath}", jsonPath);
            }

            RootDto root = JsonUtility.FromJson<RootDto>(File.ReadAllText(jsonPath));
            if (root?.levels == null || root.levels.Count == 0)
            {
                throw new InvalidOperationException("JSON enriquecido vazio ou inválido.");
            }

            var createdAssets = new Dictionary<string, PuzzleLevelDefinition>(StringComparer.Ordinal);
            foreach (LevelDto dto in root.levels)
            {
                createdAssets[dto.id] = CreateOrUpdateLevelAsset(dto);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RegisterMainCampaign(root.levels, createdAssets);
            RegisterSecretCampaign(root.levels, createdAssets);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return root.levels.Count;
        }

        // ---------------------------------------------------------- Assets --

        private static PuzzleLevelDefinition CreateOrUpdateLevelAsset(LevelDto dto)
        {
            string assetPath = $"{TW08ExpansionDataSetup.CampaignRoot}/{dto.id}.asset";
            var level = AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>(assetPath);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<PuzzleLevelDefinition>();
                AssetDatabase.CreateAsset(level, assetPath);
            }

            var so = new SerializedObject(level);
            so.FindProperty("levelId").stringValue = dto.id;
            so.FindProperty("displayName").stringValue = dto.displayName;
            so.FindProperty("sectorId").stringValue = dto.sectorId;
            so.FindProperty("briefing").stringValue = dto.briefing ?? string.Empty;
            so.FindProperty("width").intValue = dto.width;
            so.FindProperty("height").intValue = dto.height;
            so.FindProperty("cellSize").floatValue = 1f;
            so.FindProperty("goldMoveLimit").intValue = dto.goldMoveLimit;
            so.FindProperty("platinumMoveLimit").intValue = dto.platinumMoveLimit;
            so.FindProperty("allowPowerUps").boolValue = dto.allowPowerUps;

            WriteCoord(so.FindProperty("playerStart"), dto.player.x, dto.player.y);
            WriteStringList(so.FindProperty("gimmickTags"), dto.gimmickTags);
            WriteCoordList(so.FindProperty("walls"), dto.walls);
            WriteCoordList(so.FindProperty("goals"), dto.goals);
            WriteCoordList(so.FindProperty("costlyCells"), dto.costly);
            WriteCoordList(so.FindProperty("iceCells"), dto.ice);
            WriteCoordList(so.FindProperty("fakeWalls"), dto.fakeWalls);
            so.FindProperty("fogMode").enumValueIndex = (int)ParseFog(dto.fogMode);
            so.FindProperty("fogRadius").intValue = Mathf.Max(1, dto.fogRadius);

            WriteCoordList(so.FindProperty("directionButtons"), dto.directionButtons);

            SerializedProperty timedList = so.FindProperty("timedBlocks");
            timedList.arraySize = dto.timedBlocks?.Count ?? 0;
            for (int i = 0; i < timedList.arraySize; i++)
            {
                TimedBlockDto block = dto.timedBlocks[i];
                SerializedProperty element = timedList.GetArrayElementAtIndex(i);
                WriteCoord(element.FindPropertyRelative("position"), block.x, block.y);
                element.FindPropertyRelative("opensAfterCommands").intValue = Mathf.Max(1, block.opensAfter);
            }

            SerializedProperty patrolList = so.FindProperty("patrols");
            patrolList.arraySize = dto.patrols?.Count ?? 0;
            for (int i = 0; i < patrolList.arraySize; i++)
            {
                PatrolDto patrol = dto.patrols[i];
                SerializedProperty element = patrolList.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("patrolId").stringValue =
                    string.IsNullOrWhiteSpace(patrol.id) ? $"patrol-{i + 1:00}" : patrol.id;
                WriteCoordList(element.FindPropertyRelative("route"), patrol.route);
            }

            SerializedProperty conveyorList = so.FindProperty("conveyors");
            conveyorList.arraySize = dto.conveyors?.Count ?? 0;
            for (int i = 0; i < conveyorList.arraySize; i++)
            {
                ConveyorDto conveyor = dto.conveyors[i];
                SerializedProperty element = conveyorList.GetArrayElementAtIndex(i);
                WriteCoord(element.FindPropertyRelative("position"), conveyor.x, conveyor.y);
                element.FindPropertyRelative("direction").enumValueIndex = (int)ParseDirection(conveyor.dir);
            }

            SerializedProperty crates = so.FindProperty("crates");
            crates.arraySize = dto.crates?.Count ?? 0;
            for (int i = 0; i < crates.arraySize; i++)
            {
                CrateDto c = dto.crates[i];
                SerializedProperty el = crates.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("id").stringValue = $"crate-{i + 1:00}";
                el.FindPropertyRelative("kind").enumValueIndex = (int)ParseKind(c.kind);
                WriteCoord(el.FindPropertyRelative("position"), c.x, c.y);
            }

            SerializedProperty reqs = so.FindProperty("goalRequirements");
            reqs.arraySize = dto.goalRequirements?.Count ?? 0;
            for (int i = 0; i < reqs.arraySize; i++)
            {
                GoalReqDto r = dto.goalRequirements[i];
                SerializedProperty el = reqs.GetArrayElementAtIndex(i);
                WriteCoord(el.FindPropertyRelative("position"), r.x, r.y);
                el.FindPropertyRelative("requiredKind").enumValueIndex = (int)ParseKind(r.kind);
            }

            SerializedProperty groups = so.FindProperty("switchGroups");
            groups.arraySize = dto.switchGroups?.Count ?? 0;
            for (int i = 0; i < groups.arraySize; i++)
            {
                SwitchGroupDto g = dto.switchGroups[i];
                SerializedProperty el = groups.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("id").stringValue = g.id;
                WriteCoordList(el.FindPropertyRelative("sensors"), g.sensors);
                WriteCoordList(el.FindPropertyRelative("doors"), g.doors);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            return level;
        }

        private static PuzzleFogMode ParseFog(string fog)
        {
            return Enum.TryParse(fog, out PuzzleFogMode parsed) ? parsed : PuzzleFogMode.None;
        }

        private static ConveyorDirection ParseDirection(string direction)
        {
            return Enum.TryParse(direction, out ConveyorDirection parsed)
                ? parsed
                : ConveyorDirection.Right;
        }

        private static PuzzleEntityKind ParseKind(string kind)
        {
            return Enum.TryParse(kind, out PuzzleEntityKind parsed) ? parsed : PuzzleEntityKind.Crate;
        }

        private static void WriteCoord(SerializedProperty coord, int x, int y)
        {
            coord.FindPropertyRelative("x").intValue = x;
            coord.FindPropertyRelative("y").intValue = y;
        }

        private static void WriteCoordList(SerializedProperty list, List<CoordDto> coords)
        {
            list.arraySize = coords?.Count ?? 0;
            for (int i = 0; i < list.arraySize; i++)
            {
                WriteCoord(list.GetArrayElementAtIndex(i), coords[i].x, coords[i].y);
            }
        }

        private static void WriteStringList(SerializedProperty list, List<string> values)
        {
            list.arraySize = values?.Count ?? 0;
            for (int i = 0; i < list.arraySize; i++)
            {
                list.GetArrayElementAtIndex(i).stringValue = values[i];
            }
        }

        // -------------------------------------------------------- Campaign --

        private static void RegisterMainCampaign(
            List<LevelDto> all, Dictionary<string, PuzzleLevelDefinition> assets)
        {
            var campaign = AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(
                TW08ExpansionDataSetup.PuzzleCampaignPath);
            if (campaign == null)
            {
                throw new InvalidOperationException(
                    $"Campanha principal não encontrada em {TW08ExpansionDataSetup.PuzzleCampaignPath}.");
            }

            List<LevelDto> mains = all
                .Where(l => string.Equals(l.kind, "main", StringComparison.OrdinalIgnoreCase))
                .OrderBy(l => l.specIndex)
                .ToList();

            var so = new SerializedObject(campaign);
            SerializedProperty levels = so.FindProperty("levels");

            var existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < levels.arraySize; i++)
            {
                var lvl = levels.GetArrayElementAtIndex(i).FindPropertyRelative("level")
                    .objectReferenceValue as PuzzleLevelDefinition;
                if (lvl != null)
                {
                    existingIds.Add(lvl.LevelId);
                }
            }

            foreach (LevelDto dto in mains)
            {
                if (existingIds.Contains(dto.id))
                {
                    continue;
                }

                int idx = levels.arraySize;
                levels.arraySize = idx + 1;
                SerializedProperty el = levels.GetArrayElementAtIndex(idx);
                el.FindPropertyRelative("level").objectReferenceValue = assets[dto.id];
                el.FindPropertyRelative("sceneName").stringValue = dto.id;
                el.FindPropertyRelative("unlockedByDefault").boolValue = false;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(campaign);
        }

        private static void RegisterSecretCampaign(
            List<LevelDto> all, Dictionary<string, PuzzleLevelDefinition> assets)
        {
            List<LevelDto> secrets = all
                .Where(l => string.Equals(l.kind, "secret", StringComparison.OrdinalIgnoreCase))
                .OrderBy(l => l.specIndex)
                .ToList();
            if (secrets.Count == 0)
            {
                return;
            }

            var campaign = AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(SecretCampaignPath);
            if (campaign == null)
            {
                campaign = ScriptableObject.CreateInstance<PuzzleCampaignDefinition>();
                AssetDatabase.CreateAsset(campaign, SecretCampaignPath);
            }

            var so = new SerializedObject(campaign);
            SerializedProperty levels = so.FindProperty("levels");
            levels.arraySize = secrets.Count;
            for (int i = 0; i < secrets.Count; i++)
            {
                LevelDto dto = secrets[i];
                SerializedProperty el = levels.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("level").objectReferenceValue = assets[dto.id];
                el.FindPropertyRelative("sceneName").stringValue = dto.id;
                el.FindPropertyRelative("unlockedByDefault").boolValue = i == 0;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(campaign);
        }
    }
}
#endif
