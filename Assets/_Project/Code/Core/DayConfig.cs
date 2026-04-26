using UnityEngine;
namespace TinyIsland.Core
{
    [CreateAssetMenu(
        fileName = "DayConfig", 
        menuName = "TinyIsland/Configs/Day Config")]
    public sealed class DayConfig : ScriptableObject
    {
        [Header("Day")]
        [SerializeField] private int dayNumber = 1;

        [Header("Tide")]
        [SerializeField] private float lowTideDuration = 60f;
        [SerializeField] private float nightTideDuration = 20f;
        [SerializeField] private float miniTideInterval = 15f;
        [SerializeField] private float miniTideWarningDuration = 2f;
        [SerializeField] private float miniTideHeight = 0.5f;
        [SerializeField] private float nightTideMaxHeight = 3f;

        [Header("Tower")]
        [SerializeField] private int requiredTowerLevel = 1;
        [SerializeField] private int climbingInputCount = 2;
        [SerializeField] private float climbingTimingWindow = 1.2f;

        [Header("Resources")]
        [SerializeField] private int woodSpawnCount = 5;

        [Header("Hazards")]
        [SerializeField] private int crabCount = 0;

        public int DayNumber => dayNumber;
        public float LowTideDuration => lowTideDuration;
        public float NightTideDuration => nightTideDuration;
        public float MiniTideInterval => miniTideInterval;
        public float MiniTideWarningDuration => miniTideWarningDuration;
        public float MiniTideHeight => miniTideHeight;
        public float NightTideMaxHeight => nightTideMaxHeight;
        public int RequiredTowerLevel => requiredTowerLevel;
        public int ClimbingInputCount => climbingInputCount;
        public float ClimbingTimingWindow => climbingTimingWindow;
        public int WoodSpawnCount => woodSpawnCount;
        public int CrabCount => crabCount;
    }
}