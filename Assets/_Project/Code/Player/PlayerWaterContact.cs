using TinyIsland.Core;
using TinyIsland.Tide;
using UnityEngine;

namespace TinyIsland.Player
{
    public sealed class PlayerWaterContact : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TideController tideController;
        [SerializeField] private Transform waterTransform;

        [Header("Contact")]
        [SerializeField] private float contactRadius = 0.22f;
        [SerializeField] private float waterLevelPadding = 0f;

        private bool _hasTriggered;

        private void Awake()
        {
            if (gameManager == null)
                gameManager = FindAnyObjectByType<GameManager>();

            if (tideController == null)
                tideController = FindAnyObjectByType<TideController>();
        }

        private void LateUpdate()
        {
            if (_hasTriggered || gameManager == null)
                return;

            if (gameManager.State == GameState.GameLost || gameManager.State == GameState.GameWon)
                return;

            float playerContactLevel = transform.position.y - contactRadius;
            float waterLevel = GetWaterLevel();

            if (playerContactLevel > waterLevel + waterLevelPadding)
                return;

            _hasTriggered = true;
            gameManager.LoseGame();
        }

        private float GetWaterLevel()
        {
            if (tideController != null)
                return tideController.CurrentWaterWorldLevel;

            return waterTransform != null ? waterTransform.position.y : float.NegativeInfinity;
        }
    }
}
