using UnityEngine;

namespace TW08
{
    /// <summary>
    /// Lightweight procedural renderer used only by generated prototype scenes.
    /// It intentionally lives in the runtime assembly so generated scenes remain
    /// valid in player builds. Final game art should replace this component.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PrototypeSpriteRenderer : MonoBehaviour
    {
        private const float MinimumSize = 0.001f;

        private static Sprite sharedSquareSprite;

        [SerializeField] private Color color = Color.white;
        [SerializeField] private Vector2 size = Vector2.one;
        [SerializeField] private int sortingOrder;
        [SerializeField] private SpriteRenderer targetRenderer;

        public void Configure(Color tint, Vector2 dimensions, int order)
        {
            color = tint;
            size = SanitizeSize(dimensions);
            sortingOrder = order;
            Apply();
        }

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Keep validation side-effect free. Unity forbids SpriteRenderer property
            // changes from OnValidate because they can trigger internal SendMessage calls.
            size = SanitizeSize(size);
        }
#endif

        private void Apply()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            if (targetRenderer == null)
            {
                return;
            }

            targetRenderer.sprite = GetOrCreateSquareSprite();
            targetRenderer.color = color;
            targetRenderer.sortingOrder = sortingOrder;
            transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private static Vector2 SanitizeSize(Vector2 dimensions)
        {
            return new Vector2(
                Mathf.Max(MinimumSize, Mathf.Abs(dimensions.x)),
                Mathf.Max(MinimumSize, Mathf.Abs(dimensions.y)));
        }

        private static Sprite GetOrCreateSquareSprite()
        {
            if (sharedSquareSprite != null)
            {
                return sharedSquareSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "TW08 Prototype Square Texture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);

            sharedSquareSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f,
                0,
                SpriteMeshType.FullRect);
            sharedSquareSprite.name = "TW08 Prototype Square Sprite";
            sharedSquareSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSquareSprite;
        }
    }
}
