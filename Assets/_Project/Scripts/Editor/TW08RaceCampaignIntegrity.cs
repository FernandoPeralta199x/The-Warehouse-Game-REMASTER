#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TW08.Race;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Keeps the canonical Logistics Rush campaign structurally valid.
    ///
    /// This repair is deliberately narrow: it does not call the full production pipeline,
    /// does not refresh the AssetDatabase and does not create scenes. Its only job is to
    /// restore the three canonical race definitions and their campaign references.
    /// </summary>
    [InitializeOnLoad]
    public static class TW08RaceCampaignIntegrity
    {
        private const int ExpectedTrackCount = 3;

        private static readonly TrackSpec[] Specs =
        {
            new(
                "receiving-loop",
                "Receiving Loop",
                "TW08_Race01_ReceivingLoop",
                "Tutorial de pilotagem na doca de recebimento. Linhas amplas, boost controlado e leitura de checkpoint.",
                75f,
                65f,
                58f,
                52f,
                1f),
            new(
                "industrial-corridor",
                "Industrial Corridor",
                "TW08_Race02_IndustrialCorridor",
                "Corredores técnicos, chicanes e barreiras exigem freio e direção precisa.",
                95f,
                82f,
                72f,
                65f,
                0.9f),
            new(
                "frozen-route",
                "Frozen Route",
                "TW08_Race03_FrozenRoute",
                "Câmara fria com zonas de baixa aderência. Antecipe a curva e use boost apenas na saída.",
                110f,
                95f,
                84f,
                76f,
                0.45f)
        };

        static TW08RaceCampaignIntegrity()
        {
            EditorApplication.delayCall += RepairOnReloadIfNeeded;
        }

        [MenuItem("Tools/TW08/Production/Repair Race Campaign")]
        public static void RepairFromMenu()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Race Campaign",
                    "Saia do Play Mode antes de reparar a campanha de corrida.",
                    "OK");
                return;
            }

            try
            {
                RaceCampaignDefinition campaign = EnsureValidCampaign(force: true);
                EditorGUIUtility.PingObject(campaign);
                Selection.activeObject = campaign;
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Race Campaign",
                    "Campanha de corrida reparada e validada.\n\n" +
                    "01 — Receiving Loop\n" +
                    "02 — Industrial Corridor\n" +
                    "03 — Frozen Route\n\n" +
                    "Nenhum AssetDatabase.Refresh foi executado pelo reparo.",
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogError("[TW08] Race campaign repair failed.\n" + exception);
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Race Campaign falhou",
                    exception.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Returns a canonical, freshly reloaded campaign. Repairs persisted corruption when needed.
        /// Safe to call as a pre-flight before scene upgrades.
        /// </summary>
        public static RaceCampaignDefinition EnsureValidCampaign(bool force = false)
        {
            RaceCampaignDefinition campaign = AssetDatabase.LoadAssetAtPath<RaceCampaignDefinition>(
                TW08ExpansionDataSetup.RaceCampaignPath);

            if (!force && IsValid(campaign, out _))
            {
                return campaign;
            }

            EnsureFolder(TW08ExpansionDataSetup.RaceRoot);

            List<RaceTrackDefinition> tracks = new(ExpectedTrackCount);
            foreach (TrackSpec spec in Specs)
            {
                tracks.Add(EnsureTrack(spec));
            }

            campaign = LoadOrCreate<RaceCampaignDefinition>(TW08ExpansionDataSetup.RaceCampaignPath);
            SerializedObject serializedCampaign = new(campaign);
            SerializedProperty trackList = serializedCampaign.FindProperty("tracks");
            if (trackList == null)
            {
                throw new InvalidOperationException(
                    "RaceCampaignDefinition no longer exposes the serialized 'tracks' field expected by the editor pipeline.");
            }

            trackList.arraySize = tracks.Count;
            for (int i = 0; i < tracks.Count; i++)
            {
                trackList.GetArrayElementAtIndex(i).objectReferenceValue = tracks[i];
            }

            serializedCampaign.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(campaign);
            AssetDatabase.SaveAssets();

            // Do not keep native Unity object references captured before another pipeline refresh.
            // Re-resolve the campaign from the AssetDatabase before returning it to callers.
            campaign = AssetDatabase.LoadAssetAtPath<RaceCampaignDefinition>(
                TW08ExpansionDataSetup.RaceCampaignPath);

            if (!IsValid(campaign, out string issue))
            {
                throw new InvalidOperationException(
                    "Race campaign repair completed but validation still failed: " + issue);
            }

            return campaign;
        }

        public static bool IsValid(RaceCampaignDefinition campaign, out string issue)
        {
            if (campaign == null)
            {
                issue = "campaign asset is missing";
                return false;
            }

            IReadOnlyList<RaceTrackDefinition> tracks = campaign.Tracks;
            if (tracks == null)
            {
                issue = "campaign track list is null";
                return false;
            }

            if (tracks.Count != ExpectedTrackCount)
            {
                issue = $"expected {ExpectedTrackCount} tracks but found {tracks.Count}";
                return false;
            }

            HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> scenes = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < tracks.Count; i++)
            {
                RaceTrackDefinition track = tracks[i];
                TrackSpec expected = Specs[i];
                string position = $"track index {i} ({expected.Name})";

                if (track == null)
                {
                    issue = position + " is null or has a broken asset reference";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(track.TrackId))
                {
                    issue = position + " has an empty TrackId";
                    return false;
                }

                if (!string.Equals(track.TrackId, expected.Id, StringComparison.OrdinalIgnoreCase))
                {
                    issue = position + $" has TrackId '{track.TrackId}', expected '{expected.Id}'";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(track.SceneName))
                {
                    issue = position + " has an empty SceneName";
                    return false;
                }

                if (!string.Equals(track.SceneName, expected.SceneName, StringComparison.Ordinal))
                {
                    issue = position + $" targets scene '{track.SceneName}', expected '{expected.SceneName}'";
                    return false;
                }

                if (track.RaceRules == null)
                {
                    issue = position + " has no RaceDefinition assigned";
                    return false;
                }

                if (!string.Equals(track.RaceRules.RaceId, expected.Id, StringComparison.OrdinalIgnoreCase))
                {
                    issue = position + $" uses RaceDefinition '{track.RaceRules.RaceId}', expected '{expected.Id}'";
                    return false;
                }

                if (!ids.Add(track.TrackId))
                {
                    issue = position + $" duplicates TrackId '{track.TrackId}'";
                    return false;
                }

                if (!scenes.Add(track.SceneName))
                {
                    issue = position + $" duplicates SceneName '{track.SceneName}'";
                    return false;
                }
            }

            issue = string.Empty;
            return true;
        }

        private static void RepairOnReloadIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            RaceCampaignDefinition campaign = AssetDatabase.LoadAssetAtPath<RaceCampaignDefinition>(
                TW08ExpansionDataSetup.RaceCampaignPath);

            // Fresh projects are allowed to have no campaign yet. The production builder owns creation.
            if (campaign == null || IsValid(campaign, out _))
            {
                return;
            }

            IsValid(campaign, out string originalIssue);

            try
            {
                EnsureValidCampaign();
                Debug.LogWarning(
                    "[TW08] Invalid persisted race campaign was repaired before production authoring. " +
                    "Original issue: " + originalIssue);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[TW08] Automatic race campaign repair failed. Run " +
                    "Tools/TW08/Production/Repair Race Campaign for a focused retry.\n" + exception);
            }
        }

        private static RaceTrackDefinition EnsureTrack(TrackSpec spec)
        {
            string rulesPath = TW08ExpansionDataSetup.RaceRoot + "/RaceRules_" + spec.Id + ".asset";
            RaceDefinition rules = LoadOrCreate<RaceDefinition>(rulesPath);
            SerializedObject serializedRules = new(rules);
            RequireProperty(serializedRules, "raceId").stringValue = spec.Id;
            RequireProperty(serializedRules, "laps").intValue = 3;
            RequireProperty(serializedRules, "countdownSeconds").floatValue = 3f;
            RequireProperty(serializedRules, "bronzeTime").floatValue = spec.Bronze;
            RequireProperty(serializedRules, "silverTime").floatValue = spec.Silver;
            RequireProperty(serializedRules, "goldTime").floatValue = spec.Gold;
            RequireProperty(serializedRules, "platinumTime").floatValue = spec.Platinum;
            RequireProperty(serializedRules, "maximumCargoDamageForGold").floatValue = 5f;
            serializedRules.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(rules);

            string trackPath = TW08ExpansionDataSetup.RaceRoot + "/Track_" + spec.Id + ".asset";
            RaceTrackDefinition track = LoadOrCreate<RaceTrackDefinition>(trackPath);
            SerializedObject serializedTrack = new(track);
            RequireProperty(serializedTrack, "trackId").stringValue = spec.Id;
            RequireProperty(serializedTrack, "displayName").stringValue = spec.Name;
            RequireProperty(serializedTrack, "sceneName").stringValue = spec.SceneName;
            RequireProperty(serializedTrack, "raceRules").objectReferenceValue = rules;
            RequireProperty(serializedTrack, "briefing").stringValue = spec.Briefing;
            RequireProperty(serializedTrack, "surfaceGrip").floatValue = spec.SurfaceGrip;
            serializedTrack.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(track);
            return track;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"{serializedObject.targetObject.GetType().Name} no longer exposes serialized field '{propertyName}'.");
            }

            return property;
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

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string[] parts = path.Split('/');
            if (parts.Length == 0 || !string.Equals(parts[0], "Assets", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Editor asset folder must be rooted at Assets/: " + path);
            }

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private readonly struct TrackSpec
        {
            public TrackSpec(
                string id,
                string name,
                string sceneName,
                string briefing,
                float bronze,
                float silver,
                float gold,
                float platinum,
                float surfaceGrip)
            {
                Id = id;
                Name = name;
                SceneName = sceneName;
                Briefing = briefing;
                Bronze = bronze;
                Silver = silver;
                Gold = gold;
                Platinum = platinum;
                SurfaceGrip = surfaceGrip;
            }

            public string Id { get; }
            public string Name { get; }
            public string SceneName { get; }
            public string Briefing { get; }
            public float Bronze { get; }
            public float Silver { get; }
            public float Gold { get; }
            public float Platinum { get; }
            public float SurfaceGrip { get; }
        }
    }
}
#endif
