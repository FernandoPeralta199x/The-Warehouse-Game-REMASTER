using UnityEngine;

namespace TW08.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] private string screenId = "screen";
        [SerializeField] private bool hideOnAwake = true;

        private CanvasGroup canvasGroup;

        public string ScreenId => screenId;
        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(!hideOnAwake);
        }

        public virtual void SetVisible(bool visible)
        {
            IsVisible = visible;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}
