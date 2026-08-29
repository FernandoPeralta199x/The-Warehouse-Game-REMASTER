using TW08.Race;
using UnityEngine;

namespace TW08.Audio
{
    /// <summary>
    /// Áudio da corrida.
    ///
    /// Antes só a contagem regressiva e a chegada soavam: a empilhadeira era
    /// muda enquanto o jogador dirigia, batia e dava ré — que é justamente o que
    /// ele faz o tempo todo. Motor, ré e colisão são a matéria-prima da sensação
    /// de peso do veículo.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaceAudioFeedback : MonoBehaviour
    {
        [SerializeField] private RaceSessionController session;
        [SerializeField] private TW08AudioCatalog catalog;
        [SerializeField] private ArcadeForkliftController2D forklift;

        private Rigidbody2D forkliftBody;
        private bool engineRunning;
        private bool reverseRunning;

        public void Configure(
            RaceSessionController raceSession,
            TW08AudioCatalog audioCatalog,
            ArcadeForkliftController2D playerForklift = null)
        {
            session = raceSession;
            catalog = audioCatalog;
            forklift = playerForklift;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable()
        {
            ResolveForklift();

            if (forklift != null)
            {
                forklift.Impacted += OnImpact;
            }

            if (session == null) return;
            session.CountdownChanged += OnCountdown;
            session.PlayerFinished += OnFinished;
        }

        private void OnDisable()
        {
            if (forklift != null)
            {
                forklift.Impacted -= OnImpact;
            }

            StopEngine();
            StopReverse();

            if (session == null) return;
            session.CountdownChanged -= OnCountdown;
            session.PlayerFinished -= OnFinished;
        }

        /// <summary>
        /// As pistas já existentes foram geradas sem este campo, então a
        /// empilhadeira do jogador é localizada na cena em vez de exigir que
        /// todas as cenas sejam refeitas.
        /// </summary>
        private void ResolveForklift()
        {
            if (forklift == null)
            {
                foreach (ArcadeForkliftController2D candidate in
                         FindObjectsByType<ArcadeForkliftController2D>(FindObjectsSortMode.None))
                {
                    if (candidate != null && candidate.PlayerControlled)
                    {
                        forklift = candidate;
                        break;
                    }
                }
            }

            if (forklift != null)
            {
                forklift.TryGetComponent(out forkliftBody);
            }
        }

        private void Update()
        {
            if (catalog == null || AudioService.Instance == null || forklift == null)
            {
                return;
            }

            // O motor só soa em movimento: um loop com a empilhadeira parada
            // vira zumbido constante enquanto o jogador pensa na rota.
            bool moving = forklift.NormalizedSpeed > 0.05f;
            SetEngine(moving);

            // Ré é detectada pelo sentido da velocidade contra a frente do
            // veículo, e não pelo comando — assim o bipe acompanha o que está
            // acontecendo, não o que foi pedido.
            bool reversing = moving
                             && forkliftBody != null
                             && Vector2.Dot(forklift.transform.up, forkliftBody.linearVelocity) < -0.1f;
            SetReverse(reversing);
        }

        private void SetEngine(bool running)
        {
            if (running == engineRunning)
            {
                return;
            }

            if (running)
            {
                AudioService.Instance.StartLoop(catalog.ForkliftEngine, transform.position);
                engineRunning = true;
                return;
            }

            StopEngine();
        }

        private void SetReverse(bool running)
        {
            if (running == reverseRunning)
            {
                return;
            }

            if (running)
            {
                AudioService.Instance.StartLoop(catalog.ForkliftReverse, transform.position);
                reverseRunning = true;
                return;
            }

            StopReverse();
        }

        private void OnImpact(float force)
        {
            AudioService.Instance?.PlayOneShot(catalog?.ForkliftImpact, transform.position);
        }

        private void StopEngine()
        {
            if (engineRunning && catalog != null && catalog.ForkliftEngine != null)
            {
                AudioService.Instance?.StopLoop(catalog.ForkliftEngine.EventId);
            }

            engineRunning = false;
        }

        private void StopReverse()
        {
            if (reverseRunning && catalog != null && catalog.ForkliftReverse != null)
            {
                AudioService.Instance?.StopLoop(catalog.ForkliftReverse.EventId);
            }

            reverseRunning = false;
        }

        private void OnCountdown(int value)
        {
            if (value > 0) AudioService.Instance?.PlayOneShot(catalog?.RaceCountdown, transform.position);
        }

        private void OnFinished(float _, int __)
        {
            StopEngine();
            StopReverse();
            AudioService.Instance?.PlayOneShot(catalog?.RaceFinish, transform.position);
        }
    }
}
