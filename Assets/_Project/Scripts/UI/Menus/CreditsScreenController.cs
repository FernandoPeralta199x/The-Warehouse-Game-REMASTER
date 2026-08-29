using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Menus
{
    /// <summary>
    /// Rolagem contínua dos créditos e saída animada.
    ///
    /// O texto sobe em loop dentro de uma viewport recortada; a posição de origem
    /// é capturada no Awake para que a cena salva nunca guarde um quadro
    /// intermediário da rolagem.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CreditsScreenController : MonoBehaviour
    {
        [SerializeField] private RectTransform body;
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";
        [SerializeField, Min(1f)] private float scrollSpeed = 34f;
        [SerializeField, Min(1f)] private float loopDistance = 1080f;

        private Vector2 origin;
        private float offset;

        public void Configure(RectTransform bodyRect, Button back, string backSceneName, float loop)
        {
            body = bodyRect;
            backButton = back;
            backScene = string.IsNullOrWhiteSpace(backSceneName) ? "TW08_ModeSelect" : backSceneName;
            loopDistance = Mathf.Max(1f, loop);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        /// <summary>Posição da rolagem em loop. Regra pura, testável sem cena.</summary>
        public static float LoopOffset(float travelled, float loop)
        {
            return loop <= 0f ? 0f : Mathf.Repeat(Mathf.Max(0f, travelled), loop);
        }

        private void Awake()
        {
            if (body != null)
            {
                origin = body.anchoredPosition;
            }
        }

        private void OnEnable()
        {
            offset = 0f;
            backButton?.onClick.AddListener(Back);
        }

        private void OnDisable()
        {
            backButton?.onClick.RemoveListener(Back);
            if (body != null)
            {
                body.anchoredPosition = origin;
            }
        }

        private void Update()
        {
            if (body == null)
            {
                return;
            }

            offset = LoopOffset(offset + scrollSpeed * Time.unscaledDeltaTime, loopDistance);
            body.anchoredPosition = origin + new Vector2(0f, offset);
        }

        public void Back()
        {
            MenuFeedback.Click(backButton);
            MenuTransition.Go(backScene, "menu de modos");
        }
    }
}
