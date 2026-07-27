using UnityEngine;

namespace TW08.Race
{
    [CreateAssetMenu(fileName = "Race", menuName = "TW08/Race/Race Definition")]
    public sealed class RaceDefinition : ScriptableObject
    {
        [SerializeField] private string raceId = "race-id";
        [SerializeField, Min(1)] private int laps = 3;
        [SerializeField, Min(0f)] private float countdownSeconds = 3f;
        [SerializeField, Min(0f)] private float bronzeTime = 180f;
        [SerializeField, Min(0f)] private float silverTime = 140f;
        [SerializeField, Min(0f)] private float goldTime = 115f;
        [SerializeField, Min(0f)] private float platinumTime = 100f;
        [SerializeField, Min(0f)] private float maximumCargoDamageForGold = 5f;

        public string RaceId => raceId;
        public int Laps => laps;
        public float CountdownSeconds => countdownSeconds;
        public float MaximumCargoDamageForGold => maximumCargoDamageForGold;

        public int EvaluateMedal(float elapsedSeconds, float cargoDamage)
        {
            if (elapsedSeconds <= platinumTime && cargoDamage <= 0f) return 4;
            if (elapsedSeconds <= goldTime && cargoDamage <= maximumCargoDamageForGold) return 3;
            if (elapsedSeconds <= silverTime) return 2;
            if (elapsedSeconds <= bronzeTime) return 1;
            return 0;
        }
    }
}
