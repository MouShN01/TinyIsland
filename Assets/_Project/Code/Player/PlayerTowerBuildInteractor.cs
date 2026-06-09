using TinyIsland.Input;
using TinyIsland.Tower;
using TinyIsland.UI;
using UnityEngine;

namespace TinyIsland.Player
{
    public sealed class PlayerTowerBuildInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWoodInventory woodInventory;
        [SerializeField] private PlayerBuildHoldIndicator buildHoldIndicator;

        [Header("Build")]
        [SerializeField] private float buildRadius = 1.75f;
        [SerializeField] private float buildHoldDuration = 0.9f;
        [SerializeField] private bool autoCreateBuildHoldIndicator = true;

        private PlayerInputActions _inputActions;
        private TowerController _currentTower;
        private float _holdTimer;
        private bool _mustReleaseInteract;
        private bool _ownsBuildHoldIndicator;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();

            if (woodInventory == null)
                woodInventory = GetComponent<PlayerWoodInventory>();

            ResolveBuildHoldIndicator();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
            ResetHoldProgress();

            if (buildHoldIndicator != null)
                buildHoldIndicator.Hide();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();

            if (_ownsBuildHoldIndicator && buildHoldIndicator != null)
                Destroy(buildHoldIndicator.gameObject);
        }

        private void Update()
        {
            _currentTower = FindNearestBuildTarget();

            if (_currentTower == null)
            {
                ResetHoldProgress();

                if (buildHoldIndicator != null)
                    buildHoldIndicator.Hide();

                return;
            }

            bool canBuild = _currentTower.CanBuild(woodInventory);
            bool isInteractHeld = _inputActions.Player.Interact.IsPressed();

            if (!isInteractHeld)
            {
                _mustReleaseInteract = false;
                ResetHoldProgress();
                ShowBuildHoldIndicator(0f, canBuild);
                return;
            }

            if (_mustReleaseInteract || !canBuild)
            {
                ShowBuildHoldIndicator(0f, canBuild);
                return;
            }

            _holdTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_holdTimer / Mathf.Max(0.01f, buildHoldDuration));
            ShowBuildHoldIndicator(progress, true);

            if (progress < 1f)
                return;

            if (_currentTower.TryBuildNextPart(woodInventory))
                _mustReleaseInteract = true;

            ResetHoldProgress();
            ShowBuildHoldIndicator(0f, _currentTower != null && _currentTower.CanBuild(woodInventory));
        }

        private void ResolveBuildHoldIndicator()
        {
            if (buildHoldIndicator == null)
                buildHoldIndicator = GetComponentInChildren<PlayerBuildHoldIndicator>(true);

            if (buildHoldIndicator == null && autoCreateBuildHoldIndicator)
            {
                buildHoldIndicator = PlayerBuildHoldIndicator.CreateForTarget(transform);
                _ownsBuildHoldIndicator = true;
            }

            if (buildHoldIndicator != null)
            {
                buildHoldIndicator.SetTarget(transform);
                buildHoldIndicator.Hide();
            }
        }

        private void ShowBuildHoldIndicator(float progress, bool canBuild)
        {
            if (buildHoldIndicator == null)
                return;

            buildHoldIndicator.Show(progress, canBuild);
        }

        private void ResetHoldProgress()
        {
            _holdTimer = 0f;
        }

        private TowerController FindNearestBuildTarget()
        {
            TowerController[] towers = FindObjectsByType<TowerController>(FindObjectsInactive.Exclude);
            TowerController nearestTower = null;
            float nearestDistance = buildRadius * buildRadius;

            for (int i = 0; i < towers.Length; i++)
            {
                if (towers[i] == null || !towers[i].CanBuildNextPartToday)
                    continue;

                float distance = (towers[i].transform.position - transform.position).sqrMagnitude;

                if (distance > nearestDistance)
                    continue;

                nearestDistance = distance;
                nearestTower = towers[i];
            }

            return nearestTower;
        }
    }
}
