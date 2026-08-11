using TW08.Race;
using UnityEngine;

namespace TW08.Presentation
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public sealed class TW08CameraRig2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Rigidbody2D targetBody;
        [SerializeField] private ArcadeForkliftController2D targetVehicle;
        [SerializeField] private TW08GraphicsProfile profile;
        [SerializeField] private bool followZFromCurrent = true;

        private Camera cameraComponent;
        private Vector2 positionVelocity;
        private float zoomVelocity;
        private float fixedZ;
        private float shakeAmplitude;
        private float shakeTimeRemaining;
        private float shakeElapsed;
        private TW08GraphicsDirector graphicsDirector;

        public void Configure(
            Transform followTarget,
            Rigidbody2D body,
            ArcadeForkliftController2D vehicle,
            TW08GraphicsProfile graphicsProfile)
        {
            target = followTarget;
            targetBody = body;
            targetVehicle = vehicle;
            profile = graphicsProfile;
            CacheDependencies();
            SnapImmediately();
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            CacheDependencies();
            fixedZ = transform.position.z;
        }

        private void OnEnable()
        {
            ResolveGraphicsDirector();
        }

        private void OnDisable()
        {
            if (graphicsDirector != null)
            {
                graphicsDirector.CameraShakeRequested -= AddShake;
            }
            graphicsDirector = null;
        }

        private void LateUpdate()
        {
            if (target == null || cameraComponent == null)
            {
                return;
            }

            TW08GraphicsProfile activeProfile = profile != null
                ? profile
                : graphicsDirector != null ? graphicsDirector.Profile : null;

            float smoothTime = activeProfile != null ? activeProfile.CameraSmoothTime : 0.10f;
            float lookAheadTime = activeProfile != null ? activeProfile.LookAheadTime : 0.14f;
            float maxLookAhead = activeProfile != null ? activeProfile.MaximumLookAhead : 1.7f;

            Vector2 velocity = targetBody != null ? targetBody.linearVelocity : Vector2.zero;
            Vector2 lookAhead = Vector2.ClampMagnitude(velocity * lookAheadTime, maxLookAhead);
            Vector2 desired = (Vector2)target.position + lookAhead;
            Vector2 current = transform.position;
            Vector2 next = Vector2.SmoothDamp(current, desired, ref positionVelocity, smoothTime);

            if (activeProfile != null && activeProfile.PixelSnap)
            {
                next = SnapToPixel(next, activeProfile.PixelsPerUnit);
            }

            Vector2 shake = EvaluateShake(activeProfile);
            float z = followZFromCurrent ? fixedZ : transform.position.z;
            transform.position = new Vector3(next.x + shake.x, next.y + shake.y, z);

            float baseSize = activeProfile != null ? activeProfile.BaseOrthographicSize : cameraComponent.orthographicSize;
            float speedZoom = activeProfile != null ? activeProfile.MaximumSpeedZoomOut : 0f;
            float normalizedSpeed = targetVehicle != null ? targetVehicle.NormalizedSpeed : 0f;
            float desiredSize = baseSize + speedZoom * Mathf.SmoothStep(0f, 1f, normalizedSpeed);
            float zoomSmooth = activeProfile != null ? activeProfile.ZoomSmoothTime : 0.18f;
            cameraComponent.orthographicSize = Mathf.SmoothDamp(
                cameraComponent.orthographicSize,
                desiredSize,
                ref zoomVelocity,
                zoomSmooth);
        }

        public void AddShake(float amplitude, float duration)
        {
            shakeAmplitude = Mathf.Max(shakeAmplitude, amplitude);
            shakeTimeRemaining = Mathf.Max(shakeTimeRemaining, duration);
            shakeElapsed = 0f;
        }

        public void SnapImmediately()
        {
            CacheDependencies();
            if (target == null)
            {
                return;
            }

            fixedZ = transform.position.z;
            Vector2 position = target.position;
            if (profile != null && profile.PixelSnap)
            {
                position = SnapToPixel(position, profile.PixelsPerUnit);
            }
            transform.position = new Vector3(position.x, position.y, fixedZ);

            if (cameraComponent != null && profile != null)
            {
                cameraComponent.orthographicSize = profile.BaseOrthographicSize;
            }
        }

        private void CacheDependencies()
        {
            if (cameraComponent == null)
            {
                cameraComponent = GetComponent<Camera>();
            }
            if (targetBody == null && target != null)
            {
                targetBody = target.GetComponent<Rigidbody2D>();
            }
            if (targetVehicle == null && target != null)
            {
                targetVehicle = target.GetComponent<ArcadeForkliftController2D>();
            }
        }

        private void ResolveGraphicsDirector()
        {
            TW08GraphicsDirector candidate = TW08GraphicsDirector.Instance;
            if (candidate == null)
            {
                candidate = FindFirstObjectByType<TW08GraphicsDirector>();
            }

            if (graphicsDirector == candidate)
            {
                return;
            }

            if (graphicsDirector != null)
            {
                graphicsDirector.CameraShakeRequested -= AddShake;
            }

            graphicsDirector = candidate;
            if (graphicsDirector != null)
            {
                graphicsDirector.CameraShakeRequested += AddShake;
                if (profile == null)
                {
                    profile = graphicsDirector.Profile;
                }
            }
        }

        private Vector2 EvaluateShake(TW08GraphicsProfile activeProfile)
        {
            if (shakeTimeRemaining <= 0f || shakeAmplitude <= 0f)
            {
                return Vector2.zero;
            }

            shakeTimeRemaining = Mathf.Max(0f, shakeTimeRemaining - Time.unscaledDeltaTime);
            shakeElapsed += Time.unscaledDeltaTime;
            float frequency = activeProfile != null ? activeProfile.ShakeFrequency : 28f;
            float decay = shakeTimeRemaining <= 0f ? 0f : Mathf.Clamp01(shakeTimeRemaining / Mathf.Max(0.001f, shakeTimeRemaining + shakeElapsed));
            float x = Mathf.PerlinNoise(17.31f, shakeElapsed * frequency) * 2f - 1f;
            float y = Mathf.PerlinNoise(42.73f, shakeElapsed * frequency) * 2f - 1f;
            Vector2 offset = new Vector2(x, y) * (shakeAmplitude * decay);

            if (shakeTimeRemaining <= 0f)
            {
                shakeAmplitude = 0f;
                shakeElapsed = 0f;
            }

            return offset;
        }

        private static Vector2 SnapToPixel(Vector2 position, float pixelsPerUnit)
        {
            float step = 1f / Mathf.Max(1f, pixelsPerUnit);
            return new Vector2(
                Mathf.Round(position.x / step) * step,
                Mathf.Round(position.y / step) * step);
        }
    }
}
