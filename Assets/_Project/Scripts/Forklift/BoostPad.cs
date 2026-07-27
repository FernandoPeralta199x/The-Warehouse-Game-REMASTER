using UnityEngine;

namespace TW08.Race
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BoostPad : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float multiplier = 1.25f;
        [SerializeField, Min(0.05f)] private float duration = 0.65f;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ArcadeForkliftController2D controller = other.GetComponentInParent<ArcadeForkliftController2D>();
            controller?.ApplyBoost(multiplier, duration);
        }
    }
}
