using UnityEngine;

namespace TW08.Race
{
    [CreateAssetMenu(fileName = "RaceTrack", menuName = "TW08/Race/Track Definition")]
    public sealed class RaceTrackDefinition : ScriptableObject
    {
        [SerializeField] private string trackId = "receiving-loop";
        [SerializeField] private string displayName = "Receiving Loop";
        [SerializeField] private string sceneName = "TW08_Race01_ReceivingLoop";
        [SerializeField, Min(1)] private int laps = 3;
        [SerializeField, Min(1f)] private float bronzeTimeSeconds = 75f;
        [SerializeField, Min(1f)] private float silverTimeSeconds = 62f;
        [SerializeField, Min(1f)] private float goldTimeSeconds = 52f;
        [SerializeField, TextArea(2, 5)] private string briefing = string.Empty;
        [SerializeField] private Sprite previewImage;
        [SerializeField, Range(0.15f, 1f)] private float surfaceGrip = 1f;

        public string TrackId => string.IsNullOrWhiteSpace(trackId) ? name : trackId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string SceneName => sceneName;
        public int Laps => Mathf.Max(1, laps);
        public float BronzeTimeSeconds => bronzeTimeSeconds;
        public float SilverTimeSeconds => silverTimeSeconds;
        public float GoldTimeSeconds => goldTimeSeconds;
        public string Briefing => briefing;
        public Sprite PreviewImage => previewImage;
        public float SurfaceGrip => Mathf.Clamp(surfaceGrip, 0.15f, 1f);

        public int GetMedal(float elapsedSeconds)
        {
            if (elapsedSeconds <= goldTimeSeconds)
            {
                return 3;
            }

            if (elapsedSeconds <= silverTimeSeconds)
            {
                return 2;
            }

            if (elapsedSeconds <= bronzeTimeSeconds)
            {
                return 1;
            }

            return 0;
        }

        private void OnValidate()
        {
            trackId = string.IsNullOrWhiteSpace(trackId) ? name.ToLowerInvariant() : trackId.Trim().ToLowerInvariant();
            displayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            laps = Mathf.Max(1, laps);
            goldTimeSeconds = Mathf.Max(1f, goldTimeSeconds);
            silverTimeSeconds = Mathf.Max(goldTimeSeconds, silverTimeSeconds);
            bronzeTimeSeconds = Mathf.Max(silverTimeSeconds, bronzeTimeSeconds);
            surfaceGrip = Mathf.Clamp(surfaceGrip, 0.15f, 1f);
        }
    }
}
