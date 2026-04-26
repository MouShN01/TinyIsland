using UnityEngine;

namespace TinyIsland.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private DayConfig[] dayConfigs;

        private int _currentDayIndex;
        private GameState _state;

        public DayConfig CurrentDayConfig => dayConfigs[_currentDayIndex];
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
            SetState(GameState.GameLost);
        }
    }
}
