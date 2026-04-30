using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TinyIsland.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private DayConfig[] dayConfigs;

        [Header("Restart")]
        [SerializeField] private bool restartOnGameLost = true;
        [SerializeField] private float restartDelay = 1f;

        private int _currentDayIndex;
        private GameState _state;
        private bool _isRestarting;

        public DayConfig CurrentDayConfig =>
            dayConfigs != null && _currentDayIndex >= 0 && _currentDayIndex < dayConfigs.Length
                ? dayConfigs[_currentDayIndex]
                : null;
        public GameState State => _state;

        private void Awake()
        {
            SetState(GameState.DayPreparation);
        }

        public void SetState(GameState newState)
        {
            _state = newState;
            Debug.Log($"Game state changed: {_state}");
        }

        public void StartDay()
        {
            SetState(GameState.LowTide);
        }

        public void CompleteDay()
        {
            _currentDayIndex++;

            if (_currentDayIndex >= dayConfigs.Length)
            {
                SetState(GameState.GameWon);
                return;
            }

            SetState(GameState.DayPreparation);
        }

        public void LoseGame()
        {
            if (_state == GameState.GameLost)
                return;

            SetState(GameState.GameLost);

            if (restartOnGameLost)
                RestartCurrentScene(restartDelay);
        }

        public void RestartCurrentScene(float delay = 0f)
        {
            if (_isRestarting)
                return;

            _isRestarting = true;
            StartCoroutine(RestartCurrentSceneRoutine(delay));
        }

        private static IEnumerator RestartCurrentSceneRoutine(float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
