using System.Collections;
using TinyIsland.Core;
using UnityEngine;

namespace TinyIsland.Tide
{
    public sealed class TideController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Transform waterTransform;

        [Header("Flow")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool autoAdvanceDays = true;
        [SerializeField] private float dayTransitionDelay = 2f;

        [Header("Water Transform")]
        [SerializeField] private bool useLocalPosition = true;

        private Coroutine _tideRoutine;
        private float _currentWaterLevel;

        public float CurrentWaterLevel => _currentWaterLevel;
        public float CurrentWaterWorldLevel => waterTransform != null ? waterTransform.position.y : _currentWaterLevel;

        private void Awake()
        {
            if (gameManager == null)
                gameManager = FindAnyObjectByType<GameManager>();

            if (waterTransform != null)
                _currentWaterLevel = GetWaterLevel();
        }

        private void Start()
        {
            if (playOnStart)
                StartTideLoop();
        }

        public void StartTideLoop()
        {
            if (_tideRoutine != null)
                StopCoroutine(_tideRoutine);

            _tideRoutine = StartCoroutine(RunTideLoop());
        }

        public void StopTideLoop()
        {
            if (_tideRoutine == null)
                return;

            StopCoroutine(_tideRoutine);
            _tideRoutine = null;
        }

        private IEnumerator RunTideLoop()
        {
            while (gameManager != null && gameManager.State != GameState.GameWon && gameManager.State != GameState.GameLost)
            {
                DayConfig dayConfig = gameManager.CurrentDayConfig;

                if (dayConfig == null || waterTransform == null)
                    yield break;

                gameManager.StartDay();
                yield return RunDay(dayConfig);

                if (!autoAdvanceDays)
                {
                    gameManager.SetState(GameState.DayComplete);
                    yield break;
                }

                gameManager.CompleteDay();

                if (gameManager.State == GameState.GameWon || gameManager.State == GameState.GameLost)
                    yield break;

                if (dayTransitionDelay > 0f)
                    yield return new WaitForSeconds(dayTransitionDelay);
            }
        }

        private IEnumerator RunDay(DayConfig dayConfig)
        {
            int tideCount = Mathf.Max(1, dayConfig.DaytimeMiniTideCount);
            float currentLevel = dayConfig.DayStartWaterLevel;

            SetWaterLevel(currentLevel);

            for (int i = 0; i < tideCount; i++)
            {
                float lowLevel = MiniTideController.GetLowLevel(dayConfig, i);
                float ebbDuration = i == 0 ? dayConfig.InitialEbbDuration : dayConfig.MiniTideFallDuration;

                gameManager.SetState(GameState.LowTide);
                yield return MoveWater(currentLevel, lowLevel, ebbDuration);
                currentLevel = lowLevel;

                float holdDuration = i == 0
                    ? dayConfig.LowTideDuration
                    : dayConfig.MiniTideInterval;

                if (holdDuration > 0f)
                    yield return new WaitForSeconds(holdDuration);

                gameManager.SetState(GameState.MiniTideWarning);
                yield return ShakeWater(currentLevel, dayConfig.MiniTideWarningDuration, dayConfig.WarningShakeAmplitude, dayConfig.WarningShakeFrequency);

                gameManager.SetState(GameState.MiniTide);

                float miniTideLevel = MiniTideController.GetPeakLevel(dayConfig, i);
                yield return MoveWater(currentLevel, miniTideLevel, dayConfig.MiniTideRiseDuration);
                currentLevel = miniTideLevel;

                if (dayConfig.MiniTidePeakDuration > 0f)
                    yield return new WaitForSeconds(dayConfig.MiniTidePeakDuration);
            }

            gameManager.SetState(GameState.NightTideWarning);
            yield return ShakeWater(currentLevel, dayConfig.NightTideWarningDuration, dayConfig.WarningShakeAmplitude, dayConfig.WarningShakeFrequency);

            gameManager.SetState(GameState.NightTide);
            yield return MoveWater(currentLevel, dayConfig.NightTideMaxHeight, dayConfig.NightTideDuration);
        }

        private IEnumerator MoveWater(float fromLevel, float toLevel, float duration)
        {
            if (duration <= 0f)
            {
                SetWaterLevel(toLevel);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = Mathf.SmoothStep(0f, 1f, t);

                SetWaterLevel(Mathf.Lerp(fromLevel, toLevel, easedT));
                yield return null;
            }

            SetWaterLevel(toLevel);
        }

        private IEnumerator ShakeWater(float baseLevel, float duration, float amplitude, float frequency)
        {
            if (duration <= 0f || amplitude <= 0f || frequency <= 0f)
            {
                SetWaterLevel(baseLevel);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float shake = Mathf.Sin(elapsed * frequency) * amplitude;
                SetWaterLevel(baseLevel + shake);

                yield return null;
            }

            SetWaterLevel(baseLevel);
        }

        private float GetWaterLevel()
        {
            return useLocalPosition
                ? waterTransform.localPosition.y
                : waterTransform.position.y;
        }

        private void SetWaterLevel(float level)
        {
            _currentWaterLevel = level;

            if (useLocalPosition)
            {
                Vector3 localPosition = waterTransform.localPosition;
                localPosition.y = level;
                waterTransform.localPosition = localPosition;
                return;
            }

            Vector3 position = waterTransform.position;
            position.y = level;
            waterTransform.position = position;
        }
    }
}
