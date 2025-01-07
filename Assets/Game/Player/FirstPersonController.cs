using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class FirstPersonController : MonoBehaviour
{
    private const float WALK_SPEED = 5f;
    private const float SPRINT_SPEED = 7.5f;
    private const float JUMP_FORCE = 7f;
    private const float ACCELERATION = 7.5f;
    private const float CAPSULE_HEIGHT = 1f;
    private const float CAPSULE_RADIUS = 0.5f;
    private const float CAPSULE_MARGIN = 0.6f;

    public static FirstPersonController Instance { get; private set; }

    public static event UnityAction<PlayerState> OnPlayerStateChanged;
    public PlayerState State { get; private set; }

    [SerializeField] private LayerMask GroundLayer;
    private Transform cameraTransform;

    private float movementSpeed;
    private Vector3 smoothedMoveDirection;
    private bool IsGrounded;
    private float TimeGroundedInSeconds;

    private InputSystem_Actions input;
    public Rigidbody Rigidbody { get; private set; }

    private RaycastHit slopeHit;
    private readonly Collider[] collisionResult = new Collider[1];

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode;
#endif

    private WalkFoleyScript walkFoleyScript;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        movementSpeed = WALK_SPEED;

        Rigidbody = GetComponent<Rigidbody>();

        input = new();
        input.Player.Jump.performed += Jump_performed;
        input.Player.Sprint.started += Sprint_started;
        input.Player.Sprint.canceled += Sprint_canceled;

        walkFoleyScript = GetComponentInChildren<WalkFoleyScript>();

#if UNITY_EDITOR
        if (isDebugMode)
        {
            input.Player.Enable();
        }
#endif
    }

    private void Start()
    {
        cameraTransform = CameraManager.Instance.PlayerCamera.gameObject.transform;
    }

    private void OnDestroy()
    {
        input.Player.Disable();

        input.Player.Jump.performed -= Jump_performed;
        input.Player.Sprint.started -= Sprint_started;
        input.Player.Sprint.canceled -= Sprint_canceled;
    }

    void Update()
    {
        GetMoveDirection();
        RotatePlayerTowardsCamera();
    }

    void FixedUpdate()
    {
        CheckSlope();
        ApplyGravity();

        if (State == PlayerState.FirstPerson)
        {
            ApplyMovement();
        }
    }

    public void SetState(PlayerState _state)
    {
        State = _state;
        OnPlayerStateChanged?.Invoke(State);
        TryEnableInput();
    }

    private void TryEnableInput()
    {
        if (UIManager.Instance.State == UIState.HUD && State == PlayerState.FirstPerson)
        {
            input.Player.Enable();

            if (PlayerBoatController.Instance.AdmiralController.Subordinates.Count > 0)
            {
                TutorialScreen.Instance.ShowInputTooltip(TutorialType.Player, TutorialType.Command);
            }

            else
            {
                TutorialScreen.Instance.ShowInputTooltip(TutorialType.Player);
            }
        }

        else
        {
            input.Player.Disable();
            TutorialScreen.Instance.HideTutorial(TutorialType.Player, TutorialType.Command);
        }
    }

    private void GetMoveDirection()
    {
        Vector2 inputDirection = input.Player.Move.ReadValue<Vector2>().normalized;
        Vector3 moveDirection = transform.TransformDirection(new Vector3(inputDirection.x, 0, inputDirection.y) * movementSpeed);
        smoothedMoveDirection = Vector3.Lerp(smoothedMoveDirection, moveDirection, ACCELERATION * Time.deltaTime);

        TryPlayWalkFoley(inputDirection);
    }

    private void TryPlayWalkFoley(Vector2 inputDirection)
    {
        if (inputDirection.magnitude > 0 && IsGrounded)
        {
            walkFoleyScript.StartWalking();
        }

        else
        {
            walkFoleyScript.StopWalking();
        }
    }

    private void RotatePlayerTowardsCamera()
    {
        Vector3 cameraForward = cameraTransform.transform.forward;
        cameraForward.y = 0f;

        if (cameraForward != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = newRotation;
        }
    }

    private void CheckSlope()
    {
        Physics.Raycast(transform.position, -transform.up, out slopeHit, 2f, GroundLayer, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
        if (isDebugMode)
        {
            Debug.DrawLine(transform.position, transform.position - transform.up, Color.red, Time.fixedDeltaTime);
        }
#endif
    }

    private void ApplyGravity()
    {
        IsGrounded = Physics.CheckBox(transform.position + new Vector3(0, -1.05f, 0), new Vector3(0.5f, 0.05f, 0.5f), Quaternion.identity, GroundLayer, QueryTriggerInteraction.Ignore);
        Vector3 gravity = new(0, Physics.gravity.y * (IsGrounded && (TimeGroundedInSeconds += Time.fixedDeltaTime) > 0.3 ? 2 : 1) * Time.fixedDeltaTime, 0);
        Rigidbody.AddRelativeForce(gravity, ForceMode.VelocityChange);

#if UNITY_EDITOR
        if (isDebugMode)
        {
            DebugUtil.DrawBox(transform.position + new Vector3(0, -1.05f, 0), Quaternion.identity, new Vector3(1, 0.1f, 1), Color.red, Time.fixedDeltaTime);
        }
#endif
    }

    private void ApplyMovement()
    {
        Vector3 moveDirection = smoothedMoveDirection;

        if (IsGrounded)
        {
            moveDirection = Vector3.ProjectOnPlane(smoothedMoveDirection, slopeHit.normal);
        }

        Vector3 half = new(0, CAPSULE_HEIGHT / 2 * CAPSULE_MARGIN, 0);
        Vector3 target = Rigidbody.position + moveDirection * Time.fixedDeltaTime;

        if (Physics.OverlapCapsuleNonAlloc(target - half, target + half, CAPSULE_RADIUS * CAPSULE_MARGIN, collisionResult, GroundLayer, QueryTriggerInteraction.Ignore) == 0)
        {
            Rigidbody.MovePosition(target);
        }
    }

    private void Sprint_started(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        movementSpeed = SPRINT_SPEED;
        walkFoleyScript.maxTimeBetweenFootsteps = walkFoleyScript.maxTimeBetweenFootstepsSprint;
        walkFoleyScript.minTimeBetweenFootsteps = walkFoleyScript.minTimeBetweenFootstepsSprint;
    }

    private void Sprint_canceled(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        movementSpeed = WALK_SPEED;
        walkFoleyScript.maxTimeBetweenFootsteps = 0.6f;
        walkFoleyScript.minTimeBetweenFootsteps = 0.3f;
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
#if UNITY_EDITOR
        if (isDebugMode)
        {
            Rigidbody.AddRelativeForce(0, JUMP_FORCE, 0, ForceMode.VelocityChange);
            TimeGroundedInSeconds = 0;
        }
#endif

        if (IsGrounded)
        {
            Rigidbody.AddRelativeForce(0, JUMP_FORCE, 0, ForceMode.VelocityChange);
            TimeGroundedInSeconds = 0;
        }
    }
}

public enum PlayerState
{
    Inactive,
    FirstPerson,
    SteeringWheel,
    Cannon,
    Throttle,
    Fleet,
    Formation
}