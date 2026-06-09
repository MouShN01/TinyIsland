using UnityEngine;
using UnityEngine.UI;

namespace TinyIsland.UI
{
    public sealed class RhythmCircleGraphic : MaskableGraphic
    {
        [SerializeField] private bool filled = true;
        [SerializeField] private float ringThickness = 5f;
        [SerializeField, Range(16, 96)] private int segments = 48;

        public void Configure(bool isFilled, float thickness, Color circleColor)
        {
            filled = isFilled;
            ringThickness = thickness;
            color = circleColor;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            if (radius <= 0f)
                return;

            Vector2 center = rect.center;
            int segmentCount = Mathf.Max(16, segments);

            if (filled)
            {
                DrawFilledCircle(vertexHelper, center, radius, segmentCount);
                return;
            }

            DrawRing(vertexHelper, center, radius, Mathf.Clamp(ringThickness, 1f, radius), segmentCount);
        }

        private void DrawFilledCircle(VertexHelper vertexHelper, Vector2 center, float radius, int segmentCount)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = center;
            vertexHelper.AddVert(vertex);

            for (int i = 0; i <= segmentCount; i++)
            {
                float angle = i / (float)segmentCount * Mathf.PI * 2f;
                vertex.position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vertexHelper.AddVert(vertex);
            }

            for (int i = 1; i <= segmentCount; i++)
                vertexHelper.AddTriangle(0, i, i + 1);
        }

        private void DrawRing(VertexHelper vertexHelper, Vector2 center, float outerRadius, float thickness, int segmentCount)
        {
            float innerRadius = Mathf.Max(0f, outerRadius - thickness);
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            for (int i = 0; i <= segmentCount; i++)
            {
                float angle = i / (float)segmentCount * Mathf.PI * 2f;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                vertex.position = center + direction * outerRadius;
                vertexHelper.AddVert(vertex);

                vertex.position = center + direction * innerRadius;
                vertexHelper.AddVert(vertex);
            }

            for (int i = 0; i < segmentCount; i++)
            {
                int index = i * 2;
                vertexHelper.AddTriangle(index, index + 1, index + 2);
                vertexHelper.AddTriangle(index + 2, index + 1, index + 3);
            }
        }
    }
}
