using TinyIsland.Player;
using UnityEngine;

namespace TinyIsland.Hazards
{
    public sealed class CrabController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform islandCenter;

        [Header("Detection")]
        [SerializeField] private float sightRadius = 4f;
        [SerializeField] private float fieldOfViewAngle = 140f;
        [SerializeField] private float closeAwarenessRadius = 1.35f;
        [SerializeField] private float loseSightRadius = 5.5f;

        [Header("Movement")]
        [SerializeField] private float wanderSpeed = 0.65f;
        [SerializeField] private float chaseSpeed = 2.35f;
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float wanderDirectionInterval = 1.5f;
        [SerializeField] private float wanderJitterAngle = 75f;

        [Header("Push")]
        [SerializeField] private float pushedStunDuration = 0.12f;

        [Header("Surface")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float surfaceOffset = 0.2f;
        [SerializeField] private float castHeight = 3f;
        [SerializeField] private float castDistance = 8f;
        [SerializeField] private float castRadius = 0.18f;

        private Vector3 _surfaceUp = Vector3.up;
        private Vector3 _moveDirection;
        private Vector3 _lastMoveDirection;
        private Vector3 _pushDirection;
        private float _pushSpeed;
        private float _pushTimer;
        private float _stunTimer;
        private float _wanderTimer;
        private bool _isChasing;

        private void Awake()
        {
            if (target == null)
            {
                PlayerSphereWalker player = FindAnyObjectByType<PlayerSphereWalker>();
                if (player != null)
                    target = player.transform;
            }

            if (islandCenter == null)
                islandCenter = FindIslandCenter();

            _surfaceUp = transform.up;
            _lastMoveDirection = transform.forward;
            PickWanderDirection();
        }

        private void Start()
        {
            SnapToSurface();
        }

        private void Update()
        {
            if (_pushTimer > 0f)
            {
                _pushTimer -= Time.deltaTime;
                MoveOnSurface(_pushDirection, _pushSpeed);
                SnapToSurface();
                AlignToSurface();
                return;
            }

            if (_stunTimer > 0f)
                _stunTimer -= Time.deltaTime;

            UpdateState();

            Vector3 desiredDirection = _isChasing
                ? GetDirectionToTarget()
                : GetWanderDirection();

            MoveOnSurface(desiredDirection, _isChasing ? chaseSpeed : wanderSpeed);
            SnapToSurface();
            AlignToSurface();
        }

        public void Initialize(Transform targetTransform, Transform islandCenterTransform)
        {
            target = targetTransform;
            islandCenter = islandCenterTransform;
        }

        public void PushAwayFrom(Vector3 sourcePosition, float distance, float duration)
        {
            Vector3 direction = Vector3.ProjectOnPlane(transform.position - sourcePosition, _surfaceUp);

            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.ProjectOnPlane(transform.position - GetCenterPosition(), _surfaceUp);

            if (direction.sqrMagnitude < 0.001f)
                direction = -_lastMoveDirection;

            _pushDirection = direction.normalized;
            _pushSpeed = Mathf.Max(0f, distance) / Mathf.Max(0.01f, duration);
            _pushTimer = Mathf.Max(0.01f, duration);
            _stunTimer = pushedStunDuration;
            _isChasing = false;
            _moveDirection = _pushDirection;
            _lastMoveDirection = _pushDirection;
        }

        private void UpdateState()
        {
            if (target == null)
            {
                _isChasing = false;
                return;
            }

            if (_stunTimer > 0f)
            {
                _isChasing = false;
                return;
            }

            Vector3 toTarget = Vector3.ProjectOnPlane(target.position - transform.position, _surfaceUp);
            float sqrDistance = toTarget.sqrMagnitude;

            if (_isChasing)
            {
                if (sqrDistance > loseSightRadius * loseSightRadius)
                    _isChasing = false;

                return;
            }

            if (sqrDistance > sightRadius * sightRadius || sqrDistance < 0.001f)
                return;

            if (sqrDistance <= closeAwarenessRadius * closeAwarenessRadius)
            {
                _isChasing = true;
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, _surfaceUp);

            if (forward.sqrMagnitude < 0.001f)
                forward = _lastMoveDirection;

            float angle = Vector3.Angle(forward.normalized, toTarget.normalized);

            if (angle <= fieldOfViewAngle * 0.5f)
                _isChasing = true;
        }

        private Vector3 GetDirectionToTarget()
        {
            if (target == null)
                return GetWanderDirection();

            Vector3 direction = Vector3.ProjectOnPlane(target.position - transform.position, _surfaceUp);

            if (direction.sqrMagnitude < 0.001f)
                return _lastMoveDirection;

            return direction.normalized;
        }

        private Vector3 GetWanderDirection()
        {
            _wanderTimer -= Time.deltaTime;

            if (_wanderTimer <= 0f || _moveDirection.sqrMagnitude < 0.001f)
                PickWanderDirection();

            return _moveDirection;
        }

        private void PickWanderDirection()
        {
            Vector3 centerDirection = GetDirectionAwayFromCenter();
            Vector3 tangent = Vector3.Cross(_surfaceUp, centerDirection);

            if (tangent.sqrMagnitude < 0.001f)
                tangent = Vector3.ProjectOnPlane(transform.forward, _surfaceUp);

            if (tangent.sqrMagnitude < 0.001f)
                tangent = Vector3.ProjectOnPlane(Vector3.forward, _surfaceUp);

            float angle = Random.Range(-wanderJitterAngle, wanderJitterAngle);
            _moveDirection = (Quaternion.AngleAxis(angle, _surfaceUp) * tangent).normalized;
            _wanderTimer = Mathf.Max(0.1f, wanderDirectionInterval);
        }

        private Vector3 GetDirectionAwayFromCenter()
        {
            if (islandCenter == null)
                return Vector3.ProjectOnPlane(transform.position, _surfaceUp).normalized;

            Vector3 direction = Vector3.ProjectOnPlane(transform.position - islandCenter.position, _surfaceUp);

            if (direction.sqrMagnitude < 0.001f)
                return Vector3.ProjectOnPlane(transform.forward, _surfaceUp).normalized;

            return direction.normalized;
        }

        private Vector3 GetCenterPosition()
        {
            return islandCenter != null ? islandCenter.position : Vector3.zero;
        }

        private void MoveOnSurface(Vector3 desiredDirection, float speed)
        {
            desiredDirection = Vector3.ProjectOnPlane(desiredDirection, _surfaceUp);

            if (desiredDirection.sqrMagnitude < 0.001f)
                return;

            desiredDirection.Normalize();
            transform.position += desiredDirection * speed * Time.deltaTime;
            _lastMoveDirection = desiredDirection;
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
            {
                PickWanderDirection();
                return;
            }

            _surfaceUp = hit.normal.normalized;
            transform.position = hit.point + _surfaceUp * surfaceOffset;
        }

        private void AlignToSurface()
        {
            Vector3 forward = Vector3.ProjectOnPlane(_lastMoveDirection, _surfaceUp);

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.ProjectOnPlane(transform.forward, _surfaceUp);

            if (forward.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(forward.normalized, _surfaceUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        private static Transform FindIslandCenter()
        {
            GameObject island = GameObject.Find("Island_SandDome");
            return island != null ? island.transform : null;
        }
    }
}
