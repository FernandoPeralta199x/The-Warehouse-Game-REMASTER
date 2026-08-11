using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class TerminalGridGraphic : MaskableGraphic
    {
        [SerializeField, Min(16f)] private float gridSpacing = 64f;
        [SerializeField, Min(2f)] private float scanlineSpacing = 8f;
        [SerializeField, Min(0.25f)] private float gridThickness = 1f;
        [SerializeField, Range(0f, 1f)] private float scanlineAlphaMultiplier = 0.22f;

        public void Configure(
            Color tint,
            float majorGridSpacing = 64f,
            float minorScanlineSpacing = 8f,
            float thickness = 1f)
        {
            color = tint;
            gridSpacing = Mathf.Max(16f, majorGridSpacing);
            scanlineSpacing = Mathf.Max(2f, minorScanlineSpacing);
            gridThickness = Mathf.Max(0.25f, thickness);
            raycastTarget = false;
            SetVerticesDirty();
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            Color32 major = color;
            Color minorColor = new(color.r, color.g, color.b, color.a * scanlineAlphaMultiplier);
            Color32 minor = minorColor;

            for (float x = rect.xMin; x <= rect.xMax + 0.5f; x += gridSpacing)
            {
                AddQuad(vh, new Rect(x, rect.yMin, gridThickness, rect.height), major);
            }

            for (float y = rect.yMin; y <= rect.yMax + 0.5f; y += gridSpacing)
            {
                AddQuad(vh, new Rect(rect.xMin, y, rect.width, gridThickness), major);
            }

            float minorThickness = Mathf.Max(0.35f, gridThickness * 0.55f);
            for (float y = rect.yMin; y <= rect.yMax + 0.5f; y += scanlineSpacing)
            {
                AddQuad(vh, new Rect(rect.xMin, y, rect.width, minorThickness), minor);
            }
        }

        private static void AddQuad(VertexHelper vh, Rect rect, Color32 color)
        {
            int start = vh.currentVertCount;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = new Vector3(rect.xMin, rect.yMin);
            vh.AddVert(vertex);
            vertex.position = new Vector3(rect.xMin, rect.yMax);
            vh.AddVert(vertex);
            vertex.position = new Vector3(rect.xMax, rect.yMax);
            vh.AddVert(vertex);
            vertex.position = new Vector3(rect.xMax, rect.yMin);
            vh.AddVert(vertex);

            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
