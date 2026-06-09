using UnityEngine;
using UnityEngine.UI;

namespace TinyIsland.UI
{
    public sealed class RadialRingGraphic : MaskableGraphic
    {
        [SerializeField, Range(0f, 1f)] private float fillAmount = 1f;
        [SerializeField] private float thickness = 8f;
        [SerializeField, Range(16, 128)] private int segments = 64;

        public float FillAmount
        {
            get => fillAmount;
            set
            {
                float clampedValue = Mathf.Clamp01(value);
                if (Mathf.Approximately(fillAmount, clampedValue))
                    return;

                fillAmount = clampedValue;
                SetVerticesDirty();
            }
        }

        public void Configure(float amount, float ringThickness, Color ringColor)
        {
            fillAmount = Mathf.Clamp01(amount);
            thickness = ringThickness;
            color = ringColor;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            Rect rect = GetPixelAdjustedRect();
            float outerRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
            float angleRange = Mathf.Clamp01(fillAmount) * Mathf.PI * 2f;

            if (outerRadius <= 0f || angleRange <= 0f)
                return;

            float innerRadius = Mathf.Max(0f, outerRadius - Mathf.Clamp(thickness, 1f, outerRadius));
            int segmentCount = Mathf.Max(2, Mathf.CeilToInt(Mathf.Max(16, segments) * fillAmount));
            Vector2 center = rect.center;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            for (int i = 0; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                float angle = Mathf.PI * 0.5f - angleRange * t;
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
