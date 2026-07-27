using UnityEngine;

namespace TW08.PowerUps
{
    public abstract class PowerUpEffect : ScriptableObject
    {
        public abstract bool TryApply(PowerUpDefinition definition, PowerUpContext context);
    }
}
