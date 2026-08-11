#if UNITY_EDITOR
using System;
using TW08.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TW08.Editor
{
    internal static class TW08ProductionSceneUtility
    {
        internal static readonly Color Background = new(0.014f, 0.019f, 0.022f, 1f);
        internal static readonly Color Panel = new(0.035f, 0.050f, 0.055f, 0.97f);
        internal static readonly Color PanelLight = new(0.055f, 0.075f, 0.080f, 0.98f);
        internal static readonly Color Green = new(0.25f, 0.95f, 0.58f, 1f);
        internal static readonly Color Amber = new(1f, 0.63f, 0.12f, 1f);
        internal static readonly Color Cyan = new(0.26f, 0.84f, 0.92f, 1f);
        internal static readonly Color Red = new(0.96f, 0.28f, 0.22f, 1f);
        internal static readonly Color TextPrimary = new(0.87f, 0.96f, 0.91f, 1f);
        internal static readonly Color TextMuted = new(0.47f, 0.64f, 0.57f, 1f);

        internal static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        internal static Camera CreateCamera(Vector3 position, float size)
        {
            GameObject go = new("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = position;
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = size;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;

            // Every standalone generated scene owns exactly one primary camera. Keeping the
            // AudioListener on that camera prevents silent scenes and Unity's missing-listener warning.
            go.AddComponent<AudioListener>();
            return camera;
        }

        internal static Canvas CreateCanvas()
        {
            GameObject go = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        internal static EventSystem CreateEventSystem()
        {
            GameObject go = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            return go.GetComponent<EventSystem>();
        }

        internal static Image CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        internal static Text CreateText(Transform parent, string name, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        internal static Button CreateButton(Transform parent, string name, string label, Color accent, int fontSize = 18)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = new Color(accent.r * 0.16f, accent.g * 0.16f, accent.b * 0.16f, 0.98f);
            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.24f, 1.24f, 1.24f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.disabledColor = new Color(0.32f, 0.32f, 0.32f, 0.65f);
            button.colors = colors;
            Text text = CreateText(go.transform, "Label", label, fontSize, accent, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        internal static GameObject CreateSprite(
            string name,
            Vector3 position,
            Sprite sprite,
            int order,
            Color tint,
            Vector3? scale = null)
        {
            GameObject go = new(name);
            go.transform.position = position;
            go.transform.localScale = scale ?? Vector3.one;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            renderer.color = tint;
            return go;
        }

        internal static BoxCollider2D AddBoxCollider(GameObject go, Vector2 size, bool trigger = false)
        {
            if (go == null)
            {
                throw new ArgumentNullException(nameof(go), "TW08 cannot add a BoxCollider2D to a null GameObject.");
            }

            // UnityEngine.Object implements custom null semantics. Do not use ?? here: a missing
            // Component can be represented by a managed wrapper that is non-null to C# while Unity's
            // overloaded == reports null. In that case ?? would skip AddComponent and the first native
            // property access would throw MissingComponentException.
            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = go.AddComponent<BoxCollider2D>();
            }

            if (collider == null)
            {
                throw new InvalidOperationException(
                    $"TW08 failed to attach BoxCollider2D to GameObject '{go.name}'. " +
                    "Verify that Unity's 2D Physics module is available for this project.");
            }

            collider.size = size;
            collider.isTrigger = trigger;
            EditorUtility.SetDirty(collider);
            return collider;
        }

        internal static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        internal static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        internal static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        internal static void DisableNavigation(Selectable selectable)
        {
            Navigation nav = selectable.navigation;
            nav.mode = Navigation.Mode.None;
            selectable.navigation = nav;
        }

        internal static void Select(EventSystem eventSystem, Button button)
        {
            if (eventSystem != null && button != null)
            {
                eventSystem.firstSelectedGameObject = button.gameObject;
            }
        }

        internal static Color CrateTint(TW08.Puzzle.PuzzleEntityKind kind)
        {
            return kind switch
            {
                TW08.Puzzle.PuzzleEntityKind.HeavyCrate => new Color(0.46f, 0.69f, 0.92f, 1f),
                TW08.Puzzle.PuzzleEntityKind.FragileCrate => new Color(1f, 0.52f, 0.36f, 1f),
                _ => Color.white
            };
        }
    }
}
#endif