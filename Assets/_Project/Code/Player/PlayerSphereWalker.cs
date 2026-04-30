using UnityEngine;
using UnityEngine.Serialization;
using TinyIsland.Input;

namespace TinyIsland.Player
{
    public class PlayerSphereWalker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [FormerlySerializedAs("planetCenter")]
        [SerializeField] private Transform islandCenter;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float alignSpeed = 18f;

        [Header("Orbit")]
        [SerializeField] private Vector3 islandLocalAxis = Vector3.up;

        [Header("Surface")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float surfaceOffset = 1f;
        [SerializeField] private float castHeight = 4f;
        [SerializeField] private float castDistance = 10f;
        [SerializeField] private float castRadius = 0.25f;

        private PlayerInputActions _inputActions;

        private Vector3 _surfaceUp = Vector3.up;
        private Vector3 _lastMoveDirection;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _surfaceUp = transform.up;
            _lastMoveDirection = transform.forward;
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void Start()
        {
            SnapToSurface();
        }

        private void Update()
        {
            MoveOnSurface();
            SnapToSurface();
            AlignToSurface();
        }

        private void MoveOnSurface()
        {
            Vector2 input = _inputActions.Player.Move.ReadValue<Vector2>();

            if (input.sqrMagnitude > 1f)
                input.Normalize();

            if (input.sqrMagnitude < 0.01f)
                return;

            Vector3 orbitDirection = -GetOrbitDirection();
            Vector3 meridianDirection = Vector3.Cross(orbitDirection, _surfaceUp);

            if (meridianDirection.sqrMagnitude < 0.001f)
                return;

            meridianDirection.Normalize();

            Vector3 moveDirection =
                meridianDirection * input.y +
                orbitDirection * input.x;

            moveDirection = Vector3.ProjectOnPlane(moveDirection, _surfaceUp);

            if (moveDirection.sqrMagnitude < 0.001f)
                return;

            moveDirection.Normalize();

            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            _lastMoveDirection = moveDirection;
        }

        private void SnapToSurface()
        {
            Vector3 castOrigin = transform.position + _surfaceUp * castHeight;
            Vector3 castDirection = -_surfaceUp;

            bool hasHit = Physics.SphereCast(
                castOrigin,
                castRadius,
                castDirection,
                out RaycastHit hit,
                castDistance,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (!hasHit)
                return;

            _surfaceUp = hit.normal.normalized;
            transform.position = hit.point + _surfaceUp * surfaceOffset;
        }

        private void AlignToSurface()
        {
            Vector3 forward = Vector3.ProjectOnPlane(_lastMoveDirection, _surfaceUp);

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.ProjectOnPlane(transform.forward, _surfaceUp);

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.Cross(transform.right, _surfaceUp);

            forward.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(forward, _surfaceUp);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                alignSpeed * Time.deltaTime
            );
        }

        private Vector3 GetOrbitDirection()
        {
            Vector3 orbitAxis = GetOrbitAxis();
            Vector3 orbitDirection = Vector3.Cross(orbitAxis, _surfaceUp);

            if (orbitDirection.sqrMagnitude < 0.001f)
                orbitDirection = GetFallbackOrbitDirection();

            return orbitDirection.normalized;
        }

        private Vector3 GetOrbitAxis()
        {
            Vector3 localAxis = islandLocalAxis.sqrMagnitude > 0.001f
                ? islandLocalAxis.normalized
                : Vector3.up;

            return islandCenter != null
                ? islandCenter.TransformDirection(localAxis).normalized
                : localAxis;
        }

        private Vector3 GetFallbackOrbitDirection()
        {
            if (cameraTransform != null)
            {
                Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, _surfaceUp);

                if (cameraRight.sqrMagnitude >= 0.001f)
                    return cameraRight;
            }

            Vector3 right = Vector3.ProjectOnPlane(transform.right, _surfaceUp);

            if (right.sqrMagnitude >= 0.001f)
                return right;

            Vector3 worldRight = Vector3.ProjectOnPlane(Vector3.right, _surfaceUp);

            if (worldRight.sqrMagnitude >= 0.001f)
                return worldRight;

            return Vector3.ProjectOnPlane(Vector3.forward, _surfaceUp);
        }
    }
}
