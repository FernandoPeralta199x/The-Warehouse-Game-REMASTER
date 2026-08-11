using TW08.Race;
using UnityEngine;

namespace TW08.Audio
{
    [DisallowMultipleComponent]
    public sealed class RaceAudioFeedback : MonoBehaviour
    {
        [SerializeField] private RaceSessionController session;
        [SerializeField] private TW08AudioCatalog catalog;

        public void Configure(RaceSessionController raceSession, TW08AudioCatalog audioCatalog)
        {
            session = raceSession;
            catalog = audioCatalog;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable()
        {
            if (session == null) return;
            session.CountdownChanged += OnCountdown;
            session.PlayerFinished += OnFinished;
        }

        private void OnDisable()
        {
            if (session == null) return;
            session.CountdownChanged -= OnCountdown;
            session.PlayerFinished -= OnFinished;
        }

        private void OnCountdown(int value)
        {
            if (value > 0) AudioService.Instance?.PlayOneShot(catalog?.RaceCountdown, transform.position);
        }

        private void OnFinished(float _, int __)
        {
            AudioService.Instance?.PlayOneShot(catalog?.RaceFinish, transform.position);
        }
    }
}
