using UnityEngine;

namespace TW08.Narrative
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class NarrativeTrigger : MonoBehaviour
    {
        [SerializeField] private NarrativeService service;
        [SerializeField] private NarrativeSequence sequence;
        [SerializeField] private string requiredTag = "Player";

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (service == null || sequence == null || !other.CompareTag(requiredTag))
            {
                return;
            }

            service.TryStart(sequence);
        }
    }
}
