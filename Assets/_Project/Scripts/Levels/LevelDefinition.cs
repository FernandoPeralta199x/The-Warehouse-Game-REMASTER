using System;
using System.Collections.Generic;
using TW08.Core;
using UnityEngine;

namespace TW08.Levels
{
    [Serializable]
    public sealed class LevelUnlockRequirement
    {
        [SerializeField] private string requiredLevelId;
        [SerializeField, Min(0)] private int minimumMedal;

        public string RequiredLevelId => requiredLevelId;
        public int MinimumMedal => minimumMedal;
    }

    [CreateAssetMenu(fileName = "Level", menuName = "TW08/Levels/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId = "level-id";
        [SerializeField] private string displayName = "Level";
        [SerializeField] private string sceneName;
        [SerializeField] private GameMode gameMode = GameMode.Puzzle;
        [SerializeField] private SectorId sector = SectorId.Receiving;
        [SerializeField] private bool secret;
        [SerializeField] private LevelUnlockRequirement[] requirements = Array.Empty<LevelUnlockRequirement>();
        [SerializeField] private string musicEventId;
        [SerializeField] private string ambienceEventId;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public GameMode GameMode => gameMode;
        public SectorId Sector => sector;
        public bool IsSecret => secret;
        public IReadOnlyList<LevelUnlockRequirement> Requirements => requirements;
        public string MusicEventId => musicEventId;
        public string AmbienceEventId => ambienceEventId;
    }
}
