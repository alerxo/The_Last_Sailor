using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class FirstPersonController : MonoBehaviour
{
    private const float WALK_SPEED = 6f;
    private const float SPRINT_SPEED = 10f;
    private const float JUMP_FORCE = 5f;
    private const float ACCELERATION = 7f;
    private const float CAPSULE_HEIGHT = 1f;
    private const float CAPSULE_RADIUS = 0.5f;
    private const float CAPSULE_MARGIN = 0.6f;

    private WalkFoleyScript walkFoleyScript;

    public static FirstPersonController instance;

    public static event UnityAction<PlayerState> OnPlayerStateChanged;
    public PlayerState State { get; private set; }

    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private Transform cameraTransform;

    private float movementSpeed;
    private Vector3 smoothedMoveDirection;
    private bool IsGrounded;
    private float TimeGroundedInSeconds;
    private bool canWalk;
    private bool isStandingStill;


    private InputSystem_Actions input;
    private Rigidbody rb;

    private RaycastHit slopeHit;
    private readonly Collider[] collisionResult = new Collider[1];

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode;
#endif

    private void Awake()
    {
        Assert.IsNull(instance);
        instance = this;
        canWalk = false;
        isStandingStill = true;

        movementSpeed = WALK_SPEED;

        rb = GetComponent<Rigidbody>();

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
        if (State == PlayerState.FirstPerson && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && IsGrounded)
        {
            canWalk = true;
        }
        else {
            canWalk = false;
        }
        if (canWalk)
        {
            walkFoleyScript.StartWalking();
        }
        else
        {
            walkFoleyScript.StopWalking();
        }
    }

    void FixedUpdate()
    {
        CheckSlope();
        ApplyGravity();
        ApplyMovement();
    }

    public void SetState(PlayerState _state)
    {
        State = _state;
        OnPlayerStateChanged?.Invoke(State);
        EnableInput();
    }

    private void EnableInput()
    {
        if (UIManager.Instance.State == UIState.HUD && State == PlayerState.FirstPerson) input.Player.Enable();
        else input.Player.Disable();
    }

    private void GetMoveDirection()
    {
        Vector2 inputDirection = input.Player.Move.ReadValue<Vector2>().normalized;
        Vector3 moveDirection = transform.TransformDirection(new Vector3(inputDirection.x, 0, inputDirection.y) * movementSpeed);
        smoothedMoveDirection = Vector3.Lerp(smoothedMoveDirection, moveDirection, ACCELERATION * Time.deltaTime);
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
        rb.AddRelativeForce(gravity, ForceMode.VelocityChange);

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
        Vector3 target = rb.position + moveDirection * Time.fixedDeltaTime;

        if (Physics.OverlapCapsuleNonAlloc(target - half, target + half, CAPSULE_RADIUS * CAPSULE_MARGIN, collisionResult, GroundLayer, QueryTriggerInteraction.Ignore) == 0)
        {
            rb.MovePosition(target);
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
            rb.AddRelativeForce(0, JUMP_FORCE, 0, ForceMode.VelocityChange);
            TimeGroundedInSeconds = 0;
        }
#endif

        if (IsGrounded)
        {
            rb.AddRelativeForce(0, JUMP_FORCE, 0, ForceMode.VelocityChange);
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
    Throttle
}