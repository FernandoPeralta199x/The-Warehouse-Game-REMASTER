using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW08.PowerUps
{
    [Serializable]
    public sealed class WeightedPowerUpEntry
    {
        [SerializeField] private PowerUpDefinition definition;
        [SerializeField, Range(0f, 1f)] private float minimumRank;
        [SerializeField, Range(0f, 1f)] private float maximumRank = 1f;
        [SerializeField, Min(0f)] private float weight = 1f;

        public PowerUpDefinition Definition => definition;
        public float MinimumRank => minimumRank;
        public float MaximumRank => maximumRank;
        public float Weight => weight;
    }

    [CreateAssetMenu(fileName = "PowerUpTable", menuName = "TW08/Power Ups/Weighted Table")]
    public sealed class WeightedPowerUpTable : ScriptableObject
    {
        [SerializeField] private List<WeightedPowerUpEntry> entries = new();

        public PowerUpDefinition Choose(float normalizedRank, System.Random random = null)
        {
            normalizedRank = Mathf.Clamp01(normalizedRank);
            random ??= new System.Random();
            float total = 0f;

            foreach (WeightedPowerUpEntry entry in entries)
            {
                if (IsEligible(entry, normalizedRank))
                {
                    total += entry.Weight;
                }
            }

            if (total <= 0f)
            {
                return null;
            }

            double roll = random.NextDouble() * total;
            float accumulated = 0f;

            foreach (WeightedPowerUpEntry entry in entries)
            {
                if (!IsEligible(entry, normalizedRank))
                {
                    continue;
                }

                accumulated += entry.Weight;
                if (roll <= accumulated)
                {
                    return entry.Definition;
                }
            }

            return null;
        }

        private static bool IsEligible(WeightedPowerUpEntry entry, float rank)
        {
            return entry != null && entry.Definition != null && entry.Weight > 0f &&
                   rank >= entry.MinimumRank && rank <= entry.MaximumRank;
        }
    }
}
