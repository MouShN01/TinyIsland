using TinyIsland.Input;
using TinyIsland.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TinyIsland.Player
{
    public sealed class PlayerPickupInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWoodInventory woodInventory;

        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 1.5f;
        [SerializeField] private LayerMask pickupMask = ~0;
        [SerializeField] private int maxHits = 16;

        private PlayerInputActions _inputActions;
        private Collider[] _hits;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();

            if (woodInventory == null)
                woodInventory = GetComponent<PlayerWoodInventory>();

            _hits = new Collider[Mathf.Max(1, maxHits)];
        }

        private void OnEnable()
        {
            _inputActions.Player.Interact.started += OnInteract;
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Interact.started -= OnInteract;
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            IPickupable pickupable = FindNearestPickupable();

            if (pickupable == null)
                return;

            PickupContext pickupContext = new PickupContext(gameObject, woodInventory);

            if (!pickupable.CanPickup(pickupContext))
                return;

            pickupable.Pickup(pickupContext);
        }

        private IPickupable FindNearestPickupable()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                interactionRadius,
                _hits,
                pickupMask,
                QueryTriggerInteraction.Collide
            );

            IPickupable nearestPickupable = null;
            float nearestDistance = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                IPickupable pickupable = GetPickupable(_hits[i]);

                if (pickupable == null)
                    continue;

                float distance = (pickupable.PickupTransform.position - transform.position).sqrMagnitude;

                if (distance >= nearestDistance)
                    continue;

                nearestDistance = distance;
                nearestPickupable = pickupable;
            }

            return nearestPickupable ?? FindNearestPickupableWithoutCollider();
        }

        private static IPickupable GetPickupable(Collider hit)
        {
            if (hit == null)
                return null;

            MonoBehaviour[] behaviours = hit.GetComponentsInParent<MonoBehaviour>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IPickupable pickupable)
                    return pickupable;
            }

            return null;
        }

        private IPickupable FindNearestPickupableWithoutCollider()
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);
            IPickupable nearestPickupable = null;
            float nearestDistance = interactionRadius * interactionRadius;

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IPickupable pickupable)
                    continue;

                float distance = (pickupable.PickupTransform.position - transform.position).sqrMagnitude;

                if (distance > nearestDistance)
                    continue;

                nearestDistance = distance;
                nearestPickupable = pickupable;
            }

            return nearestPickupable;
        }
    }
}
