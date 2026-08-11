using TW08.Race;
using UnityEngine;

namespace TW08.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ForkliftVfxController : MonoBehaviour
    {
        [SerializeField] private ArcadeForkliftController2D vehicle;
        [SerializeField] private RaceSessionController session;
        [SerializeField] private ParticleSystem trailParticles;
        [SerializeField] private Color normalColor = new(0.55f, 0.58f, 0.56f, 0.65f);
        [SerializeField] private Color driftColor = new(1f, 0.62f, 0.14f, 0.9f);
        [SerializeField] private Color finishColor = new(0.25f, 1f, 0.55f, 1f);

        public void Configure(ArcadeForkliftController2D controller, RaceSessionController raceSession)
        {
            vehicle = controller;
            session = raceSession;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (trailParticles == null) trailParticles = CreateParticles();
        }

        private void OnEnable()
        {
            if (session != null) session.PlayerFinished += OnFinished;
        }

        private void OnDisable()
        {
            if (session != null) session.PlayerFinished -= OnFinished;
        }

        private void Update()
        {
            if (vehicle == null || trailParticles == null) return;
            ParticleSystem.EmissionModule emission = trailParticles.emission;
            float speed = vehicle.NormalizedSpeed;
            emission.rateOverTime = vehicle.IsDrifting ? Mathf.Lerp(10f, 28f, speed) : Mathf.Lerp(0f, 7f, speed);
            ParticleSystem.MainModule main = trailParticles.main;
            main.startColor = vehicle.IsDrifting ? driftColor : normalColor;
        }

        private void OnFinished(float _, int __)
        {
            if (trailParticles == null) return;
            ParticleSystem.MainModule main = trailParticles.main;
            main.startColor = finishColor;
            main.startSpeed = 2f;
            trailParticles.Emit(40);
        }

        private ParticleSystem CreateParticles()
        {
            GameObject go = new("Forklift Trail VFX");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            ParticleSystem system = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.playOnAwake = true;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.48f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.55f, 0.12f, 0.01f);
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 25;
            return system;
        }
    }
}
