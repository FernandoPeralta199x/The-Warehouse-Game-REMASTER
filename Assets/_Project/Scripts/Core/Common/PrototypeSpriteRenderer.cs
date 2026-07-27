using UnityEngine;

namespace TW08.Common
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSpriteRenderer : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Vector2 size = Vector2.one;
        [SerializeField] private int sortingOrder;

        private static Sprite sharedSprite;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (sharedSprite == null)
            {
                Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "TW08 Prototype Pixel",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                sharedSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                sharedSprite.name = "TW08 Prototype Sprite";
            }

            spriteRenderer.sprite = sharedSprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
            transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        public void Configure(Color newColor, Vector2 newSize, int order)
        {
            color = newColor;
            size = newSize;
            sortingOrder = order;
        }
    }
}
