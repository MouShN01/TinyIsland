using UnityEngine;
using UnityEngine.UI;

namespace TinyIsland.UI
{
    public sealed class RhythmClimbHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text statusText;
        [SerializeField] private Text promptLetterText;
        [SerializeField] private Image panelImage;
        [SerializeField] private Image laneImage;
        [SerializeField] private RhythmCircleGraphic targetCircleGraphic;
        [SerializeField] private RhythmCircleGraphic promptCircleGraphic;

        [Header("Layout")]
        [SerializeField] private Vector2 panelSize = new Vector2(520f, 168f);
        [SerializeField] private Vector2 panelOffset = new Vector2(0f, 32f);
        [SerializeField] private float laneHorizontalPadding = 78f;
        [SerializeField] private float laneY = 70f;
        [SerializeField] private float laneHeight = 4f;
        [SerializeField] private float promptCircleDiameter = 72f;
        [SerializeField] private float targetRingThickness = 6f;

        [Header("Style")]
        [SerializeField] private Color panelColor = new Color(0.05f, 0.07f, 0.08f, 0.78f);
        [SerializeField] private Color laneColor = new Color(1f, 1f, 1f, 0.14f);
        [SerializeField] private Color targetCircleColor = new Color(0.42f, 0.92f, 0.66f, 0.72f);
        [SerializeField] private Color targetCircleMatchedColor = new Color(0.5f, 1f, 0.76f, 1f);
        [SerializeField] private Color promptCircleColor = new Color(1f, 0.88f, 0.34f, 1f);
        [SerializeField] private Color promptCircleMatchedColor = new Color(0.5f, 1f, 0.76f, 1f);
        [SerializeField] private Color textColor = new Color(0.95f, 0.98f, 1f, 1f);

        private RectTransform _panelRect;
        private RectTransform _targetCircleRect;
        private RectTransform _promptCircleRect;

        public bool IsVisible => canvasGroup != null && canvasGroup.alpha > 0.01f;

        public static RhythmClimbHud CreateDefault()
        {
            GameObject root = new GameObject(
                "Rhythm Climb HUD",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup),
                typeof(RhythmClimbHud)
            );

