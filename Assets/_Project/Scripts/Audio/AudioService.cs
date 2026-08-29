using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TW08.Audio
{
    [DefaultExecutionOrder(-800)]
    [DisallowMultipleComponent]
    public sealed class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [SerializeField, Min(1)] private int initialPoolSize = 12;
        [SerializeField] private bool persistAcrossScenes = true;

        private readonly Queue<AudioSource> available = new();
        private readonly Dictionary<string, AudioSource> loops = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
            for (int i = 0; i < initialPoolSize; i++) available.Enqueue(CreateSource());
        }

        public void PlayOneShot(AudioEvent audioEvent, Vector3 position = default)
        {
            if (audioEvent == null) return;
            AudioClip clip = audioEvent.GetRandomClip();
            if (clip == null) return;
            AudioSource source = GetSource();
            Configure(source, audioEvent, clip, position);
            source.loop = false;
            source.Play();
            StartCoroutine(ReturnAfter(source, clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch))));
        }

        /// <summary>
        /// Toca depois de um atraso.
        ///
        /// Sensor e porta disparavam no mesmo frame e o ouvido lia os dois como
        /// um evento só; separá-los deixa a relação de causa audível.
        /// </summary>
        public void PlayOneShotDelayed(AudioEvent audioEvent, float delaySeconds, Vector3 position = default)
        {
            if (audioEvent == null) return;
            if (delaySeconds <= 0f)
            {
                PlayOneShot(audioEvent, position);
                return;
            }

            StartCoroutine(PlayAfter(audioEvent, delaySeconds, position));
        }

        private System.Collections.IEnumerator PlayAfter(AudioEvent audioEvent, float delay, Vector3 position)
        {
            yield return new WaitForSeconds(delay);
            PlayOneShot(audioEvent, position);
        }

        public void StartLoop(AudioEvent audioEvent, Vector3 position = default)
        {
            if (audioEvent == null || loops.ContainsKey(audioEvent.EventId)) return;
            AudioClip clip = audioEvent.GetRandomClip();
            if (clip == null) return;
            AudioSource source = GetSource();
            Configure(source, audioEvent, clip, position);
            source.loop = true;
            source.Play();
            loops.Add(audioEvent.EventId, source);
        }

        public void StopLoop(string eventId)
        {
            if (!loops.TryGetValue(eventId, out AudioSource source)) return;
            loops.Remove(eventId);
            source.Stop();
            Return(source);
        }

        private AudioSource CreateSource()
        {
            GameObject sourceObject = new("Pooled Audio Source");
            sourceObject.transform.SetParent(transform);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sourceObject.SetActive(false);
            return source;
        }

        private AudioSource GetSource()
        {
            AudioSource source = available.Count > 0 ? available.Dequeue() : CreateSource();
            source.gameObject.SetActive(true);
            return source;
        }

        private static void Configure(AudioSource source, AudioEvent audioEvent, AudioClip clip, Vector3 position)
        {
            source.transform.position = position;
            source.clip = clip;
            source.outputAudioMixerGroup = audioEvent.MixerGroup;
            float sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("tw08.audio.sfx", 1f));
            source.volume = Random.Range(audioEvent.VolumeRange.x, audioEvent.VolumeRange.y) * sfxVolume;
            source.pitch = Random.Range(audioEvent.PitchRange.x, audioEvent.PitchRange.y);
            source.spatialBlend = audioEvent.SpatialBlend;
        }

        private IEnumerator ReturnAfter(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, delay));
            Return(source);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Return(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.gameObject.SetActive(false);
            available.Enqueue(source);
        }
    }
}
