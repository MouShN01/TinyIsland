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
        [SerializeField] private int daytimeMiniTideCount = 3;
        [SerializeField] private float dayStartWaterLevel = 1.35f;
        [SerializeField] private float firstLowTideLevel = 0.9f;
        [SerializeField] private float initialEbbDuration = 4f;
        [SerializeField] private float lowTideDuration = 8f;
        [SerializeField] private float miniTideInterval = 15f;
        [SerializeField] private float miniTideWarningDuration = 2f;
        [SerializeField] private float miniTideRiseDuration = 3f;
        [SerializeField] private float miniTidePeakDuration = 3f;
        [SerializeField] private float miniTideFallDuration = 3f;
        [SerializeField] private float miniTideHeight = 0.5f;
        [SerializeField] private float nightTideWarningDuration = 5f;
        [SerializeField] private float nightTideDuration = 20f;
        [SerializeField] private float nightTideMaxHeight = 3f;
        [SerializeField] private float warningShakeAmplitude = 0.04f;
        [SerializeField] private float warningShakeFrequency = 24f;

        [Header("Tower")]
        [SerializeField] private int requiredTowerLevel = 1;
        [SerializeField] private int climbingInputCount = 2;
        [SerializeField] private float climbingTimingWindow = 1.2f;

        [Header("Resources")]
        [SerializeField] private int woodSpawnCount = 5;

        [Header("Hazards")]
        [SerializeField] private int crabCount = 0;

        public int DayNumber => dayNumber;
        public int DaytimeMiniTideCount => daytimeMiniTideCount;
        public float DayStartWaterLevel => dayStartWaterLevel;
        public float FirstLowTideLevel => firstLowTideLevel;
        public float InitialEbbDuration => initialEbbDuration;
        public float LowTideDuration => lowTideDuration;
        public float MiniTideInterval => miniTideInterval;
        public float MiniTideWarningDuration => miniTideWarningDuration;
        public float MiniTideRiseDuration => miniTideRiseDuration;
        public float MiniTidePeakDuration => miniTidePeakDuration;
        public float MiniTideFallDuration => miniTideFallDuration;
        public float MiniTideHeight => miniTideHeight;
        public float NightTideWarningDuration => nightTideWarningDuration;
        public float NightTideDuration => nightTideDuration;
        public float NightTideMaxHeight => nightTideMaxHeight;
        public float WarningShakeAmplitude => warningShakeAmplitude;
        public float WarningShakeFrequency => warningShakeFrequency;
        public int RequiredTowerLevel => requiredTowerLevel;
        public int ClimbingInputCount => climbingInputCount;
        public float ClimbingTimingWindow => climbingTimingWindow;
        public int WoodSpawnCount => woodSpawnCount;
        public int CrabCount => crabCount;
    }
}
