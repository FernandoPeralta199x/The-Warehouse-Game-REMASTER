using TW08.Race;
using UnityEngine;

namespace TW08.Presentation
{
    /// <summary>
    /// Câmera de perseguição da corrida.
    ///
    /// A câmera era ortográfica e **fixa**: a pista inteira cabia na tela e o
    /// veículo virava um ponto se afastando. Sem movimento de câmera não há
    /// sensação de velocidade — o cérebro lê velocidade pelo que passa ao lado,
    /// não pelo número no HUD.
    ///
    /// Aqui ela persegue por trás e **gira junto com o veículo**, que é o truque
    /// que jogos de kart em vista superior usam para dar a leitura de "atrás do
    /// carro" sem 3D: a pista é que roda, e a frente do veículo aponta sempre
    /// para o topo da tela.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class RaceChaseCamera : MonoBehaviour
    {
        [SerializeField] private ArcadeForkliftController2D target;

        [Header("Perseguição")]
        [Tooltip("Quanto a câmera se adianta na direção do movimento.")]
        [SerializeField, Min(0f)] private float lookAhead = 1.35f;
        [SerializeField, Min(0.01f)] private float positionSmoothing = 0.09f;

        [Header("Rotação")]
        [Tooltip("Desligue para manter a vista fixa, como uma câmera de teto.")]
        [SerializeField] private bool rotateWithVehicle = true;
        [SerializeField, Min(1f)] private float rotationSpeed = 6.5f;

        [Header("Enquadramento")]
        [SerializeField, Min(1f)] private float restSize = 5.2f;
        [SerializeField, Min(1f)] private float topSpeedSize = 7.4f;
        [SerializeField, Min(0.1f)] private float zoomSpeed = 2.4f;

        [Header("Impacto")]
        [SerializeField, Min(0f)] private float shakePerImpact = 0.22f;
        [SerializeField, Min(0.1f)] private float shakeDecay = 4.5f;

        private Camera cam;
        private Vector3 velocity;
        private float shake;
        private float seed;

        public void Configure(ArcadeForkliftController2D vehicle)
        {
            target = vehicle;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            seed = Random.value * 100f;
        }

        private void OnEnable()
        {
            if (target == null)
            {
                foreach (ArcadeForkliftController2D candidate in
                         FindObjectsByType<ArcadeForkliftController2D>(FindObjectsSortMode.None))
                {
                    if (candidate != null && candidate.PlayerControlled)
                    {
                        target = candidate;
                        break;
                    }
                }
            }

            if (target != null)
            {
                target.Impacted += OnImpact;
                SnapToTarget();
            }
        }

        private void OnDisable()
        {
            if (target != null)
            {
                target.Impacted -= OnImpact;
            }
        }

        private void OnImpact(float force)
        {
            // Acumula em vez de substituir: batidas em sequência somam, que é o
            // que faz uma zebra de barreiras parecer uma zebra de barreiras.
            shake = Mathf.Min(shake + shakePerImpact * Mathf.Clamp01(force), 0.75f);
        }

        private void LateUpdate()
        {
            if (target == null || cam == null)
            {
                return;
            }

            float speed01 = Mathf.Clamp01(target.NormalizedSpeed);

            // A câmera se adianta com a velocidade: em baixa fica centrada para
            // manobrar, em alta abre espaço à frente para o piloto ler a curva.
            Vector3 ahead = target.transform.up * (lookAhead * speed01);
            Vector3 desired = target.transform.position + ahead;
            desired.z = transform.position.z;

            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref velocity, positionSmoothing);

            if (rotateWithVehicle)
            {
                // A frente do veículo aponta para o topo da tela: a pista gira,
                // não o veículo. É o que dá a leitura de câmera traseira.
                float targetAngle = target.transform.eulerAngles.z;
                float current = transform.eulerAngles.z;
                float next = Mathf.LerpAngle(current, targetAngle, 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime));
                transform.rotation = Quaternion.Euler(0f, 0f, next);
            }

            // Afastar com a velocidade amplia a sensação de aceleração sem
            // mexer na física: mais pista entra em campo quanto mais rápido.
            float targetSize = Mathf.Lerp(restSize, topSpeedSize, speed01);
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize, targetSize, 1f - Mathf.Exp(-zoomSpeed * Time.deltaTime));

            ApplyShake();
        }

        private void ApplyShake()
        {
            if (shake <= 0.001f)
            {
                return;
            }

            float t = Time.time * 26f;
            Vector3 offset = new(
                (Mathf.PerlinNoise(seed, t) - 0.5f) * 2f,
                (Mathf.PerlinNoise(seed + 31f, t) - 0.5f) * 2f,
                0f);

            transform.position += offset * shake;
            shake = Mathf.Max(0f, shake - shakeDecay * Time.deltaTime * shake);
        }

        private void SnapToTarget()
        {
            Vector3 position = target.transform.position;
            position.z = transform.position.z;
            transform.position = position;

            if (rotateWithVehicle)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, target.transform.eulerAngles.z);
            }
        }
    }
}
