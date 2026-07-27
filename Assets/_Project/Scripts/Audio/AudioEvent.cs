using UnityEngine;
using UnityEngine.Audio;

namespace TW08.Audio
{
    [CreateAssetMenu(fileName = "AudioEvent", menuName = "TW08/Audio/Audio Event")]
    public sealed class AudioEvent : ScriptableObject
    {
        [SerializeField] private string eventId = "audio-event";
        [SerializeField] private AudioClip[] clips;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField] private Vector2 volumeRange = new(0.9f, 1f);
        [SerializeField] private Vector2 pitchRange = new(0.97f, 1.03f);
        [SerializeField, Range(0f, 1f)] private float spatialBlend;
        [SerializeField] private bool loop;

        public string EventId => eventId;
        public AudioMixerGroup MixerGroup => mixerGroup;
        public Vector2 VolumeRange => volumeRange;
        public Vector2 PitchRange => pitchRange;
        public float SpatialBlend => spatialBlend;
        public bool Loop => loop;

        public AudioClip GetRandomClip()
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            return clips[Random.Range(0, clips.Length)];
        }
    }
}
