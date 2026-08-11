using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class RaceRouteScanner : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private RacerProgress progress;
        [SerializeField] private Color routeColor = new(0.25f, 0.95f, 0.58f, 0.88f);
        [SerializeField, Min(0.01f)] private float lineWidth = 0.075f;

        private LineRenderer line;
        private Material runtimeMaterial;
        private float visibleUntil;

        public bool Visible => Time.time < visibleUntil;

        public void Configure(RaceManager manager, RacerProgress racerProgress)
        {
            raceManager = manager;
            progress = racerProgress;
            EnsureRenderer();
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (progress == null) progress = GetComponent<RacerProgress>();
            EnsureRenderer();
            SetVisible(false);
        }

        private void Update()
        {
            bool visible = Visible && raceManager != null && progress != null && !progress.Finished;
            SetVisible(visible);
            if (!visible || !raceManager.TryGetCheckpointPosition(progress.NextCheckpointIndex, out Vector2 target))
            {
                return;
            }

            line.SetPosition(0, transform.position);
            line.SetPosition(1, new Vector3(target.x, target.y, transform.position.z));
        }

        public void Reveal(float duration)
        {
            visibleUntil = Mathf.Max(visibleUntil, Time.time + Mathf.Max(0.1f, duration));
            SetVisible(true);
        }

        private void EnsureRenderer()
        {
            if (line != null)
            {
                return;
            }

            line = GetComponent<LineRenderer>();
            if (line == null)
            {
                line = gameObject.AddComponent<LineRenderer>();
            }

            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth * 0.65f;
            line.startColor = routeColor;
            line.endColor = new Color(routeColor.r, routeColor.g, routeColor.b, 0.08f);
            line.sortingOrder = 50;

            if (line.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
                if (shader != null)
                {
                    runtimeMaterial = new Material(shader)
                    {
                        name = "TW08 Route Scanner Runtime"
                    };
                    line.sharedMaterial = runtimeMaterial;
                }
            }
        }

        private void SetVisible(bool value)
        {
            if (line != null)
            {
                line.enabled = value;
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }
    }
}
