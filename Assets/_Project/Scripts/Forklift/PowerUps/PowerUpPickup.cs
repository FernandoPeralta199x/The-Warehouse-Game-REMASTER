using System.Collections;
using TW08.Race;
using UnityEngine;

namespace TW08.PowerUps
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PowerUpPickup : MonoBehaviour
    {
        [SerializeField] private WeightedPowerUpTable table;
        [SerializeField] private RaceManager raceManager;
        [SerializeField, Min(0.1f)] private float respawnDelay = 5f;
        [SerializeField] private Renderer[] visuals;
        private Collider2D trigger;
        private bool available = true;

        private void Awake()
        {
            trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;

            if (visuals == null || visuals.Length == 0)
            {
                visuals = GetComponentsInChildren<Renderer>(true);
            }
        }

        public void Configure(WeightedPowerUpTable weightedTable, RaceManager manager)
        {
            table = weightedTable;
            raceManager = manager;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!available || table == null)
            {
                return;
            }

            PowerUpInventory inventory = other.GetComponentInParent<PowerUpInventory>();
            RacerProgress progress = other.GetComponentInParent<RacerProgress>();
            if (inventory == null || progress == null)
            {
                return;
            }

            float rank = raceManager == null ? 0.5f : raceManager.GetNormalizedRank(progress);
            PowerUpDefinition selected = table.Choose(rank);
            if (inventory.TryStore(selected))
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            available = false;
            trigger.enabled = false;
            SetVisuals(false);
            yield return new WaitForSeconds(respawnDelay);
            SetVisuals(true);
            trigger.enabled = true;
            available = true;
        }

        private void SetVisuals(bool value)
        {
            foreach (Renderer visual in visuals)
            {
                if (visual != null)
                {
                    visual.enabled = value;
                }
            }
        }
    }
}
