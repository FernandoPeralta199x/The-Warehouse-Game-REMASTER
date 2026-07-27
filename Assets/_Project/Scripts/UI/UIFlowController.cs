using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class UIFlowController : MonoBehaviour
    {
        [SerializeField] private List<UIScreen> screens = new();

        private readonly Stack<UIScreen> history = new();
        private Dictionary<string, UIScreen> byId;

        public UIScreen Current { get; private set; }
        public event Action<UIScreen> ScreenChanged;

        private void Awake()
        {
            byId = screens
                .Where(screen => screen != null && !string.IsNullOrWhiteSpace(screen.ScreenId))
                .GroupBy(screen => screen.ScreenId)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        }

        public bool Show(string screenId, bool rememberCurrent = true)
        {
            if (byId == null || !byId.TryGetValue(screenId, out UIScreen next))
            {
                Debug.LogWarning($"Unknown UI screen '{screenId}'.", this);
                return false;
            }

            if (Current == next)
            {
                return true;
            }

            if (rememberCurrent && Current != null)
            {
                history.Push(Current);
            }

            Current?.SetVisible(false);
            Current = next;
            Current.SetVisible(true);
            ScreenChanged?.Invoke(Current);
            return true;
        }

        public bool Back()
        {
            if (history.Count == 0)
            {
                return false;
            }

            UIScreen previous = history.Pop();
            Current?.SetVisible(false);
            Current = previous;
            Current.SetVisible(true);
            ScreenChanged?.Invoke(Current);
            return true;
        }

        public void ClearHistory()
        {
            history.Clear();
        }
    }
}
