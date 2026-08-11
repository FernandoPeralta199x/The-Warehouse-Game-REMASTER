using UnityEngine;

namespace TW08.Race
{
    [CreateAssetMenu(fileName = "RaceTrack", menuName = "TW08/Race/Track Definition")]
    public sealed class RaceTrackDefinition : ScriptableObject
    {
        [SerializeField] private string trackId = "receiving-loop";
        [SerializeField] private string displayName = "Receiving Loop";
        [SerializeField] private string sceneName = "TW08_Race01_ReceivingLoop";
        [SerializeField] private RaceDefinition raceRules;
        [SerializeField, TextArea(2, 5)] private string briefing = string.Empty;
        [SerializeField] private Sprite previewImage;
        [SerializeField, Range(0.15f, 1f)] private float surfaceGrip = 1f;

        public string TrackId => string.IsNullOrWhiteSpace(trackId) ? name : trackId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string SceneName => sceneName;
        public RaceDefinition RaceRules => raceRules;
        public int Laps => raceRules != null ? raceRules.Laps : 3;
        public string Briefing => briefing;
        public Sprite PreviewImage => previewImage;
        public float SurfaceGrip => Mathf.Clamp(surfaceGrip, 0.15f, 1f);

        public int GetMedal(float elapsedSeconds, float cargoDamage = 0f)
        {
            return raceRules != null ? raceRules.EvaluateMedal(elapsedSeconds, cargoDamage) : 0;
        }

        private void OnValidate()
        {
            trackId = string.IsNullOrWhiteSpace(trackId) ? name.ToLowerInvariant() : trackId.Trim().ToLowerInvariant();
            displayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            surfaceGrip = Mathf.Clamp(surfaceGrip, 0.15f, 1f);
        }
    }
}
