using UnityEngine;

namespace TinyIsland.Camera
{
    public class IslandOrbitCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform islandCenter;

        [Header("Position")]
        [SerializeField] private float distanceFromPlayer = 6f;
        [SerializeField] private float height = 3f;
        [SerializeField] private float sideOffset = 0f;

        [Header("Orbit")]
        [SerializeField] private Vector3 islandLocalAxis = Vector3.up;

        [Header("Look")]
        [SerializeField] private float lookHeight = 1.2f;
        [SerializeField] private float lookToCenterOffset = 0.5f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.2f;
        [SerializeField] private float rotationSmoothSpeed = 10f;

        private Vector3 _positionVelocity;
        private bool _hasOverride;
        private Vector3 _overridePosition;
        private Quaternion _overrideRotation;

        public void SetOverridePose(Vector3 position, Quaternion rotation)
        {
            _hasOverride = true;
            _overridePosition = position;
            _overrideRotation = rotation;
        }

        public void ClearOverridePose()
        {
            _hasOverride = false;
            _positionVelocity = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (_hasOverride)
            {
                transform.SetPositionAndRotation(_overridePosition, _overrideRotation);
                return;
            }

            if (target == null || islandCenter == null)
                return;

            Vector3 orbitAxis = GetOrbitAxis();
            Vector3 targetFromCenter = target.position - islandCenter.position;
            Vector3 orbitDirection = Vector3.ProjectOnPlane(targetFromCenter, orbitAxis);

            if (orbitDirection.sqrMagnitude < 0.001f)
                return;

            orbitDirection.Normalize();

            Vector3 sideDirection = Vector3.Cross(orbitAxis, orbitDirection).normalized;
            float targetRadius = Vector3.Dot(targetFromCenter, orbitDirection);
            float targetHeight = Vector3.Dot(targetFromCenter, orbitAxis);

            Vector3 desiredPosition =
                islandCenter.position +
                orbitDirection * (targetRadius + distanceFromPlayer) +
                orbitAxis * (targetHeight + height) +
                sideDirection * sideOffset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _positionVelocity,
                positionSmoothTime
            );

            Vector3 lookPoint =
                target.position +
                orbitAxis * lookHeight -
                orbitDirection * lookToCenterOffset;

            Vector3 lookDirection = lookPoint - transform.position;

            if (lookDirection.sqrMagnitude < 0.001f)
                return;

            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, orbitAxis);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
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
    }
}
