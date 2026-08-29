using TW08.Motion;
using TW08.Puzzle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW08.Narrative
{
    /// <summary>
    /// Decide o que a cena tem a dizer.
    ///
    /// Lê o setor e a fase do <see cref="PuzzleRuntime"/> presente na cena, consulta
    /// o catálogo e enfileira o que ainda não foi visto. Executa antes do runtime
    /// (ordem -50) para já estar inscrito na conclusão da fase quando ela acontecer.
    ///
    /// Se instala sozinho via <see cref="NarrativeBootstrap"/>: os construtores de
    /// cena de puzzle são de outro sistema e não referenciam narrativa.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class NarrativeDirector : MonoBehaviour
    {
        [SerializeField] private NarrativeCatalog catalog;
        [SerializeField] private NarrativeService service;
        [SerializeField] private NarrativeOverlayController overlay;

        [Header("Contexto")]
        [Tooltip("Usado quando a cena não tem PuzzleRuntime (menu, oficina, corrida).")]
        [SerializeField] private string fallbackSectorId = string.Empty;
        [SerializeField] private string fallbackLevelId = string.Empty;

        [Header("Momentos")]
        [SerializeField] private bool playOnEntry = true;
        [SerializeField] private bool playOnLevelCompleted = true;
        [Tooltip("Espera antes da cutscene de conclusão, para a carga terminar de deslizar.")]
        [SerializeField, Min(0f)] private float completionDelaySeconds = 0.65f;

        private PuzzleRuntime runtime;
        private bool subscribed;

        public NarrativeService Service => service;
        public NarrativeOverlayController Overlay => overlay;

        /// <summary>Ponto de entrada para quem monta a cena por código.</summary>
        public void Configure(NarrativeCatalog narrativeCatalog, string sectorId, string levelId)
        {
            catalog = narrativeCatalog;
            fallbackSectorId = sectorId ?? string.Empty;
            fallbackLevelId = levelId ?? string.Empty;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                if (gameObject.scene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
            }
#endif
        }

        private void Awake()
        {
            EnsureDependencies();
        }

        private void Start()
        {
            if (catalog == null || service == null)
            {
                return;
            }

            Subscribe();

            if (playOnEntry)
            {
                QueueMatch(NarrativeTriggerKind.Opening);
                QueueMatch(NarrativeTriggerKind.SectorEntry);
                QueueMatch(NarrativeTriggerKind.LevelStart);
                service.PlayQueued();
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void EnsureDependencies()
        {
            // Nunca use ??= com UnityEngine.Object: uma referência serializada
            // quebrada é não-nula para o C# e nula para o operador do Unity.
            if (catalog == null)
            {
                catalog = NarrativeCatalog.LoadDefault();
            }

            runtime = FindFirstObjectByType<PuzzleRuntime>();

            if (catalog == null)
            {
                // Sem catálogo não existe roteiro: não vale criar overlay nem serviço.
                enabled = false;
                return;
            }

            if (service == null)
            {
                service = FindFirstObjectByType<NarrativeService>();
            }

            if (service == null)
            {
                service = gameObject.AddComponent<NarrativeService>();
            }

            if (overlay == null)
            {
                overlay = FindFirstObjectByType<NarrativeOverlayController>();
            }

            if (overlay == null)
            {
                overlay = gameObject.AddComponent<NarrativeOverlayController>();
            }

            overlay.Configure(service, catalog.Roster);
        }

        private void Subscribe()
        {
            if (subscribed || runtime == null)
            {
                return;
            }

            runtime.LevelCompleted += OnLevelCompleted;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (runtime != null)
            {
                runtime.LevelCompleted -= OnLevelCompleted;
            }

            subscribed = false;
        }

        private void OnLevelCompleted()
        {
            if (!playOnLevelCompleted || service == null)
            {
                return;
            }

            // A última carga ainda está deslizando quando o tabuleiro fecha. Entrar
            // na cutscene no mesmo frame congelaria a peça no meio do movimento,
            // porque o overlay zera o timeScale e a view do puzzle é escalada.
            if (completionDelaySeconds > 0f)
            {
                UIMotion.Chain().Wait(completionDelaySeconds).Then(QueueCompletion).Play();
                return;
            }

            QueueCompletion();
        }

        private void QueueCompletion()
        {
            // O encadeamento vive em um objeto persistente: a cena pode ter trocado.
            if (this == null || service == null)
            {
                return;
            }

            QueueMatch(NarrativeTriggerKind.LevelCompleted);
            QueueMatch(NarrativeTriggerKind.Ending);
            service.PlayQueued();
        }

        private void QueueMatch(NarrativeTriggerKind kind)
        {
            if (catalog == null || service == null)
            {
                return;
            }

            NarrativeContext context = new(kind, ResolveSectorId(), ResolveLevelId());
            NarrativeSequence sequence = catalog.Resolve(context, service.IsEligible);
            if (sequence != null)
            {
                service.Enqueue(sequence);
            }
        }

        private string ResolveSectorId()
        {
            return runtime != null && runtime.Level != null ? runtime.Level.SectorId : fallbackSectorId;
        }

        private string ResolveLevelId()
        {
            return runtime != null && runtime.Level != null ? runtime.Level.LevelId : fallbackLevelId;
        }
    }

    /// <summary>
    /// Auto-registro do diretor.
    ///
    /// As cenas de puzzle são geradas por construtores que não podem depender deste
    /// sistema, então a instalação acontece por evento de carga de cena. O catálogo
    /// tem a chave para desligar isso sem tocar em código.
    /// </summary>
    internal static class NarrativeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            InstallDirector();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InstallDirector();
        }

        private static void InstallDirector()
        {
            if (UnityEngine.Object.FindFirstObjectByType<NarrativeDirector>() != null)
            {
                return;
            }

            NarrativeCatalog catalog = NarrativeCatalog.LoadDefault();
            if (catalog == null || !catalog.AutoInstallInScenes)
            {
                return;
            }

            // Por enquanto a narrativa é escopo de puzzle: menus e corridas ficam de fora.
            if (UnityEngine.Object.FindFirstObjectByType<PuzzleRuntime>() == null)
            {
                return;
            }

            new GameObject("Narrative Director").AddComponent<NarrativeDirector>();
        }
    }
}
