using System;
using System.Collections;
using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class RaceCountdown : MonoBehaviour
    {
        public event Action<int> Tick;
        public event Action Completed;

        public void Begin(float seconds)
        {
            StopAllCoroutines();
            StartCoroutine(Run(seconds));
        }

        private IEnumerator Run(float seconds)
        {
            int previous = int.MinValue;
            float remaining = Mathf.Max(0f, seconds);
            while (remaining > 0f)
            {
                int value = Mathf.CeilToInt(remaining);
                if (value != previous)
                {
                    previous = value;
                    Tick?.Invoke(value);
                }

                remaining -= Time.unscaledDeltaTime;
                yield return null;
            }

            Tick?.Invoke(0);
            Completed?.Invoke();
        }
    }
}
