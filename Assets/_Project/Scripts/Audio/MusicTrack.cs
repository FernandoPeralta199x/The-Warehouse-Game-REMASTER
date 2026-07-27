using UnityEngine;
using UnityEngine.Audio;

namespace TW08.Audio
{
    [CreateAssetMenu(fileName = "MusicTrack", menuName = "TW08/Audio/Music Track")]
    public sealed class MusicTrack : ScriptableObject
    {
        [SerializeField] private string trackId = "music-track";
        [SerializeField] private AudioClip clip;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField, Range(0f, 1f)] private float volume = 0.75f;
        [SerializeField] private bool loop = true;

        public string TrackId => trackId;
        public AudioClip Clip => clip;
        public AudioMixerGroup MixerGroup => mixerGroup;
        public float Volume => volume;
        public bool Loop => loop;
    }
}
