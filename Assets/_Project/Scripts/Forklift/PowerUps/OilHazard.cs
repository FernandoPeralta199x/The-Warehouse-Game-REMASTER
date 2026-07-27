using System.Collections;
using TW08.Race;
using UnityEngine;

namespace TW08.PowerUps
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class OilHazard : MonoBehaviour
    {
        [SerializeField, Range(0.2f, 1f)] private float speedMultiplier = 0.55f;
        [SerializeField, Min(0.1f)] private float slowDuration = 1.2f;
        [SerializeField, Min(0.1f)] private float lifetime = 8f;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
            StartCoroutine(LifetimeRoutine());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ArcadeForkliftController2D controller = other.GetComponentInParent<ArcadeForkliftController2D>();
            if (controller != null)
            {
                controller.ApplySlow(speedMultiplier, slowDuration);
                Destroy(gameObject);
            }
        }

        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(lifetime);
            Destroy(gameObject);
        }
    }
}
