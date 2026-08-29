using UnityEngine;

namespace TW08.Narrative
{
    /// <summary>
    /// Gatilho por volume: dispara uma sequência quando o operador pisa na área.
    ///
    /// Complementa o <see cref="NarrativeDirector"/>, que cuida dos momentos de
    /// campanha. Aqui é para fala presa a um lugar do mapa.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class NarrativeTrigger : MonoBehaviour
    {
        [SerializeField] private NarrativeService service;
        [SerializeField] private NarrativeSequence sequence;
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool disableAfterFiring = true;

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void Awake()
        {
            if (service == null)
            {
                service = FindFirstObjectByType<NarrativeService>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (sequence == null || other == null || !other.CompareTag(requiredTag))
            {
                return;
            }

            if (service == null)
            {
                service = FindFirstObjectByType<NarrativeService>();
                if (service == null)
                {
                    return;
                }
            }

            if (service.TryStart(sequence) && disableAfterFiring)
            {
                enabled = false;
            }
        }
    }
}
