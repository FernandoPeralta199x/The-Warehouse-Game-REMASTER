using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW08.Presentation
{
    [CreateAssetMenu(fileName = "DirectionalSpriteSet", menuName = "TW08/Art/Directional Sprite Set")]
    public sealed class DirectionalSpriteSet : ScriptableObject
    {
        [Header("Idle")]
        [SerializeField] private Sprite idleDown;
        [SerializeField] private Sprite idleUp;
        [SerializeField] private Sprite idleLeft;
        [SerializeField] private Sprite idleRight;

        [Header("Walk")]
        [SerializeField] private Sprite[] walkDown = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] walkUp = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] walkLeft = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] walkRight = Array.Empty<Sprite>();

        [Header("Playback")]
        [SerializeField, Min(1f)] private float framesPerSecond = 8f;

        public float FramesPerSecond => Mathf.Max(1f, framesPerSecond);
        public bool IsGameplayReady =>
            idleDown != null &&
            idleUp != null &&
            idleLeft != null &&
            idleRight != null &&
            HasFrame(walkDown) &&
            HasFrame(walkUp) &&
            HasFrame(walkLeft) &&
            HasFrame(walkRight);

        public Sprite GetIdle(FacingDirection direction)
        {
            switch (direction)
            {
                case FacingDirection.Up:
                    return idleUp;
                case FacingDirection.Left:
                    return idleLeft;
                case FacingDirection.Right:
                    return idleRight;
                default:
                    return idleDown;
            }
        }

        public IReadOnlyList<Sprite> GetWalk(FacingDirection direction)
        {
            switch (direction)
            {
                case FacingDirection.Up:
                    return walkUp ?? Array.Empty<Sprite>();
                case FacingDirection.Left:
                    return walkLeft ?? Array.Empty<Sprite>();
                case FacingDirection.Right:
                    return walkRight ?? Array.Empty<Sprite>();
                default:
                    return walkDown ?? Array.Empty<Sprite>();
            }
        }

        private static bool HasFrame(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnValidate()
        {
            framesPerSecond = Mathf.Max(1f, framesPerSecond);
        }
    }
}