            RhythmClimbHud hud = root.GetComponent<RhythmClimbHud>();
            hud.BuildIfNeeded();
            hud.Hide();
            return hud;
        }

        private void Awake()
        {
            BuildIfNeeded();
            Hide();
        }

        [ContextMenu("Build Visuals")]
        private void BuildIfNeeded()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Font font = LoadDefaultFont();
            panelImage = panelImage != null ? panelImage : CreatePanel();
            statusText = statusText != null ? statusText : CreateText("Status", panelImage.transform, 16, FontStyle.Bold, TextAnchor.MiddleCenter, font);
            laneImage = laneImage != null ? laneImage : CreateImage("Travel Lane", panelImage.transform, laneColor);
            targetCircleGraphic = targetCircleGraphic != null ? targetCircleGraphic : CreateCircle("Target Circle", panelImage.transform, false, targetRingThickness, targetCircleColor);
            promptCircleGraphic = promptCircleGraphic != null ? promptCircleGraphic : CreateCircle("Prompt Circle", panelImage.transform, true, targetRingThickness, promptCircleColor);
            promptLetterText = promptLetterText != null ? promptLetterText : CreateText("Prompt Letter", promptCircleGraphic.transform, 44, FontStyle.Bold, TextAnchor.MiddleCenter, font);

            ApplyLayout();
            ApplyStyle();
        }

        public void ShowClimbPrompt(
            string actionLabel,
            string prompt,
            float promptProgress,
            float hitStartNormalized,
            float hitEndNormalized,
            int stepIndex,
            int stepCount
        )
        {
            BuildIfNeeded();
            SetVisible(true);

            int safeStepCount = Mathf.Max(1, stepCount);
            int visibleStep = Mathf.Clamp(stepIndex, 0, safeStepCount);
            statusText.text = $"{actionLabel}  {visibleStep}/{safeStepCount}";
            promptLetterText.text = string.IsNullOrEmpty(prompt) ? string.Empty : prompt;
            SetTimingVisible(true);
            SetCircleTiming(promptProgress, hitStartNormalized, hitEndNormalized);
        }

        public void ShowWaiting(string message)
        {
            BuildIfNeeded();
            SetVisible(true);

            statusText.text = message;
            promptLetterText.text = string.Empty;
            SetTimingVisible(false);
        }

        public void Hide()
        {
            if (canvasGroup == null)
                return;

            SetVisible(false);
        }

        private void SetVisible(bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1f : 0f;
        }

        private void SetTimingVisible(bool isVisible)
        {
            laneImage.gameObject.SetActive(isVisible);
            targetCircleGraphic.gameObject.SetActive(isVisible);
            promptCircleGraphic.gameObject.SetActive(isVisible);
        }

        private void SetCircleTiming(float promptProgress, float hitStartNormalized, float hitEndNormalized)
        {
            float hitStart = Mathf.Clamp01(hitStartNormalized);
            float hitEnd = Mathf.Clamp01(Mathf.Max(hitStart, hitEndNormalized));
            float promptPosition = Mathf.Clamp01(promptProgress);
            float targetPosition = Mathf.Lerp(hitStart, hitEnd, 0.5f);
            float hitWidth = Mathf.Max(0.01f, hitEnd - hitStart);
            float laneWidth = Mathf.Max(1f, panelSize.x - laneHorizontalPadding * 2f);
            float targetDiameter = Mathf.Clamp(laneWidth * hitWidth, promptCircleDiameter, promptCircleDiameter * 1.45f);
            bool isMatched = promptPosition >= hitStart && promptPosition <= hitEnd;

            _targetCircleRect.anchoredPosition = new Vector2(GetLaneX(targetPosition), laneY);
            _targetCircleRect.sizeDelta = Vector2.one * targetDiameter;

            _promptCircleRect.anchoredPosition = new Vector2(GetLaneX(promptPosition), laneY);
            _promptCircleRect.sizeDelta = Vector2.one * promptCircleDiameter;

            targetCircleGraphic.Configure(false, targetRingThickness, isMatched ? targetCircleMatchedColor : targetCircleColor);
            promptCircleGraphic.Configure(true, targetRingThickness, isMatched ? promptCircleMatchedColor : promptCircleColor);
        }

        private float GetLaneX(float normalizedPosition)
        {
            float halfWidth = panelSize.x * 0.5f;
            float left = -halfWidth + laneHorizontalPadding;
            float right = halfWidth - laneHorizontalPadding;
            return Mathf.Lerp(left, right, Mathf.Clamp01(normalizedPosition));
        }

        private Image CreatePanel()
        {
            Image image = CreateImage("Rhythm Panel", transform, panelColor);
            _panelRect = image.rectTransform;
            return image;
        }

        private RhythmCircleGraphic CreateCircle(string name, Transform parent, bool isFilled, float thickness, Color color)
        {
            GameObject circleObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RhythmCircleGraphic));
            circleObject.transform.SetParent(parent, false);

            RhythmCircleGraphic circleGraphic = circleObject.GetComponent<RhythmCircleGraphic>();
            circleGraphic.raycastTarget = false;
            circleGraphic.Configure(isFilled, thickness, color);
            return circleGraphic;
        }

        private Text CreateText(
            string name,
            Transform parent,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Font font
        )
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = textColor;
            text.raycastTarget = false;
            return text;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void ApplyLayout()
        {
            _panelRect = panelImage.rectTransform;
            _panelRect.anchorMin = new Vector2(0.5f, 0f);
            _panelRect.anchorMax = new Vector2(0.5f, 0f);
            _panelRect.pivot = new Vector2(0.5f, 0f);
            _panelRect.anchoredPosition = panelOffset;
            _panelRect.sizeDelta = panelSize;

            RectTransform statusRect = statusText.rectTransform;
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.anchoredPosition = new Vector2(0f, -16f);
            statusRect.sizeDelta = new Vector2(-32f, 28f);

            RectTransform laneRect = laneImage.rectTransform;
            laneRect.anchorMin = new Vector2(0.5f, 0f);
            laneRect.anchorMax = new Vector2(0.5f, 0f);
            laneRect.pivot = new Vector2(0.5f, 0.5f);
            laneRect.anchoredPosition = new Vector2(0f, laneY);
            laneRect.sizeDelta = new Vector2(panelSize.x - laneHorizontalPadding * 2f, laneHeight);

            _targetCircleRect = targetCircleGraphic.rectTransform;
            _targetCircleRect.anchorMin = new Vector2(0.5f, 0f);
            _targetCircleRect.anchorMax = new Vector2(0.5f, 0f);
            _targetCircleRect.pivot = new Vector2(0.5f, 0.5f);

            _promptCircleRect = promptCircleGraphic.rectTransform;
            _promptCircleRect.anchorMin = new Vector2(0.5f, 0f);
            _promptCircleRect.anchorMax = new Vector2(0.5f, 0f);
            _promptCircleRect.pivot = new Vector2(0.5f, 0.5f);
            _promptCircleRect.SetAsLastSibling();

            RectTransform promptLetterRect = promptLetterText.rectTransform;
            promptLetterRect.anchorMin = Vector2.zero;
            promptLetterRect.anchorMax = Vector2.one;
            promptLetterRect.offsetMin = Vector2.zero;
            promptLetterRect.offsetMax = Vector2.zero;
        }

        private void ApplyStyle()
        {
            panelImage.color = panelColor;
            laneImage.color = laneColor;
            targetCircleGraphic.Configure(false, targetRingThickness, targetCircleColor);
            promptCircleGraphic.Configure(true, targetRingThickness, promptCircleColor);
            statusText.color = textColor;
            promptLetterText.color = textColor;
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
