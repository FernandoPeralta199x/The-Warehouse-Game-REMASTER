using UnityEngine;

namespace TW08.Presentation
{
    [CreateAssetMenu(fileName = "TW08GraphicsProfile", menuName = "TW08/Presentation/Graphics Profile")]
    public sealed class TW08GraphicsProfile : ScriptableObject
    {
        [Header("Frame pacing")]
        [SerializeField, Min(30)] private int targetFrameRate = 60;
        [SerializeField, Range(0, 4)] private int vSyncCount;
        [SerializeField] private int antiAliasing;

        [Header("Pixel presentation")]
        [SerializeField] private bool pixelSnap = true;
        [SerializeField, Min(1f)] private float pixelsPerUnit = 32f;

        [Header("Camera")]
        [SerializeField, Min(0.01f)] private float cameraSmoothTime = 0.10f;
        [SerializeField, Min(0f)] private float lookAheadTime = 0.14f;
        [SerializeField, Min(0f)] private float maximumLookAhead = 1.7f;
        [SerializeField, Min(0.1f)] private float baseOrthographicSize = 6.65f;
        [SerializeField, Min(0f)] private float maximumSpeedZoomOut = 0.9f;
        [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.18f;

        [Header("Feedback")]
        [SerializeField, Min(0f)] private float defaultImpactShake = 0.10f;
        [SerializeField, Min(0.01f)] private float shakeFrequency = 28f;

        public int TargetFrameRate => Mathf.Max(30, targetFrameRate);
        public int VSyncCount => Mathf.Clamp(vSyncCount, 0, 4);
        public int AntiAliasing => antiAliasing;
        public bool PixelSnap => pixelSnap;
        public float PixelsPerUnit => Mathf.Max(1f, pixelsPerUnit);
        public float CameraSmoothTime => Mathf.Max(0.01f, cameraSmoothTime);
        public float LookAheadTime => Mathf.Max(0f, lookAheadTime);
        public float MaximumLookAhead => Mathf.Max(0f, maximumLookAhead);
        public float BaseOrthographicSize => Mathf.Max(0.1f, baseOrthographicSize);
        public float MaximumSpeedZoomOut => Mathf.Max(0f, maximumSpeedZoomOut);
        public float ZoomSmoothTime => Mathf.Max(0.01f, zoomSmoothTime);
        public float DefaultImpactShake => Mathf.Max(0f, defaultImpactShake);
        public float ShakeFrequency => Mathf.Max(0.01f, shakeFrequency);

        private void OnValidate()
        {
            targetFrameRate = Mathf.Max(30, targetFrameRate);
            vSyncCount = Mathf.Clamp(vSyncCount, 0, 4);
            antiAliasing = NormalizeAntiAliasing(antiAliasing);
            pixelsPerUnit = Mathf.Max(1f, pixelsPerUnit);
            cameraSmoothTime = Mathf.Max(0.01f, cameraSmoothTime);
            lookAheadTime = Mathf.Max(0f, lookAheadTime);
            maximumLookAhead = Mathf.Max(0f, maximumLookAhead);
            baseOrthographicSize = Mathf.Max(0.1f, baseOrthographicSize);
            maximumSpeedZoomOut = Mathf.Max(0f, maximumSpeedZoomOut);
            zoomSmoothTime = Mathf.Max(0.01f, zoomSmoothTime);
            defaultImpactShake = Mathf.Max(0f, defaultImpactShake);
            shakeFrequency = Mathf.Max(0.01f, shakeFrequency);
        }

        private static int NormalizeAntiAliasing(int value)
        {
            return value switch
            {
                2 => 2,
                4 => 4,
                8 => 8,
                _ => 0
            };
        }
    }
}
