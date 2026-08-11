using System;
using UnityEngine;

namespace TW08.Presentation
{
    [DefaultExecutionOrder(-850)]
    [DisallowMultipleComponent]
    public sealed class TW08GraphicsDirector : MonoBehaviour
    {
        [SerializeField] private TW08GraphicsProfile profile;
        [SerializeField] private bool persistAcrossScenes = true;

        public static TW08GraphicsDirector Instance { get; private set; }
        public TW08GraphicsProfile Profile => profile;

        public event Action<TW08GraphicsProfile> ProfileApplied;
        public event Action<float, float> CameraShakeRequested;

        public void Configure(TW08GraphicsProfile graphicsProfile, bool persist = true)
        {
            profile = graphicsProfile;
            persistAcrossScenes = persist;
            if (Application.isPlaying)
            {
                ApplyProfile();
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            ApplyProfile();
        }

        public void ApplyProfile()
        {
            if (profile == null)
            {
                return;
            }

            QualitySettings.vSyncCount = profile.VSyncCount;
            QualitySettings.antiAliasing = profile.AntiAliasing;
            Application.targetFrameRate = profile.TargetFrameRate;
            ProfileApplied?.Invoke(profile);
        }

        public void RequestCameraShake(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f)
            {
                return;
            }

            CameraShakeRequested?.Invoke(amplitude, duration);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
