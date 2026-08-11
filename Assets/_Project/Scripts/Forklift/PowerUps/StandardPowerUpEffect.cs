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
                case PowerUpType.HydraulicNitro:
                    context.Controller?.ApplyBoost(Mathf.Max(1f, definition.Magnitude), definition.Duration);
                    return context.Controller != null;

                case PowerUpType.SafetyBarrier:
                    context.Damage?.GrantShield(Mathf.Max(1, Mathf.RoundToInt(definition.Magnitude)));
                    return context.Damage != null;

                case PowerUpType.CargoStabilizer:
                    if (context.Controller == null) return false;
                    context.Controller.ApplyGripAssist(Mathf.Max(1.15f, definition.Magnitude), definition.Duration);
                    context.Controller.ApplyImpactProtection(0.45f, definition.Duration);
                    context.Damage?.GrantShield(1);
                    return true;

                case PowerUpType.AbsBrake:
                    if (context.Controller == null) return false;
                    context.Controller.ApplyGripAssist(Mathf.Max(1.35f, definition.Magnitude), definition.Duration);
                    context.Controller.ApplyHandlingAssist(1.18f, definition.Duration);
                    return true;

                case PowerUpType.MagneticFork:
                    if (context.Controller == null) return false;
                    context.Controller.ApplyGripAssist(Mathf.Max(1.2f, definition.Magnitude), definition.Duration);
                    context.Controller.ApplyImpactProtection(0.65f, definition.Duration);
                    return true;

                case PowerUpType.ReinforcedSuspension:
                    if (context.Controller == null) return false;
                    context.Controller.ApplyImpactProtection(Mathf.Clamp(definition.Magnitude, 0.2f, 0.8f), definition.Duration);
                    return true;

                case PowerUpType.OilCanister:
                    if (definition.SpawnedPrefab == null || context.User == null) return false;
                    Instantiate(definition.SpawnedPrefab, context.User.position - context.User.up * 1.2f, context.User.rotation);
                    return true;

                case PowerUpType.EmpSignal:
                case PowerUpType.IndustrialHorn:
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

                case PowerUpType.RouteScanner:
                    // Reserved for the route-overlay subsystem. Do not consume the item until that
                    // presenter is present on the vehicle.
                    RaceRouteScanner scanner = context.User != null
                        ? context.User.GetComponent<RaceRouteScanner>()
                        : null;
                    if (scanner == null) return false;
                    scanner.Reveal(definition.Duration);
                    return true;

                default:
                    return false;
            }
        }
    }
}
