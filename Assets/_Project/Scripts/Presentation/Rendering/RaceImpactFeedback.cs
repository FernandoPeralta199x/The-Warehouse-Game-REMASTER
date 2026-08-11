using TW08.Race;
using UnityEngine;

namespace TW08.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RaceImpactFeedback : MonoBehaviour
    {
        [SerializeField] private ArcadeForkliftController2D controller;
        [SerializeField, Min(0f)] private float minimumImpact = 2.5f;
        [SerializeField, Min(0f)] private float maximumShake = 0.22f;
        [SerializeField, Min(0.01f)] private float shakeDuration = 0.16f;

        public void Configure(ArcadeForkliftController2D forklift)
        {
            controller = forklift;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<ArcadeForkliftController2D>();
            }
        }

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.Impacted += OnImpact;
            }
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.Impacted -= OnImpact;
            }
        }

        private void OnImpact(float impact)
        {
            if (impact < minimumImpact)
            {
                return;
            }

            float amplitude = Mathf.Lerp(0.04f, maximumShake, Mathf.InverseLerp(minimumImpact, 12f, impact));
            TW08GraphicsDirector director = TW08GraphicsDirector.Instance;
            if (director != null)
            {
                director.RequestCameraShake(amplitude, shakeDuration);
            }
        }
    }
}
