#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Data;
using TW08.Presentation;
using TW08.Puzzle;
using TW08.Race;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    public static class TW08ExpansionDataSetup
    {
        public const string CharacterRoot = "Assets/_Project/ScriptableObjects/Characters";
        public const string CampaignRoot = "Assets/_Project/ScriptableObjects/Campaign";
        public const string RaceRoot = "Assets/_Project/ScriptableObjects/Race";
        public const string RosterPath = CharacterRoot + "/TW08_CharacterRoster.asset";
        public const string PuzzleCampaignPath = CampaignRoot + "/TW08_PuzzleCampaign.asset";
        public const string RaceCampaignPath = RaceRoot + "/TW08_RaceCampaign.asset";
        public const string ForkliftStatsPath = RaceRoot + "/N8_Standard_ForkliftStats.asset";

        private const string ExistingLevelRoot = "Assets/_Project/ScriptableObjects/VerticalSlice";

        public sealed class ExpansionData
        {
            public CharacterRoster Roster;
            public PuzzleCampaignDefinition PuzzleCampaign;
            public RaceCampaignDefinition RaceCampaign;
            public ForkliftStats ForkliftStats;
            public List<PuzzleLevelDefinition> PuzzleLevels = new();
            public List<RaceTrackDefinition> RaceTracks = new();
        }

        [MenuItem("Tools/TW08/Production/Build Full Expansion Data")]
        public static void BuildFromMenu()
        {
            ExpansionData data = EnsureAll();
            Selection.activeObject = data.Roster;
            EditorGUIUtility.PingObject(data.Roster);
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08 — Expansion Data",
                "Dados de produção atualizados.\n\n" +
                "Operadores: John, Duda, Robert\n" +
                "Campanha puzzle: " + data.PuzzleLevels.Count + " fases\n" +
                "Corridas: " + data.RaceTracks.Count + " pistas\n" +
                "Save schema alvo: v2",
                "OK");
        }

        public static ExpansionData EnsureAll()
        {
            EnsureFolder(CharacterRoot);
            EnsureFolder(CampaignRoot);
            EnsureFolder(RaceRoot);
            TW08ProductionArtSetup.EnsureProductionArtAssets();
            TW08ExpansionStarterArt.EnsureAll();

            DirectionalSpriteSet johnSprites = AssetDatabase.LoadAssetAtPath<DirectionalSpriteSet>(TW08ProductionArtSetup.JohnSpriteSetPath);
            DirectionalSpriteSet dudaSprites = TW08ExpansionStarterArt.EnsureDudaSpriteSet();

            CharacterProfile john = EnsureCharacter(
                CharacterRoot + "/John_Miller.asset",
                "john",
                "John Miller",
                "Operador Manual",
                "Operador veterano do N-8. Especialista em rotas, carga e recuperação manual de armazém.",
                TW08ExpansionStarterArt.LoadPortrait("john"),
                johnSprites,
                new Color(0.94f, 0.61f, 0.12f, 1f),
                true,
                true,
                true);

            CharacterProfile duda = EnsureCharacter(
                CharacterRoot + "/Maria_Eduarda_Duda.asset",
                "duda",
                "Maria Eduarda — Duda",
                "Sistemas & Segurança",
                "Analista de sistemas logísticos do N-8. Lê rotas, sensores e terminais com precisão operacional.",
                TW08ExpansionStarterArt.LoadPortrait("duda"),
                dudaSprites,
                new Color(0.18f, 0.78f, 0.82f, 1f),
                true,
                true,
                true);

            CharacterProfile robert = EnsureCharacter(
                CharacterRoot + "/Robert_Big_Rob_Hayes.asset",
                "robert",
                "Robert — Big Rob — Hayes",
                "Mecânico da Oficina N-8",
                "Mecânico e mentor da garagem. Dá suporte às empilhadeiras e briefings de manutenção.",
                TW08ExpansionStarterArt.LoadPortrait("robert"),
                null,
                new Color(0.78f, 0.43f, 0.12f, 1f),
                false,
                false,
                false);

            CharacterRoster roster = EnsureRoster(john, duda, robert);
            List<PuzzleLevelDefinition> puzzleLevels = EnsurePuzzleLevels();
            PuzzleCampaignDefinition puzzleCampaign = EnsurePuzzleCampaign(puzzleLevels);
            ForkliftStats forkliftStats = EnsureForkliftStats();
            List<RaceTrackDefinition> raceTracks = EnsureRaceTracks();
            RaceCampaignDefinition raceCampaign = EnsureRaceCampaign(raceTracks);
            UpgradeGameConfigs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new ExpansionData
            {
                Roster = roster,
                PuzzleCampaign = puzzleCampaign,
                RaceCampaign = raceCampaign,
                ForkliftStats = forkliftStats,
                PuzzleLevels = puzzleLevels,
                RaceTracks = raceTracks
            };
        }

        private static CharacterProfile EnsureCharacter(
            string path,
            string id,
            string displayName,
            string role,
            string description,
            Sprite portrait,
            DirectionalSpriteSet sprites,
            Color accent,
            bool puzzle,
            bool race,
            bool unlocked)
        {
            CharacterProfile profile = LoadOrCreate<CharacterProfile>(path);
            SerializedObject serialized = new(profile);
            serialized.FindProperty("characterId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("role").stringValue = role;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("portrait").objectReferenceValue = portrait;
            serialized.FindProperty("puzzleSprites").objectReferenceValue = sprites;
            serialized.FindProperty("uiAccent").colorValue = accent;
            serialized.FindProperty("puzzleEnabled").boolValue = puzzle;
            serialized.FindProperty("raceEnabled").boolValue = race;
            serialized.FindProperty("unlockedByDefault").boolValue = unlocked;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CharacterRoster EnsureRoster(params CharacterProfile[] profiles)
        {
            CharacterRoster roster = LoadOrCreate<CharacterRoster>(RosterPath);
            SerializedObject serialized = new(roster);
            serialized.FindProperty("defaultCharacterId").stringValue = "john";
            SerializedProperty list = serialized.FindProperty("characters");
            list.arraySize = profiles.Length;
            for (int i = 0; i < profiles.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = profiles[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(roster);
            return roster;
        }

        private static List<PuzzleLevelDefinition> EnsurePuzzleLevels()
        {
            List<PuzzleLevelDefinition> result = new();
            string[] existingPaths =
            {
                ExistingLevelRoot + "/TW08_Level01_FirstShift.asset",
                ExistingLevelRoot + "/TW08_Level02_TightCorridor.asset",
                ExistingLevelRoot + "/TW08_Level03_CrossLoad.asset"
            };

            foreach (string path in existingPaths)
            {
                PuzzleLevelDefinition existing = AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>(path);
                if (existing != null) result.Add(existing);
            }

            result.Add(ConfigureLevel(new LevelSpec(
                "TW08_Level04_SensorSplit", "Sensor Split", "S02",
                "Use a primeira carga como trava de segurança. Enquanto o sensor estiver ocupado, a porta da doca permanece aberta.",
                8, 7, new Vector2Int(1, 2),
                Array.Empty<Vector2Int>(),
                new[] { new Vector2Int(3, 2), new Vector2Int(6, 4) },
                new[] { new CrateSpec("crate-sensor", 2, 2), new CrateSpec("crate-route", 2, 4) },
                12, 8,
                new[] { "sensor", "door", "priority" },
                Array.Empty<Vector2Int>(),
                new[] { new SwitchSpec("dock-a", new[] { new Vector2Int(3, 2) }, new[] { new Vector2Int(4, 4) }) },
                Array.Empty<GoalRequirementSpec>())));

            result.Add(ConfigureLevel(new LevelSpec(
                "TW08_Level05_TightLift", "Tight Lift", "S02",
                "Três corredores estreitos. Planeje a ordem para não bloquear sua própria rota de retorno.",
                9, 7, new Vector2Int(1, 1),
                new[] { new Vector2Int(3, 3), new Vector2Int(5, 3), new Vector2Int(3, 4), new Vector2Int(5, 4) },
                new[] { new Vector2Int(2, 5), new Vector2Int(4, 5), new Vector2Int(6, 5) },
                new[] { new CrateSpec("crate-a", 2, 2), new CrateSpec("crate-b", 4, 2), new CrateSpec("crate-c", 6, 2) },
                26, 20,
                new[] { "tight", "deadlock", "routing" },
                Array.Empty<Vector2Int>(), Array.Empty<SwitchSpec>(), Array.Empty<GoalRequirementSpec>())));

            result.Add(ConfigureLevel(new LevelSpec(
                "TW08_Level06_TerminalRoute", "Terminal Route", "S02",
                "Sincronize os dois sensores para liberar a porta central e despachar a carga final.",
                9, 7, new Vector2Int(1, 1),
                Array.Empty<Vector2Int>(),
                new[] { new Vector2Int(3, 2), new Vector2Int(3, 4), new Vector2Int(7, 3) },
                new[] { new CrateSpec("crate-a", 2, 2), new CrateSpec("crate-b", 2, 4), new CrateSpec("crate-route", 2, 3) },
                18, 13,
                new[] { "terminal", "multi-sensor", "door" },
                Array.Empty<Vector2Int>(),
                new[] { new SwitchSpec("terminal-a", new[] { new Vector2Int(3, 2), new Vector2Int(3, 4) }, new[] { new Vector2Int(5, 3) }) },
                Array.Empty<GoalRequirementSpec>())));

            result.Add(ConfigureLevel(new LevelSpec(
                "TW08_Level07_DockSync", "Dock Sync", "S03",
                "Cada doca aceita um tipo específico de carga. Heavy, fragile e padrão precisam terminar no destino correto.",
                9, 8, new Vector2Int(1, 1),
                new[] { new Vector2Int(3, 3), new Vector2Int(5, 3) },
                new[] { new Vector2Int(2, 5), new Vector2Int(4, 5), new Vector2Int(6, 5) },
                new[]
                {
                    new CrateSpec("crate-standard", 2, 2, PuzzleEntityKind.Crate),
                    new CrateSpec("crate-heavy", 4, 2, PuzzleEntityKind.HeavyCrate),
                    new CrateSpec("crate-fragile", 6, 2, PuzzleEntityKind.FragileCrate)
                },
                26, 20,
                new[] { "typed-dock", "cargo", "order" },
                Array.Empty<Vector2Int>(), Array.Empty<SwitchSpec>(),
                new[]
                {
                    new GoalRequirementSpec(2, 5, PuzzleEntityKind.Crate),
                    new GoalRequirementSpec(4, 5, PuzzleEntityKind.HeavyCrate),
                    new GoalRequirementSpec(6, 5, PuzzleEntityKind.FragileCrate)
                })));

            List<Vector2Int> coldCells = new();
            for (int y = 2; y <= 4; y++)
            for (int x = 3; x <= 6; x++)
                coldCells.Add(new Vector2Int(x, y));

            result.Add(ConfigureLevel(new LevelSpec(
                "TW08_Level08_ColdStorage", "Cold Storage", "S03",
                "Piso de câmara fria custa duas unidades de movimento. A rota curta nem sempre é a rota eficiente.",
                9, 7, new Vector2Int(1, 1),
                Array.Empty<Vector2Int>(),
                new[] { new Vector2Int(7, 2), new Vector2Int(7, 4) },
                new[] { new CrateSpec("crate-a", 2, 2), new CrateSpec("crate-b", 5, 3) },
                28, 22,
                new[] { "cold", "movement-cost", "efficiency" },
                coldCells, Array.Empty<SwitchSpec>(), Array.Empty<GoalRequirementSpec>())));

            List<Vector2Int> crossCost = new();
            for (int y = 3; y <= 4; y++)
            for (int x = 4; x <= 6; x++)
                crossCost.Add(new Vector2Int(x, y));

            result.Add(ConfigureLevel(new LevelSpec(
                "TW08_Level09_CrossDispatch", "Cross Dispatch", "S03",
                "Combine sensor, porta, piso de custo e docas tipadas. Este é o primeiro exame completo do turno.",
                10, 7, new Vector2Int(1, 1),
                Array.Empty<Vector2Int>(),
                new[] { new Vector2Int(3, 2), new Vector2Int(7, 3), new Vector2Int(7, 4) },
                new[]
                {
                    new CrateSpec("crate-sensor", 2, 2, PuzzleEntityKind.Crate),
                    new CrateSpec("crate-heavy", 2, 3, PuzzleEntityKind.HeavyCrate),
                    new CrateSpec("crate-fragile", 2, 4, PuzzleEntityKind.FragileCrate)
                },
                36, 28,
                new[] { "sensor", "door", "cold", "typed-dock", "exam" },
                crossCost,
                new[] { new SwitchSpec("dispatch-a", new[] { new Vector2Int(3, 2) }, new[] { new Vector2Int(5, 3), new Vector2Int(5, 4) }) },
                new[]
                {
                    new GoalRequirementSpec(7, 3, PuzzleEntityKind.HeavyCrate),
                    new GoalRequirementSpec(7, 4, PuzzleEntityKind.FragileCrate)
                })));

            return result;
        }

        private static PuzzleLevelDefinition ConfigureLevel(LevelSpec spec)
        {
            string path = CampaignRoot + "/" + spec.Id + ".asset";
            PuzzleLevelDefinition level = LoadOrCreate<PuzzleLevelDefinition>(path);
            SerializedObject s = new(level);
            s.FindProperty("levelId").stringValue = spec.Id;
            s.FindProperty("displayName").stringValue = spec.Name;
            s.FindProperty("sectorId").stringValue = spec.Sector;
            s.FindProperty("briefing").stringValue = spec.Briefing;
            s.FindProperty("width").intValue = spec.Width;
            s.FindProperty("height").intValue = spec.Height;
            s.FindProperty("cellSize").floatValue = 1f;
            s.FindProperty("allowPowerUps").boolValue = false;
            s.FindProperty("goldMoveLimit").intValue = spec.Gold;
            s.FindProperty("platinumMoveLimit").intValue = spec.Platinum;
            SetCoordinate(s.FindProperty("playerStart"), spec.Player.x, spec.Player.y);

            HashSet<Vector2Int> walls = new(BuildBorder(spec.Width, spec.Height));
            foreach (Vector2Int wall in spec.InternalWalls) walls.Add(wall);
            SetCoordinates(s.FindProperty("walls"), walls);
            SetCoordinates(s.FindProperty("goals"), spec.Goals);
            SetCoordinates(s.FindProperty("costlyCells"), spec.CostlyCells);
            SetStrings(s.FindProperty("gimmickTags"), spec.Tags);

            SerializedProperty crateList = s.FindProperty("crates");
            crateList.arraySize = spec.Crates.Count;
            for (int i = 0; i < spec.Crates.Count; i++)
            {
                CrateSpec crate = spec.Crates[i];
                SerializedProperty item = crateList.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue = crate.Id;
                item.FindPropertyRelative("kind").enumValueIndex = (int)crate.Kind;
                SetCoordinate(item.FindPropertyRelative("position"), crate.X, crate.Y);
            }

            SerializedProperty requirementList = s.FindProperty("goalRequirements");
            requirementList.arraySize = spec.GoalRequirements.Count;
            for (int i = 0; i < spec.GoalRequirements.Count; i++)
            {
                GoalRequirementSpec requirement = spec.GoalRequirements[i];
                SerializedProperty item = requirementList.GetArrayElementAtIndex(i);
                SetCoordinate(item.FindPropertyRelative("position"), requirement.X, requirement.Y);
                item.FindPropertyRelative("requiredKind").enumValueIndex = (int)requirement.Kind;
            }

            SerializedProperty switchList = s.FindProperty("switchGroups");
            switchList.arraySize = spec.Switches.Count;
            for (int i = 0; i < spec.Switches.Count; i++)
            {
                SwitchSpec group = spec.Switches[i];
                SerializedProperty item = switchList.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue = group.Id;
                SetCoordinates(item.FindPropertyRelative("sensors"), group.Sensors);
                SetCoordinates(item.FindPropertyRelative("doors"), group.Doors);
            }

            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            return level;
        }

        private static PuzzleCampaignDefinition EnsurePuzzleCampaign(IReadOnlyList<PuzzleLevelDefinition> levels)
        {
            PuzzleCampaignDefinition campaign = LoadOrCreate<PuzzleCampaignDefinition>(PuzzleCampaignPath);
            SerializedObject s = new(campaign);
            SerializedProperty list = s.FindProperty("levels");
            list.arraySize = levels.Count;
            for (int i = 0; i < levels.Count; i++)
            {
                PuzzleLevelDefinition level = levels[i];
                SerializedProperty item = list.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("level").objectReferenceValue = level;
                item.FindPropertyRelative("sceneName").stringValue = SceneNameForLevel(level, i + 1);
                item.FindPropertyRelative("unlockedByDefault").boolValue = i == 0;
            }
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(campaign);
            return campaign;
        }

        private static ForkliftStats EnsureForkliftStats()
        {
            ForkliftStats stats = LoadOrCreate<ForkliftStats>(ForkliftStatsPath);
            SerializedObject s = new(stats);
            s.FindProperty("maxForwardSpeed").floatValue = 11.5f;
            s.FindProperty("maxReverseSpeed").floatValue = 4.5f;
            s.FindProperty("acceleration").floatValue = 17f;
            s.FindProperty("brakeForce").floatValue = 23f;
            s.FindProperty("rollingResistance").floatValue = 0.045f;
            s.FindProperty("steeringDegreesPerSecond").floatValue = 155f;
            s.FindProperty("lowSpeedSteering").floatValue = 0.42f;
            s.FindProperty("normalLateralRetention").floatValue = 0.2f;
            s.FindProperty("driftLateralRetention").floatValue = 0.66f;
            s.FindProperty("minimumDriftCharge").floatValue = 0.5f;
            s.FindProperty("maximumDriftCharge").floatValue = 2.1f;
            s.FindProperty("driftBoostMultiplier").floatValue = 1.32f;
            s.FindProperty("driftBoostDuration").floatValue = 0.75f;
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(stats);
            return stats;
        }

        private static List<RaceTrackDefinition> EnsureRaceTracks()
        {
            return new List<RaceTrackDefinition>
            {
                EnsureRaceTrack("receiving-loop", "Receiving Loop", "TW08_Race01_ReceivingLoop",
                    "Tutorial de pilotagem na doca de recebimento. Linhas amplas, boost controlado e leitura de checkpoint.",
                    75f, 65f, 58f, 52f, 1f),
                EnsureRaceTrack("industrial-corridor", "Industrial Corridor", "TW08_Race02_IndustrialCorridor",
                    "Corredores técnicos, chicanes e barreiras exigem freio e direção precisa.",
                    95f, 82f, 72f, 65f, 0.9f),
                EnsureRaceTrack("frozen-route", "Frozen Route", "TW08_Race03_FrozenRoute",
                    "Câmara fria com zonas de baixa aderência. Antecipe a curva e use boost apenas na saída.",
                    110f, 95f, 84f, 76f, 0.45f)
            };
        }

        private static RaceTrackDefinition EnsureRaceTrack(
            string id,
            string name,
            string scene,
            string briefing,
            float bronze,
            float silver,
            float gold,
            float platinum,
            float grip)
        {
            string rulesPath = RaceRoot + "/RaceRules_" + id + ".asset";
            RaceDefinition rules = LoadOrCreate<RaceDefinition>(rulesPath);
            SerializedObject r = new(rules);
            r.FindProperty("raceId").stringValue = id;
            r.FindProperty("laps").intValue = 3;
            r.FindProperty("countdownSeconds").floatValue = 3f;
            r.FindProperty("bronzeTime").floatValue = bronze;
            r.FindProperty("silverTime").floatValue = silver;
            r.FindProperty("goldTime").floatValue = gold;
            r.FindProperty("platinumTime").floatValue = platinum;
            r.FindProperty("maximumCargoDamageForGold").floatValue = 5f;
            r.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(rules);

            string path = RaceRoot + "/Track_" + id + ".asset";
            RaceTrackDefinition track = LoadOrCreate<RaceTrackDefinition>(path);
            SerializedObject t = new(track);
            t.FindProperty("trackId").stringValue = id;
            t.FindProperty("displayName").stringValue = name;
            t.FindProperty("sceneName").stringValue = scene;
            t.FindProperty("raceRules").objectReferenceValue = rules;
            t.FindProperty("briefing").stringValue = briefing;
            t.FindProperty("surfaceGrip").floatValue = grip;
            t.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(track);
            return track;
        }

        private static RaceCampaignDefinition EnsureRaceCampaign(IReadOnlyList<RaceTrackDefinition> tracks)
        {
            RaceCampaignDefinition campaign = LoadOrCreate<RaceCampaignDefinition>(RaceCampaignPath);
            SerializedObject s = new(campaign);
            SerializedProperty list = s.FindProperty("tracks");
            list.arraySize = tracks.Count;
            for (int i = 0; i < tracks.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = tracks[i];
            }
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(campaign);
            return campaign;
        }

        private static void UpgradeGameConfigs()
        {
            string[] guids = AssetDatabase.FindAssets("t:GameConfig");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(path);
                if (config == null) continue;
                SerializedObject s = new(config);
                s.FindProperty("saveVersion").intValue = 2;
                s.FindProperty("mainMenuScene").stringValue = "TW08_MainMenu";
                s.FindProperty("firstPuzzleScene").stringValue = "TW08_Level01_FirstShift";
                s.FindProperty("firstRaceScene").stringValue = "TW08_Race01_ReceivingLoop";
                s.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(config);
            }
        }

        private static string SceneNameForLevel(PuzzleLevelDefinition level, int index)
        {
            if (index == 1) return "TW08_Level01_FirstShift";
            if (index == 2) return "TW08_Level02_TightCorridor";
            if (index == 3) return "TW08_Level03_CrossLoad";
            return level.LevelId;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static IEnumerable<Vector2Int> BuildBorder(int width, int height)
        {
            HashSet<Vector2Int> cells = new();
            for (int x = 0; x < width; x++)
            {
                cells.Add(new Vector2Int(x, 0));
                cells.Add(new Vector2Int(x, height - 1));
            }
            for (int y = 0; y < height; y++)
            {
                cells.Add(new Vector2Int(0, y));
                cells.Add(new Vector2Int(width - 1, y));
            }
            return cells;
        }

        private static void SetCoordinate(SerializedProperty property, int x, int y)
        {
            property.FindPropertyRelative("x").intValue = x;
            property.FindPropertyRelative("y").intValue = y;
        }

        private static void SetCoordinates(SerializedProperty property, IEnumerable<Vector2Int> values)
        {
            Vector2Int[] cells = values?.ToArray() ?? Array.Empty<Vector2Int>();
            property.arraySize = cells.Length;
            for (int i = 0; i < cells.Length; i++) SetCoordinate(property.GetArrayElementAtIndex(i), cells[i].x, cells[i].y);
        }

        private static void SetStrings(SerializedProperty property, IEnumerable<string> values)
        {
            string[] items = values?.ToArray() ?? Array.Empty<string>();
            property.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++) property.GetArrayElementAtIndex(i).stringValue = items[i];
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private readonly struct CrateSpec
        {
            public CrateSpec(string id, int x, int y, PuzzleEntityKind kind = PuzzleEntityKind.Crate)
            {
                Id = id; X = x; Y = y; Kind = kind;
            }
            public string Id { get; }
            public int X { get; }
            public int Y { get; }
            public PuzzleEntityKind Kind { get; }
        }

        private readonly struct GoalRequirementSpec
        {
            public GoalRequirementSpec(int x, int y, PuzzleEntityKind kind)
            {
                X = x; Y = y; Kind = kind;
            }
            public int X { get; }
            public int Y { get; }
            public PuzzleEntityKind Kind { get; }
        }

        private sealed class SwitchSpec
        {
            public SwitchSpec(string id, IEnumerable<Vector2Int> sensors, IEnumerable<Vector2Int> doors)
            {
                Id = id;
                Sensors = sensors?.ToArray() ?? Array.Empty<Vector2Int>();
                Doors = doors?.ToArray() ?? Array.Empty<Vector2Int>();
            }
            public string Id { get; }
            public IReadOnlyList<Vector2Int> Sensors { get; }
            public IReadOnlyList<Vector2Int> Doors { get; }
        }

        private sealed class LevelSpec
        {
            public LevelSpec(
                string id, string name, string sector, string briefing,
                int width, int height, Vector2Int player,
                IEnumerable<Vector2Int> internalWalls,
                IEnumerable<Vector2Int> goals,
                IEnumerable<CrateSpec> crates,
                int gold, int platinum,
                IEnumerable<string> tags,
                IEnumerable<Vector2Int> costlyCells,
                IEnumerable<SwitchSpec> switches,
                IEnumerable<GoalRequirementSpec> goalRequirements)
            {
                Id = id; Name = name; Sector = sector; Briefing = briefing;
                Width = width; Height = height; Player = player;
                InternalWalls = internalWalls?.ToArray() ?? Array.Empty<Vector2Int>();
                Goals = goals?.ToArray() ?? Array.Empty<Vector2Int>();
                Crates = crates?.ToArray() ?? Array.Empty<CrateSpec>();
                Gold = gold; Platinum = platinum;
                Tags = tags?.ToArray() ?? Array.Empty<string>();
                CostlyCells = costlyCells?.ToArray() ?? Array.Empty<Vector2Int>();
                Switches = switches?.ToArray() ?? Array.Empty<SwitchSpec>();
                GoalRequirements = goalRequirements?.ToArray() ?? Array.Empty<GoalRequirementSpec>();
            }
            public string Id { get; }
            public string Name { get; }
            public string Sector { get; }
            public string Briefing { get; }
            public int Width { get; }
            public int Height { get; }
            public Vector2Int Player { get; }
            public IReadOnlyList<Vector2Int> InternalWalls { get; }
            public IReadOnlyList<Vector2Int> Goals { get; }
            public IReadOnlyList<CrateSpec> Crates { get; }
            public int Gold { get; }
            public int Platinum { get; }
            public IReadOnlyList<string> Tags { get; }
            public IReadOnlyList<Vector2Int> CostlyCells { get; }
            public IReadOnlyList<SwitchSpec> Switches { get; }
            public IReadOnlyList<GoalRequirementSpec> GoalRequirements { get; }
        }
    }
}
#endif
