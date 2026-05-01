using TinyIsland.Input;
using TinyIsland.Tower;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TinyIsland.Player
{
    public sealed class PlayerTowerBuildInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWoodInventory woodInventory;

        [Header("Build")]
        [SerializeField] private float buildRadius = 1.75f;

        private PlayerInputActions _inputActions;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();

            if (woodInventory == null)
                woodInventory = GetComponent<PlayerWoodInventory>();
        }

        private void OnEnable()
        {
            _inputActions.Player.Interact.performed += OnInteractPerformed;
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Interact.performed -= OnInteractPerformed;
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            TowerController tower = FindNearestTower();

            if (tower == null)
                return;

            tower.TryBuildNextPart(woodInventory);
        }

        private TowerController FindNearestTower()
        {
            TowerController[] towers = FindObjectsByType<TowerController>(FindObjectsInactive.Exclude);
            TowerController nearestTower = null;
            float nearestDistance = buildRadius * buildRadius;

            for (int i = 0; i < towers.Length; i++)
            {
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
