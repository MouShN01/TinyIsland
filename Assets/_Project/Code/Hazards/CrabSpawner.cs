using System.Collections.Generic;
using TinyIsland.Core;
using TinyIsland.Player;
using UnityEngine;

namespace TinyIsland.Hazards
{
    public sealed class CrabSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject crabPrefab;
        [SerializeField] private Transform spawnPointsRoot;
        [SerializeField] private Transform spawnedParent;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Transform islandCenter;

        [Header("Spawn")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool clearBeforeSpawn = true;
        [SerializeField] private bool randomizeSpawnPoints = true;

        private readonly List<GameObject> _spawnedCrabs = new List<GameObject>();
        private readonly List<Transform> _spawnPoints = new List<Transform>();
        private int _lastSpawnedDayNumber = -1;

        private void Awake()
        {
            if (gameManager == null)
                gameManager = FindAnyObjectByType<GameManager>();

            if (spawnPointsRoot == null)
                spawnPointsRoot = transform;

            if (spawnedParent == null)
                spawnedParent = transform;

            if (playerTarget == null)
            {
                PlayerSphereWalker player = FindAnyObjectByType<PlayerSphereWalker>();
                if (player != null)
                    playerTarget = player.transform;
            }

            if (islandCenter == null)
            {
                GameObject island = GameObject.Find("Island_SandDome");
                if (island != null)
                    islandCenter = island.transform;
            }
        }

        private void OnEnable()
        {
            if (gameManager == null)
                gameManager = FindAnyObjectByType<GameManager>();

            if (gameManager != null)
                gameManager.DayStarted += OnDayStarted;
        }

        private void OnDisable()
        {
            if (gameManager != null)
                gameManager.DayStarted -= OnDayStarted;
        }

        private void Start()
        {
            if (spawnOnStart)
                SpawnForCurrentDay();
        }

        public void SpawnForCurrentDay()
        {
            DayConfig currentDay = gameManager != null ? gameManager.CurrentDayConfig : null;
            int dayNumber = currentDay != null ? currentDay.DayNumber : 1;
            int crabCount = currentDay != null ? currentDay.CrabCount : 0;

            SpawnForDay(dayNumber, crabCount);
        }

        public void SpawnForDay(int dayNumber, int crabCount)
        {
            if (_lastSpawnedDayNumber == dayNumber)
                return;

            if (crabPrefab == null)
            {
                Debug.LogWarning("CrabSpawner has no crab prefab assigned.", this);
                return;
            }

            RefreshSpawnPoints();

            if (_spawnPoints.Count == 0)
            {
                Debug.LogWarning("CrabSpawner has no spawn points.", this);
                return;
            }

            if (clearBeforeSpawn)
                ClearSpawnedCrabs();

            if (randomizeSpawnPoints)
                Shuffle(_spawnPoints);

            int spawnCount = Mathf.Min(Mathf.Max(0, crabCount), _spawnPoints.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                Transform spawnPoint = _spawnPoints[i];
                GameObject spawnedCrab = Instantiate(crabPrefab, spawnPoint.position, spawnPoint.rotation, spawnedParent);
                CrabController crab = spawnedCrab.GetComponent<CrabController>();

                if (crab != null)
                    crab.Initialize(playerTarget, islandCenter);

                _spawnedCrabs.Add(spawnedCrab);
            }

            _lastSpawnedDayNumber = dayNumber;
        }

        public void ClearSpawnedCrabs()
        {
            for (int i = _spawnedCrabs.Count - 1; i >= 0; i--)
            {
                if (_spawnedCrabs[i] != null)
                    Destroy(_spawnedCrabs[i]);
            }

            _spawnedCrabs.Clear();
        }

        private void RefreshSpawnPoints()
        {
            _spawnPoints.Clear();

            for (int i = 0; i < spawnPointsRoot.childCount; i++)
                _spawnPoints.Add(spawnPointsRoot.GetChild(i));
        }

        private static void Shuffle<T>(IList<T> items)
        {
            for (int i = 0; i < items.Count - 1; i++)
            {
                int randomIndex = Random.Range(i, items.Count);
                (items[i], items[randomIndex]) = (items[randomIndex], items[i]);
            }
        }

        private void OnDayStarted(DayConfig dayConfig)
        {
            if (!spawnOnStart || dayConfig == null)
                return;

            SpawnForDay(dayConfig.DayNumber, dayConfig.CrabCount);
        }
    }
}
