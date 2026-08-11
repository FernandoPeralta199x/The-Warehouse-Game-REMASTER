using TW08.Race;
using UnityEngine;

namespace TW08.PowerUps
{
    [DisallowMultipleComponent]
    public sealed class RaceAiPowerUpDriver : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private RacerProgress progress;
        [SerializeField] private PowerUpInventory inventory;
        [SerializeField] private PowerUpExecutor executor;
        [SerializeField, Min(0.1f)] private float decisionInterval = 0.65f;

        private float nextDecisionTime;

        public void Configure(
            RaceManager manager,
            RacerProgress racerProgress,
            PowerUpInventory powerUpInventory,
            PowerUpExecutor powerUpExecutor)
        {
            raceManager = manager;
            progress = racerProgress;
            inventory = powerUpInventory;
            executor = powerUpExecutor;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Update()
        {
            if (raceManager == null || progress == null || inventory == null || executor == null ||
                !raceManager.RaceRunning || progress.Finished || !inventory.HasPowerUp || Time.time < nextDecisionTime)
            {
                return;
            }

            nextDecisionTime = Time.time + decisionInterval;
            PowerUpDefinition stored = inventory.Stored;
            if (stored == null)
            {
                return;
            }

            float rank = raceManager.GetNormalizedRank(progress);
            if (ShouldUse(stored.Type, rank))
            {
                executor.UseStored();
            }
        }

        private static bool ShouldUse(PowerUpType type, float normalizedRank)
        {
            return type switch
            {
                PowerUpType.RepairKit => true,
                PowerUpType.SafetyBarrier => true,
                PowerUpType.CargoStabilizer => true,
                PowerUpType.AbsBrake => true,
                PowerUpType.ReinforcedSuspension => true,
                PowerUpType.EmpSignal => normalizedRank > 0.15f,
                PowerUpType.IndustrialHorn => normalizedRank > 0.2f,
                PowerUpType.TurboCompressor => normalizedRank > 0.2f,
                PowerUpType.HydraulicNitro => normalizedRank > 0.2f,
                _ => normalizedRank > 0.35f
            };
        }
    }
}
