using UnityEngine;
using TinyIsland.Core;
using TinyIsland.Player;

namespace TinyIsland.Tower
{
    public sealed class TowerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;

        [Header("Build")]
        [SerializeField] private int partsPerLevel = 3;
        [SerializeField] private int[] woodCostsByPart = { 2, 2, 1, 2, 2, 2, 2, 3, 2 };

        private Transform[][] _partsByLevel;
        private int _builtPartCount;

        public int BuiltPartCount => _builtPartCount;
        public int BuiltLevelCount => partsPerLevel > 0 ? _builtPartCount / partsPerLevel : 0;
        public int CurrentWoodCost => GetWoodCost(_builtPartCount);
        public bool IsFullyBuilt => _partsByLevel != null && _builtPartCount >= _partsByLevel.Length * partsPerLevel;

        private void Awake()
        {
            if (gameManager == null)
                gameManager = FindAnyObjectByType<GameManager>();

            CacheTowerParts();
            RefreshVisuals();
        }

        public bool CanBuild(PlayerWoodInventory inventory)
        {
            if (inventory == null || _partsByLevel == null || IsFullyBuilt)
                return false;

            if (_builtPartCount >= GetAllowedPartCount())
                return false;

            return inventory.WoodCount >= CurrentWoodCost;
        }

        public bool TryBuildNextPart(PlayerWoodInventory inventory)
        {
            if (!CanBuild(inventory))
                return false;

            if (!inventory.TrySpendWood(CurrentWoodCost))
                return false;

            _builtPartCount++;
            RefreshVisuals();

            return true;
        }

        private int GetAllowedPartCount()
        {
            int requiredTowerLevel = gameManager != null && gameManager.CurrentDayConfig != null
                ? gameManager.CurrentDayConfig.RequiredTowerLevel
                : _partsByLevel.Length;

            int allowedLevelCount = Mathf.Clamp(requiredTowerLevel, 0, _partsByLevel.Length);
            return allowedLevelCount * partsPerLevel;
        }

        private int GetWoodCost(int partIndex)
        {
            if (woodCostsByPart == null || woodCostsByPart.Length == 0)
                return 1;

            if (partIndex < 0)
                return 1;

            if (partIndex >= woodCostsByPart.Length)
                return woodCostsByPart[woodCostsByPart.Length - 1];

            return Mathf.Max(1, woodCostsByPart[partIndex]);
        }

        private void CacheTowerParts()
        {
            int levelCount = transform.childCount;
            _partsByLevel = new Transform[levelCount][];

            for (int levelIndex = 0; levelIndex < levelCount; levelIndex++)
            {
                Transform level = transform.GetChild(levelIndex);
                Transform[] parts = new Transform[Mathf.Min(partsPerLevel, level.childCount)];

                for (int partIndex = 0; partIndex < parts.Length; partIndex++)
                    parts[partIndex] = level.GetChild(partIndex);

                _partsByLevel[levelIndex] = parts;
            }
        }

        private void RefreshVisuals()
        {
            if (_partsByLevel == null)
                return;

            int visiblePartIndex = 0;

            for (int levelIndex = 0; levelIndex < _partsByLevel.Length; levelIndex++)
            {
                Transform[] parts = _partsByLevel[levelIndex];

                for (int partIndex = 0; partIndex < parts.Length; partIndex++)
                {
                    bool isBuilt = visiblePartIndex < _builtPartCount;
                    parts[partIndex].gameObject.SetActive(isBuilt);
                    visiblePartIndex++;
                }
            }
        }
    }
}
