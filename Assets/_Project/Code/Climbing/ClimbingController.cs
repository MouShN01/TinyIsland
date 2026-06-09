using TinyIsland.Camera;
using TinyIsland.Core;
using TinyIsland.Hazards;
using TinyIsland.Input;
using TinyIsland.Player;
using TinyIsland.Tower;
using TinyIsland.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TinyIsland.Climbing
{
    public sealed class ClimbingController : MonoBehaviour
    {
        private enum ClimbMode
        {
            None,
            ClimbingUp,
            WaitingOnTop,
            ClimbingDown
        }

        private enum ClimbKey
        {
            W,
            A,
            S,
            D
        }

        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PlayerSphereWalker sphereWalker;
        [SerializeField] private IslandOrbitCamera orbitCamera;

        [Header("Interaction")]
        [SerializeField] private float mountRadius = 1.8f;
        [SerializeField] private float climbDistanceFromTower = 0.85f;
        [SerializeField] private float climbBaseHeightOffset = 0.35f;
        [SerializeField] private float seatedDistanceFromTower = 0f;
        [SerializeField] private float seatedHeightOffset = 0.25f;

        [Header("Crab Push")]
        [SerializeField] private float crabPushRadius = 1.25f;
        [SerializeField] private float crabPushDistance = 1.8f;
        [SerializeField] private float crabPushDuration = 0.28f;
        [SerializeField] private LayerMask crabPushMask = ~0;
        [SerializeField] private int maxCrabPushHits = 8;

        [Header("Rhythm")]
        [SerializeField] private int stepsPerBuiltLevel = 3;
        [SerializeField] private float promptDuration = 1.1f;
        [SerializeField] private float hitWindowStart = 0.4f;
        [SerializeField] private float hitWindowDuration = 0.35f;
        [SerializeField] private float promptGapDuration = 0.15f;
        [SerializeField] private float missPenaltyHeight = 0.15f;

        [Header("Camera")]
        [SerializeField] private float closeCameraDistance = 2.2f;
        [SerializeField] private float closeCameraHeight = 0.75f;
        [SerializeField] private float closeCameraSideOffset = 0.45f;

        [Header("UI")]
        [SerializeField] private RhythmClimbHud rhythmHud;
        [SerializeField] private bool autoCreateRhythmHud = true;

        [Header("Debug Prompt")]
        [SerializeField] private bool drawDebugPrompt = true;
        [SerializeField] private Vector2 promptWindowSize = new Vector2(360f, 150f);
        [SerializeField] private float promptKeyFontSize = 42f;

        private PlayerInputActions _inputActions;
        private PlayerPickupInteractor _pickupInteractor;
        private PlayerTowerBuildInteractor _buildInteractor;
        private Collider[] _crabPushHits;
        private TowerController _currentTower;
        private Vector3 _climbAxis;
        private Vector3 _climbRadialDirection;
        private Vector3 _groundExitPosition;
        private Quaternion _groundExitRotation;
        private ClimbMode _mode;
        private ClimbKey _currentPrompt;
        private float _promptTimer;
        private float _gapTimer;
        private int _stepIndex;
        private int _targetStepCount;
        private int _ascentDayIndex;
        private float _stepHeight;
        private ClimbKey _lastPrompt;
        private bool _hasLastPrompt;
        private bool _sphereWalkerWasEnabled;
        private bool _pickupInteractorWasEnabled;
        private bool _buildInteractorWasEnabled;

        public bool IsClimbing => _mode == ClimbMode.ClimbingUp || _mode == ClimbMode.ClimbingDown;
        public bool IsWaitingOnTop => _mode == ClimbMode.WaitingOnTop;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();

            if (gameManager == null)
                gameManager = FindAnyObjectByType<GameManager>();

            if (sphereWalker == null)
                sphereWalker = GetComponent<PlayerSphereWalker>();

            if (orbitCamera == null)
                orbitCamera = FindAnyObjectByType<IslandOrbitCamera>();

            _pickupInteractor = GetComponent<PlayerPickupInteractor>();
            _buildInteractor = GetComponent<PlayerTowerBuildInteractor>();
            _crabPushHits = new Collider[Mathf.Max(1, maxCrabPushHits)];

            ResolveRhythmHud();
        }

        private void OnEnable()
        {
            _inputActions.Player.Jump.started += OnJumpStarted;
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            if (_mode != ClimbMode.None)
                ExitToGround();

            _inputActions.Player.Jump.started -= OnJumpStarted;
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void Update()
        {
            switch (_mode)
            {
                case ClimbMode.ClimbingUp:
                case ClimbMode.ClimbingDown:
                    UpdateRhythmClimb();
                    break;

                case ClimbMode.WaitingOnTop:
                    ApplyPoseForStep(_targetStepCount);
                    UpdateRhythmHud();

                    if (CanDescend())
                        BeginDescent();
                    break;
            }
        }

        private void OnGUI()
        {
            if (rhythmHud != null || !drawDebugPrompt || _mode == ClimbMode.None)
                return;

            float width = Mathf.Max(260f, promptWindowSize.x);
            float height = Mathf.Max(120f, promptWindowSize.y);
            Rect area = new Rect((Screen.width - width) * 0.5f, Screen.height - height - 32f, width, height);
            GUI.Box(area, string.Empty);

            string text = _mode switch
            {
                ClimbMode.ClimbingUp => $"CLIMB  {GetPromptText()}",
                ClimbMode.ClimbingDown => $"DESCEND  {GetPromptText()}",
                ClimbMode.WaitingOnTop => CanDescend() ? "DESCENDING" : "WAIT FOR LOW TIDE",
                _ => string.Empty
            };

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(promptKeyFontSize),
                fontStyle = FontStyle.Bold
            };

            GUI.Label(new Rect(area.x + 16f, area.y + 18f, width - 32f, 58f), text, labelStyle);

            if (!IsClimbing)
                return;

            float progress = Mathf.Clamp01(_promptTimer / Mathf.Max(0.01f, promptDuration));
            float hitStart = Mathf.Clamp01(hitWindowStart / Mathf.Max(0.01f, promptDuration));
            float hitEnd = Mathf.Clamp01((hitWindowStart + hitWindowDuration) / Mathf.Max(0.01f, promptDuration));
            Rect track = new Rect(area.x + 28f, area.y + height - 46f, width - 56f, 18f);
            GUI.Box(track, string.Empty);
            GUI.Box(new Rect(track.x + track.width * hitStart, track.y, track.width * (hitEnd - hitStart), track.height), string.Empty);
            GUI.Box(new Rect(track.x + track.width * progress - 2f, track.y - 4f, 4f, track.height + 8f), string.Empty);
        }

        private void OnJumpStarted(InputAction.CallbackContext context)
        {
            if (_mode == ClimbMode.None)
            {
                if (TryPushNearbyCrabs())
                    return;

                TryBeginAscent();
                return;
            }

            if (_mode == ClimbMode.WaitingOnTop && CanDescend())
                BeginDescent();
        }

        private void TryBeginAscent()
        {
            TowerController tower = FindNearestBuiltTower();

            if (tower == null)
                return;

            _currentTower = tower;
            _climbAxis = tower.transform.up.sqrMagnitude > 0.001f ? tower.transform.up.normalized : Vector3.up;
            _climbRadialDirection = Vector3.ProjectOnPlane(transform.position - tower.transform.position, _climbAxis);

            if (_climbRadialDirection.sqrMagnitude < 0.001f)
                _climbRadialDirection = -tower.transform.forward;

            _climbRadialDirection.Normalize();
            _groundExitPosition = transform.position;
            _groundExitRotation = transform.rotation;

            float climbHeight = GetCurrentClimbHeight(tower);
            _targetStepCount = Mathf.Max(1, tower.BuiltLevelCount * stepsPerBuiltLevel);
            _stepHeight = climbHeight / _targetStepCount;
            _stepIndex = 0;
            _ascentDayIndex = gameManager != null ? gameManager.CurrentDayIndex : -1;

            DisableGroundControls();
            _mode = ClimbMode.ClimbingUp;
            StartNextPrompt();
            ApplyPoseForStep(_stepIndex);
            UpdateRhythmHud();
        }

        private void BeginDescent()
        {
            if (_currentTower == null)
            {
                ExitToGround();
                return;
            }

            _mode = ClimbMode.ClimbingDown;
            _stepIndex = _targetStepCount;
            StartNextPrompt();
            ApplyPoseForStep(_stepIndex);
            UpdateRhythmHud();
        }

        private void UpdateRhythmClimb()
        {
            if (_currentTower == null)
            {
                ExitToGround();
                return;
            }

            UpdateCloseCamera();
            UpdateRhythmHud();

            if (_gapTimer > 0f)
            {
                _gapTimer -= Time.deltaTime;
                return;
            }

            _promptTimer += Time.deltaTime;

            if (TryReadPressedKey(out ClimbKey pressedKey))
            {
                bool isHit = pressedKey == _currentPrompt && IsInsideHitWindow();

                if (isHit)
                    AdvanceStep();
                else
                    ApplyMissPenalty();

                StartNextPrompt();
                return;
            }

            if (_promptTimer >= promptDuration)
            {
                ApplyMissPenalty();
                StartNextPrompt();
            }
        }

        private void AdvanceStep()
        {
            if (_mode == ClimbMode.ClimbingUp)
            {
                _stepIndex++;

                if (_stepIndex >= _targetStepCount)
                {
                    _stepIndex = _targetStepCount;
                    _mode = ClimbMode.WaitingOnTop;
                }
            }
            else if (_mode == ClimbMode.ClimbingDown)
            {
                _stepIndex--;

                if (_stepIndex <= 0)
                {
                    ExitToGround();
                    return;
                }
            }

            RotateToNextFace();
            ApplyPoseForStep(_stepIndex);
        }

        private void ApplyMissPenalty()
        {
            if (missPenaltyHeight <= 0f)
                return;

            if (_mode == ClimbMode.ClimbingUp)
            {
                float penaltySteps = missPenaltyHeight / Mathf.Max(0.01f, _stepHeight);
                _stepIndex = Mathf.Max(0, Mathf.FloorToInt(_stepIndex - penaltySteps));
            }
            else if (_mode == ClimbMode.ClimbingDown)
            {
                float penaltySteps = missPenaltyHeight / Mathf.Max(0.01f, _stepHeight);
                _stepIndex = Mathf.Min(_targetStepCount, Mathf.CeilToInt(_stepIndex + penaltySteps));
            }

            ApplyPoseForStep(_stepIndex);
        }

        private void StartNextPrompt()
        {
            _currentPrompt = GetRandomPrompt();
            _lastPrompt = _currentPrompt;
            _hasLastPrompt = true;
            _promptTimer = 0f;
            _gapTimer = promptGapDuration;
        }

        private ClimbKey GetRandomPrompt()
        {
            int promptCount = 4;
            int randomIndex = Random.Range(0, promptCount);
            ClimbKey prompt = (ClimbKey)randomIndex;

            if (_hasLastPrompt && prompt == _lastPrompt)
                prompt = (ClimbKey)((randomIndex + 1) % promptCount);

            return prompt;
        }

        private bool IsInsideHitWindow()
        {
            return _promptTimer >= hitWindowStart &&
                   _promptTimer <= hitWindowStart + hitWindowDuration;
        }

        private void RotateToNextFace()
        {
            Quaternion turn = Quaternion.AngleAxis(90f, _climbAxis);
            _climbRadialDirection = (turn * _climbRadialDirection).normalized;
        }

        private void ApplyPoseForStep(int stepIndex)
        {
            if (_currentTower == null)
                return;

            float clampedStepIndex = Mathf.Clamp(stepIndex, 0, _targetStepCount);
            float baseOffsetProgress = _targetStepCount > 0 ? clampedStepIndex / _targetStepCount : 1f;
            float baseOffset = Mathf.Lerp(climbBaseHeightOffset, 0f, baseOffsetProgress);
            float height = baseOffset + clampedStepIndex * _stepHeight;

            if (_mode == ClimbMode.WaitingOnTop)
                height += seatedHeightOffset;

            float distanceFromTower = _mode == ClimbMode.WaitingOnTop
                ? seatedDistanceFromTower
                : climbDistanceFromTower;

            Vector3 basePosition = _currentTower.transform.position;
            Vector3 climbPosition =
                basePosition +
                _climbAxis * height +
                _climbRadialDirection * distanceFromTower;

            Quaternion climbRotation = Quaternion.LookRotation(-_climbRadialDirection, _climbAxis);
            transform.SetPositionAndRotation(climbPosition, climbRotation);
            UpdateCloseCamera();
        }

        private void UpdateCloseCamera()
        {
            if (orbitCamera == null || _currentTower == null)
                return;

            Vector3 side = Vector3.Cross(_climbAxis, _climbRadialDirection).normalized;
            Vector3 cameraPosition =
                transform.position +
                _climbRadialDirection * closeCameraDistance +
                _climbAxis * closeCameraHeight +
                side * closeCameraSideOffset;

            Vector3 lookPoint = transform.position + _climbAxis * 0.25f;
            Quaternion cameraRotation = Quaternion.LookRotation((lookPoint - cameraPosition).normalized, _climbAxis);
            orbitCamera.SetOverridePose(cameraPosition, cameraRotation);
        }

        private void ExitToGround()
        {
            _mode = ClimbMode.None;
            _currentTower = null;
            transform.SetPositionAndRotation(_groundExitPosition, _groundExitRotation);
            RestoreGroundControls();
            UpdateRhythmHud();

            if (orbitCamera != null)
                orbitCamera.ClearOverridePose();
        }

        private void ResolveRhythmHud()
        {
            if (rhythmHud == null)
                rhythmHud = FindAnyObjectByType<RhythmClimbHud>(FindObjectsInactive.Include);

            if (rhythmHud == null && autoCreateRhythmHud)
                rhythmHud = RhythmClimbHud.CreateDefault();

            if (rhythmHud != null)
                rhythmHud.Hide();
        }

        private void UpdateRhythmHud()
        {
            if (rhythmHud == null)
                return;

            if (_mode == ClimbMode.None)
            {
                rhythmHud.Hide();
                return;
            }

            if (_mode == ClimbMode.WaitingOnTop)
            {
                rhythmHud.ShowWaiting(CanDescend() ? "DESCENDING" : "WAIT FOR LOW TIDE");
                return;
            }

            float duration = Mathf.Max(0.01f, promptDuration);
            float progress = _gapTimer > 0f ? 0f : Mathf.Clamp01(_promptTimer / duration);
            float hitStart = Mathf.Clamp01(hitWindowStart / duration);
            float hitEnd = Mathf.Clamp01((hitWindowStart + hitWindowDuration) / duration);
            string actionLabel = _mode == ClimbMode.ClimbingDown ? "DESCEND" : "CLIMB";

            rhythmHud.ShowClimbPrompt(
                actionLabel,
                GetPromptText(),
                progress,
                hitStart,
                hitEnd,
                _stepIndex,
                _targetStepCount
            );
        }

        private void DisableGroundControls()
        {
            _sphereWalkerWasEnabled = sphereWalker != null && sphereWalker.enabled;
            _pickupInteractorWasEnabled = _pickupInteractor != null && _pickupInteractor.enabled;
            _buildInteractorWasEnabled = _buildInteractor != null && _buildInteractor.enabled;

            if (sphereWalker != null)
                sphereWalker.enabled = false;

            if (_pickupInteractor != null)
                _pickupInteractor.enabled = false;

            if (_buildInteractor != null)
                _buildInteractor.enabled = false;
        }

        private void RestoreGroundControls()
        {
            if (sphereWalker != null)
                sphereWalker.enabled = _sphereWalkerWasEnabled;

            if (_pickupInteractor != null)
                _pickupInteractor.enabled = _pickupInteractorWasEnabled;

            if (_buildInteractor != null)
                _buildInteractor.enabled = _buildInteractorWasEnabled;
        }

        private TowerController FindNearestBuiltTower()
        {
            TowerController[] towers = FindObjectsByType<TowerController>(FindObjectsInactive.Exclude);
            TowerController nearestTower = null;
            float nearestDistance = mountRadius * mountRadius;

            for (int i = 0; i < towers.Length; i++)
            {
                if (towers[i] == null || !towers[i].CanClimbForCurrentDay)
                    continue;

                float distance = Vector3.SqrMagnitude(towers[i].transform.position - transform.position);

                if (distance > nearestDistance)
                    continue;

                nearestDistance = distance;
                nearestTower = towers[i];
            }

            return nearestTower;
        }

        private float GetCurrentClimbHeight(TowerController tower)
        {
            float builtHeight = tower.GetBuiltTopWorldY() - tower.transform.position.y;
            return Mathf.Max(0.6f, builtHeight);
        }

        private bool CanDescend()
        {
            if (gameManager == null)
                return true;

            return IsLowTideState() && gameManager.CurrentDayIndex > _ascentDayIndex;
        }

        private bool IsLowTideState()
        {
            return gameManager != null &&
                   gameManager.State == GameState.LowTide;
        }

        private string GetPromptText()
        {
            return _gapTimer > 0f ? string.Empty : _currentPrompt.ToString();
        }

        private bool TryPushNearbyCrabs()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                crabPushRadius,
                _crabPushHits,
                crabPushMask,
                QueryTriggerInteraction.Collide
            );

            bool pushedAnyCrab = false;

            for (int i = 0; i < hitCount; i++)
            {
                CrabController crab = GetCrabController(_crabPushHits[i]);

                if (crab == null)
                    continue;

                crab.PushAwayFrom(transform.position, crabPushDistance, crabPushDuration);
                pushedAnyCrab = true;
            }

            return pushedAnyCrab;
        }

        private static CrabController GetCrabController(Collider hit)
        {
            return hit != null ? hit.GetComponentInParent<CrabController>() : null;
        }

        private static bool TryReadPressedKey(out ClimbKey key)
        {
            key = default;

            if (Keyboard.current == null)
                return false;

            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                key = ClimbKey.W;
                return true;
            }

            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                key = ClimbKey.A;
                return true;
            }

            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                key = ClimbKey.S;
                return true;
            }

            if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                key = ClimbKey.D;
                return true;
            }

            return false;
        }
    }
}
