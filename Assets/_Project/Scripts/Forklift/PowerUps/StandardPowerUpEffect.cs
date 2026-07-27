using TW08.Race;
using UnityEngine;

namespace TW08.PowerUps
{
    [CreateAssetMenu(fileName = "StandardPowerUpEffect", menuName = "TW08/Power Ups/Standard Effect")]
    public sealed class StandardPowerUpEffect : PowerUpEffect
    {
        public override bool TryApply(PowerUpDefinition definition, PowerUpContext context)
        {
            if (definition == null)
            {
                return false;
            }

            switch (definition.Type)
            {
                case PowerUpType.TurboCompressor:
                    context.Controller?.ApplyBoost(Mathf.Max(1f, definition.Magnitude), definition.Duration);
                    return context.Controller != null;

                case PowerUpType.SafetyBarrier:
                    context.Damage?.GrantShield(Mathf.Max(1, Mathf.RoundToInt(definition.Magnitude)));
                    return context.Damage != null;

                case PowerUpType.OilCanister:
                    if (definition.SpawnedPrefab == null || context.User == null) return false;
                    Instantiate(definition.SpawnedPrefab, context.User.position - context.User.up * 1.2f, context.User.rotation);
                    return true;

                case PowerUpType.EmpSignal:
                    if (context.User == null) return false;
                    Collider2D[] hits = Physics2D.OverlapCircleAll(context.User.position, definition.Radius, context.RacerLayers);
                    foreach (Collider2D hit in hits)
                    {
                        ArcadeForkliftController2D other = hit.GetComponentInParent<ArcadeForkliftController2D>();
                        if (other != null && other != context.Controller)
                        {
                            other.ApplySlow(Mathf.Clamp(definition.Magnitude, 0.2f, 0.95f), definition.Duration);
                        }
                    }
                    return true;

                case PowerUpType.RepairKit:
                    context.Damage?.Repair(definition.Magnitude);
                    return context.Damage != null;

                default:
                    return false;
            }
        }
    }
}
