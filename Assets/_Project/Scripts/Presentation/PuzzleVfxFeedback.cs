using TW08.Puzzle;
using UnityEngine;

namespace TW08.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PuzzleVfxFeedback : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private Color pushColor = new(1f, 0.58f, 0.12f, 1f);
        [SerializeField] private Color successColor = new(0.25f, 1f, 0.55f, 1f);
        [SerializeField] private Color errorColor = new(1f, 0.22f, 0.18f, 1f);

        public void Configure(PuzzleRuntime puzzleRuntime)
        {
            runtime = puzzleRuntime;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (particles == null)
            {
                particles = CreateParticleSystem();
            }
        }

        private void OnEnable()
        {
            if (runtime == null) return;
            runtime.MoveApplied += OnMove;
            runtime.LevelCompleted += OnCompleted;
            runtime.StaticDeadlockDetected += OnDeadlock;
        }

        private void OnDisable()
        {
            if (runtime == null) return;
            runtime.MoveApplied -= OnMove;
            runtime.LevelCompleted -= OnCompleted;
            runtime.StaticDeadlockDetected -= OnDeadlock;
        }

        private void OnMove(PuzzleMove move)
        {
            if (!move.CrateMoved || runtime?.Level == null) return;
            Emit(move.CrateTo.ToWorld(runtime.Level.CellSize), pushColor, 7, 0.7f);
        }

        private void OnCompleted()
        {
            Vector3 position = runtime?.Board != null && runtime.Level != null
                ? runtime.Board.PlayerPosition.ToWorld(runtime.Level.CellSize)
                : transform.position;
            Emit(position, successColor, 28, 1.35f);
        }

        private void OnDeadlock()
        {
            Vector3 position = runtime?.Board != null && runtime.Level != null
                ? runtime.Board.PlayerPosition.ToWorld(runtime.Level.CellSize)
                : transform.position;
            Emit(position, errorColor, 14, 0.9f);
        }

        private void Emit(Vector3 worldPosition, Color color, int count, float speed)
        {
            if (particles == null) return;
            particles.transform.position = worldPosition;
            ParticleSystem.MainModule main = particles.main;
            main.startColor = color;
            main.startSpeed = speed;
            particles.Emit(count);
        }

        private ParticleSystem CreateParticleSystem()
        {
            GameObject go = new("Puzzle VFX Particles");
            go.transform.SetParent(transform, false);
            ParticleSystem system = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.4f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
            main.startSpeed = 0.8f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 80;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.18f;
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 50;
            return system;
        }
    }
}
