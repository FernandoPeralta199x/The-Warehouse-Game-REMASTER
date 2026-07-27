using System;
using UnityEngine;

namespace TW08.PowerUps
{
    [DisallowMultipleComponent]
    public sealed class PowerUpInventory : MonoBehaviour
    {
        public PowerUpDefinition Stored { get; private set; }
        public bool HasPowerUp => Stored != null;

        public event Action<PowerUpDefinition> StoredChanged;

        public bool TryStore(PowerUpDefinition definition)
        {
            if (definition == null || HasPowerUp)
            {
                return false;
            }

            Stored = definition;
            StoredChanged?.Invoke(Stored);
            return true;
        }

        public bool TryConsume(out PowerUpDefinition definition)
        {
            definition = Stored;
            if (definition == null)
            {
                return false;
            }

            Stored = null;
            StoredChanged?.Invoke(null);
            return true;
        }
    }
}
