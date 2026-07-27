using System;
using TW08.Core.Services;
using UnityEngine;

namespace TW08.Core
{
    [DisallowMultipleComponent]
    public sealed class PauseService : MonoBehaviour, IGameService
    {
        private float previousTimeScale = 1f;

        public bool IsPaused { get; private set; }
        public event Action<bool> PauseChanged;

        public void Initialize(ServiceRegistry services)
        {
            IsPaused = false;
        }

        public void Shutdown()
        {
            SetPaused(false);
        }

        public void Toggle()
        {
            SetPaused(!IsPaused);
        }

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
            {
                return;
            }

            IsPaused = paused;
            if (paused)
            {
                previousTimeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = previousTimeScale;
            }

            PauseChanged?.Invoke(paused);
        }
    }
}
