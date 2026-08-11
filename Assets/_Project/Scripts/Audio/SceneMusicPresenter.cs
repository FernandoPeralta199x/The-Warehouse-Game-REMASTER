using TW08.Core;
using UnityEngine;

namespace TW08.Audio
{
    [DisallowMultipleComponent]
    public sealed class SceneMusicPresenter : MonoBehaviour
    {
        [SerializeField] private MusicTrack track;
        [SerializeField, Min(0f)] private float fadeSeconds = 0.65f;

        public void Configure(MusicTrack musicTrack, float fade = 0.65f)
        {
            track = musicTrack;
            fadeSeconds = Mathf.Max(0f, fade);
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Start()
        {
            if (track == null) return;
            MusicService service = null;
            if (GameBootstrap.Instance != null)
            {
                try { service = GameBootstrap.Instance.GetService<MusicService>(); }
                catch { service = null; }
            }
            service ??= Object.FindFirstObjectByType<MusicService>();
            service?.Play(track, fadeSeconds);
        }
    }
}
