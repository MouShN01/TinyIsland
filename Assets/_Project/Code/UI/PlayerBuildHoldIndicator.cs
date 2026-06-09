using UnityEngine;
using UnityEngine.UI;

namespace TinyIsland.UI
{
    public sealed class PlayerBuildHoldIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RhythmCircleGraphic centerCircle;
        [SerializeField] private RadialRingGraphic backgroundRing;
        [SerializeField] private RadialRingGraphic progressRing;
        [SerializeField] private Text promptText;

        [Header("Layout")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private float worldScale = 0.008f;
        [SerializeField] private Vector2 canvasSize = new Vector2(112f, 112f);
        [SerializeField] private float ringDiameter = 92f;
        [SerializeField] private float ringThickness = 8f;
        [SerializeField] private float centerDiameter = 58f;

        [Header("Style")]
        [SerializeField] private Color centerColor = new Color(0.07f, 0.1f, 0.12f, 0.82f);
        [SerializeField] private Color backgroundRingColor = new Color(1f, 1f, 1f, 0.22f);
        [SerializeField] private Color progressRingColor = new Color(0.5f, 0.9f, 0.62f, 1f);
        [SerializeField] private Color unavailableRingColor = new Color(1f, 0.42f, 0.36f, 0.9f);
        [SerializeField] private Color textColor = new Color(0.96f, 0.98f, 1f, 1f);

        private RectTransform _rectTransform;
        private UnityEngine.Camera _mainCamera;

        public static PlayerBuildHoldIndicator CreateForTarget(Transform target)
        {
            GameObject indicatorObject = new GameObject(
                "Build Hold Indicator",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasGroup),
                typeof(PlayerBuildHoldIndicator)
            );

            PlayerBuildHoldIndicator indicator = indicatorObject.GetComponent<PlayerBuildHoldIndicator>();
            indicator.followTarget = target;
            indicator.BuildIfNeeded();
            indicator.Hide();
            return indicator;
        }

        private void Awake()
        {
            BuildIfNeeded();
            Hide();
        }

        private void LateUpdate()
        {
            FollowTarget();
            FaceCamera();
        }

        [ContextMenu("Build Visuals")]
        private void BuildIfNeeded()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
                _rectTransform = gameObject.AddComponent<RectTransform>();

            canvas = GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 45;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Font font = LoadDefaultFont();
            backgroundRing = backgroundRing != null ? backgroundRing : CreateRing("Hold Ring Background", transform, backgroundRingColor);
            progressRing = progressRing != null ? progressRing : CreateRing("Hold Ring Fill", transform, progressRingColor);
            centerCircle = centerCircle != null ? centerCircle : CreateCenterCircle();
            promptText = promptText != null ? promptText : CreateText("Hold Prompt", centerCircle.transform, font);

            ApplyLayout();
            ApplyStyle();
        }

        public void SetTarget(Transform target)
        {
            followTarget = target;
            FollowTarget();
        }

        public void Show(float progress01, bool canBuild)
        {
            BuildIfNeeded();
            canvasGroup.alpha = 1f;
            progressRing.FillAmount = Mathf.Clamp01(progress01);
            progressRing.color = canBuild ? progressRingColor : unavailableRingColor;
            promptText.text = "E";
            FollowTarget();
            FaceCamera();
        }

        public void Hide()
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 0f;
            progressRing.FillAmount = 0f;
        }

        private void FollowTarget()
        {
            if (followTarget == null)
                return;

            transform.position = followTarget.position + worldOffset;
        }

        private void FaceCamera()
        {
            if (_mainCamera == null)
                _mainCamera = UnityEngine.Camera.main;

            if (_mainCamera == null)
                return;

            Vector3 direction = transform.position - _mainCamera.transform.position;
            if (direction.sqrMagnitude <= 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void ApplyLayout()
        {
            _rectTransform.sizeDelta = canvasSize;
            _rectTransform.localScale = Vector3.one * worldScale;

            RectTransform backgroundRingRect = backgroundRing.rectTransform;
            backgroundRingRect.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundRingRect.anchorMax = new Vector2(0.5f, 0.5f);
            backgroundRingRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRingRect.anchoredPosition = Vector2.zero;
            backgroundRingRect.sizeDelta = Vector2.one * ringDiameter;

            RectTransform progressRingRect = progressRing.rectTransform;
            progressRingRect.anchorMin = new Vector2(0.5f, 0.5f);
            progressRingRect.anchorMax = new Vector2(0.5f, 0.5f);
            progressRingRect.pivot = new Vector2(0.5f, 0.5f);
            progressRingRect.anchoredPosition = Vector2.zero;
            progressRingRect.sizeDelta = Vector2.one * ringDiameter;

            RectTransform centerRect = centerCircle.rectTransform;
            centerRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerRect.pivot = new Vector2(0.5f, 0.5f);
            centerRect.anchoredPosition = Vector2.zero;
            centerRect.sizeDelta = Vector2.one * centerDiameter;

            RectTransform textRect = promptText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private void ApplyStyle()
        {
            backgroundRing.Configure(1f, ringThickness, backgroundRingColor);
            progressRing.Configure(progressRing.FillAmount, ringThickness, progressRingColor);
            centerCircle.Configure(true, ringThickness, centerColor);
            promptText.color = textColor;
        }

        private RadialRingGraphic CreateRing(string name, Transform parent, Color ringColor)
        {
            GameObject ringObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RadialRingGraphic));
            ringObject.transform.SetParent(parent, false);

            RadialRingGraphic ring = ringObject.GetComponent<RadialRingGraphic>();
            ring.raycastTarget = false;
            ring.Configure(1f, ringThickness, ringColor);
            return ring;
        }

        private RhythmCircleGraphic CreateCenterCircle()
        {
            GameObject circleObject = new GameObject("Hold Center", typeof(RectTransform), typeof(CanvasRenderer), typeof(RhythmCircleGraphic));
            circleObject.transform.SetParent(transform, false);

            RhythmCircleGraphic circle = circleObject.GetComponent<RhythmCircleGraphic>();
            circle.raycastTarget = false;
            circle.Configure(true, ringThickness, centerColor);
            return circle;
        }

        private Text CreateText(string name, Transform parent, Font font)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = 34;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = textColor;
            text.raycastTarget = false;
            return text;
        }

        private static Font LoadDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
                return font;

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
