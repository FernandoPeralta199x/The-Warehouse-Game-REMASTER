using UnityEngine;

namespace TW08.PowerUps
{
    [CreateAssetMenu(fileName = "PowerUp", menuName = "TW08/Power Ups/Definition")]
    public sealed class PowerUpDefinition : ScriptableObject
    {
        [SerializeField] private string id = "powerup";
        [SerializeField] private string displayName = "Power Up";
        [SerializeField] private PowerUpType type;
        [SerializeField] private PowerUpEffect effect;
        [SerializeField, Min(0f)] private float magnitude = 1f;
        [SerializeField, Min(0f)] private float duration = 1f;
        [SerializeField, Min(0f)] private float radius = 5f;
        [SerializeField] private GameObject spawnedPrefab;
        [SerializeField] private Sprite icon;

        public string Id => id;
        public string DisplayName => displayName;
        public PowerUpType Type => type;
        public PowerUpEffect Effect => effect;
        public float Magnitude => magnitude;
        public float Duration => duration;
        public float Radius => radius;
        public GameObject SpawnedPrefab => spawnedPrefab;
        public Sprite Icon => icon;
    }
}
