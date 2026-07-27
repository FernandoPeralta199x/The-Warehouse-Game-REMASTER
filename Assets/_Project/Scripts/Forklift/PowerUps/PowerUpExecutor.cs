using TW08.Input;
using TW08.Race;
using UnityEngine;

namespace TW08.PowerUps
{
    [DisallowMultipleComponent]
    public sealed class PowerUpExecutor : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private PowerUpInventory inventory;
        [SerializeField] private ArcadeForkliftController2D controller;
        [SerializeField] private ForkliftDamage damage;
        [SerializeField] private LayerMask racerLayers = ~0;
        [SerializeField] private StandardPowerUpEffect fallbackEffect;

        private StandardPowerUpEffect runtimeFallback;

        private void Awake()
        {
            if (fallbackEffect == null)
            {
                runtimeFallback = ScriptableObject.CreateInstance<StandardPowerUpEffect>();
            }
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.RacePowerUpRequested += UseStored;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.RacePowerUpRequested -= UseStored;
            }
        }

        public void Configure(GameInput gameInput, PowerUpInventory powerUpInventory, ArcadeForkliftController2D forklift, ForkliftDamage forkliftDamage)
        {
            input = gameInput;
            inventory = powerUpInventory;
            controller = forklift;
            damage = forkliftDamage;
        }

        public void UseStored()
        {
            if (inventory == null || !inventory.TryConsume(out PowerUpDefinition definition))
            {
                return;
            }

            if (!Execute(definition))
            {
                inventory.TryStore(definition);
            }
        }

        private void OnDestroy()
        {
            if (runtimeFallback != null)
            {
                Destroy(runtimeFallback);
            }
        }

        public bool Execute(PowerUpDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            PowerUpEffect effect = definition.Effect != null ? definition.Effect : (fallbackEffect != null ? fallbackEffect : runtimeFallback);
            if (effect == null)
            {
                Debug.LogWarning($"Power Up '{definition.Id}' has no effect strategy.", definition);
                return false;
            }

            PowerUpContext context = new(transform, controller, damage, racerLayers);
            return effect.TryApply(definition, context);
        }
    }
}
