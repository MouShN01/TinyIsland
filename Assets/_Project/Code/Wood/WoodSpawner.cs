using System.Collections.Generic;
using TinyIsland.Core;
using UnityEngine;

namespace TinyIsland.Wood
{
    public sealed class WoodSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject woodPrefab;
        [SerializeField] private Transform spawnPointsRoot;
        [SerializeField] private Transform spawnedParent;

        [Header("Spawn")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool clearBeforeSpawn = true;
        [SerializeField] private bool randomizeSpawnPoints = true;

        [Header("Day Point Ranges")]
        [SerializeField] private int dayOneLastPointNumber = 4;
        [SerializeField] private int dayTwoLastPointNumber = 10;

        private readonly List<GameObject> _spawnedWood = new List<GameObject>();
        private readonly List<Transform> _spawnPoints = new List<Transform>();
        private readonly List<Transform> _selectedSpawnPoints = new List<Transform>();
        private readonly List<Transform> _previewCandidates = new List<Transform>();
        private int _lastSpawnedDayNumber = -1;

        private void Awake()
        {
            if (gameManager == null)
                gameManager = FindAnyObjectByType<GameManager>();

            if (spawnPointsRoot == null)
                spawnPointsRoot = transform;

            if (spawnedParent == null)
                spawnedParent = transform;
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
            int dayNumber = gameManager != null && gameManager.CurrentDayConfig != null
                ? gameManager.CurrentDayConfig.DayNumber
                : 1;
            int spawnCount = gameManager != null && gameManager.CurrentDayConfig != null
                ? gameManager.CurrentDayConfig.WoodSpawnCount
                : 0;

            SpawnForDay(dayNumber, spawnCount);
        }

        public void SpawnForDay(int dayNumber, int spawnCount)
        {
            if (_lastSpawnedDayNumber == dayNumber)
                return;

            if (woodPrefab == null)
            {
                Debug.LogWarning("WoodSpawner has no wood prefab assigned.", this);
                return;
            }

            RefreshSpawnPoints();

            if (_spawnPoints.Count == 0)
            {
                Debug.LogWarning("WoodSpawner has no spawn points.", this);
                return;
            }

            if (clearBeforeSpawn)
                ClearSpawnedWood();

            SelectSpawnPointsForDay(dayNumber, spawnCount);

            if (randomizeSpawnPoints)
                ShuffleSelectedSpawnPoints();

            for (int i = 0; i < _selectedSpawnPoints.Count; i++)
            {
                Transform spawnPoint = _selectedSpawnPoints[i];
                GameObject spawnedWood = Instantiate(woodPrefab, spawnPoint.position, spawnPoint.rotation, spawnedParent);
                _spawnedWood.Add(spawnedWood);
            }

            _lastSpawnedDayNumber = dayNumber;
        }

        public void ClearSpawnedWood()
        {
            for (int i = _spawnedWood.Count - 1; i >= 0; i--)
            {
                if (_spawnedWood[i] != null)
                    Destroy(_spawnedWood[i]);
            }

            _spawnedWood.Clear();
        }

        private void RefreshSpawnPoints()
        {
            _spawnPoints.Clear();

            for (int i = 0; i < spawnPointsRoot.childCount; i++)
                _spawnPoints.Add(spawnPointsRoot.GetChild(i));

            _spawnPoints.Sort((left, right) => GetPointNumber(left).CompareTo(GetPointNumber(right)));
        }

        private void SelectSpawnPointsForDay(int dayNumber, int spawnCount)
        {
            _selectedSpawnPoints.Clear();

            if (spawnCount <= 0)
                return;

            switch (dayNumber)
            {
                case 1:
                    AddPointsInRange(0, dayOneLastPointNumber, spawnCount);
                    AddPointsInRange(dayOneLastPointNumber + 1, dayTwoLastPointNumber, spawnCount);
                    break;

                case 2:
                    AddPointsInRange(dayOneLastPointNumber + 1, dayTwoLastPointNumber, spawnCount);
                    AddPointsInRange(dayTwoLastPointNumber + 1, int.MaxValue, spawnCount);
                    break;

                default:
                    AddPointsInRange(dayTwoLastPointNumber + 1, int.MaxValue, spawnCount);
                    break;
            }
        }

        private void AddPointsInRange(int firstPointNumber, int lastPointNumber, int maxSelectedCount)
        {
            if (_selectedSpawnPoints.Count >= maxSelectedCount)
                return;

            _previewCandidates.Clear();

            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                int pointNumber = GetPointNumber(_spawnPoints[i]);

                if (pointNumber < firstPointNumber || pointNumber > lastPointNumber)
                    continue;

                _previewCandidates.Add(_spawnPoints[i]);
            }

            if (randomizeSpawnPoints)
                Shuffle(_previewCandidates);

            int remainingCount = maxSelectedCount - _selectedSpawnPoints.Count;
            int count = Mathf.Min(remainingCount, _previewCandidates.Count);

            for (int i = 0; i < count; i++)
                _selectedSpawnPoints.Add(_previewCandidates[i]);
        }

        private void ShuffleSelectedSpawnPoints()
        {
            Shuffle(_selectedSpawnPoints);
        }

        private static void Shuffle<T>(IList<T> items)
        {
            for (int i = 0; i < items.Count - 1; i++)
            {
                int randomIndex = Random.Range(i, items.Count);
                (items[i], items[randomIndex]) = (items[randomIndex], items[i]);
            }
        }

        private static int GetPointNumber(Transform spawnPoint)
        {
            string pointName = spawnPoint.name;
            int openIndex = pointName.LastIndexOf('(');
            int closeIndex = pointName.LastIndexOf(')');

            if (openIndex < 0 || closeIndex <= openIndex)
                return 0;

            string numberText = pointName.Substring(openIndex + 1, closeIndex - openIndex - 1);

            return int.TryParse(numberText, out int pointNumber)
                ? pointNumber
                : 0;
        }

        private void OnDayStarted(DayConfig dayConfig)
        {
            if (!spawnOnStart || dayConfig == null)
                return;

            SpawnForDay(dayConfig.DayNumber, dayConfig.WoodSpawnCount);
        }
    }
}
