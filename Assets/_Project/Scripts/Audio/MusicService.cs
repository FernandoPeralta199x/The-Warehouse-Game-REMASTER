using System.Collections;
using TW08.Core.Services;
using UnityEngine;

namespace TW08.Audio
{
    [DisallowMultipleComponent]
    public sealed class MusicService : MonoBehaviour, IGameService
    {
        [SerializeField, Min(0.01f)] private float defaultFadeSeconds = 0.75f;

        private AudioSource sourceA;
        private AudioSource sourceB;
        private AudioSource active;
        private Coroutine transition;

        public string CurrentTrackId { get; private set; }

        public void Initialize(ServiceRegistry services)
        {
            sourceA = CreateSource("Music A");
            sourceB = CreateSource("Music B");
            active = sourceA;
        }

        public void Shutdown()
        {
            if (transition != null)
            {
                StopCoroutine(transition);
            }

            sourceA?.Stop();
            sourceB?.Stop();
        }

        public void Play(MusicTrack track, float fadeSeconds = -1f)
        {
            if (track == null || track.Clip == null || track.TrackId == CurrentTrackId)
            {
                return;
            }

            if (transition != null)
            {
                StopCoroutine(transition);
            }

            transition = StartCoroutine(Crossfade(track, fadeSeconds < 0f ? defaultFadeSeconds : fadeSeconds));
        }

        public void Stop(float fadeSeconds = -1f)
        {
            if (transition != null)
            {
                StopCoroutine(transition);
            }

            transition = StartCoroutine(FadeOut(fadeSeconds < 0f ? defaultFadeSeconds : fadeSeconds));
        }

        private AudioSource CreateSource(string sourceName)
        {
            GameObject child = new(sourceName);
            child.transform.SetParent(transform, false);
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        private IEnumerator Crossfade(MusicTrack track, float seconds)
        {
            AudioSource next = active == sourceA ? sourceB : sourceA;
            next.clip = track.Clip;
            next.outputAudioMixerGroup = track.MixerGroup;
            next.loop = track.Loop;
            next.volume = 0f;
            next.Play();

            float startVolume = active.volume;
            float duration = Mathf.Max(0.01f, seconds);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                active.volume = Mathf.Lerp(startVolume, 0f, t);
                next.volume = Mathf.Lerp(0f, track.Volume, t);
                yield return null;
            }

            active.Stop();
            active.clip = null;
            active.volume = 0f;
            active = next;
            CurrentTrackId = track.TrackId;
            transition = null;
        }

        private IEnumerator FadeOut(float seconds)
        {
            float startVolume = active == null ? 0f : active.volume;
            float duration = Mathf.Max(0.01f, seconds);
            float elapsed = 0f;

            while (active != null && elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                active.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            active?.Stop();
            CurrentTrackId = null;
            transition = null;
        }
    }
}
