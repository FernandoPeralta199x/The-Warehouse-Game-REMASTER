using UnityEngine;

namespace TW08.Race
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class SurfaceGripZone : MonoBehaviour
    {
        [SerializeField, Range(0.15f, 2f)] private float gripMultiplier = 0.45f;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            other.GetComponentInParent<ArcadeForkliftController2D>()?.SetSurfaceGripMultiplier(gripMultiplier);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            other.GetComponentInParent<ArcadeForkliftController2D>()?.SetSurfaceGripMultiplier(1f);
        }
    }
}
