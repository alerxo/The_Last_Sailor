using Unity.Mathematics;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    private InputSystem_Actions input;

    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float walkSpeed, sprintSpeed, jumpForce, acceleration;
    [SerializeField] private Transform cameraTransform;

    private float movementSpeed;
    private Vector3 smoothedMoveDirection;
    private bool IsGrounded;
    private float TimeGroundedInSeconds;

    private Rigidbody rb;

    private void Awake()
    {
        movementSpeed = walkSpeed;

        rb = GetComponent<Rigidbody>();

        input = new();
        input.Player.Jump.performed += Jump_performed;
        input.Player.Sprint.started += Sprint_started;
        input.Player.Sprint.canceled += Sprint_canceled;

        CameraManager.OnStateChanged += CameraManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.Jump.performed -= Jump_performed;
        input.Player.Sprint.started -= Sprint_started;
        input.Player.Sprint.canceled -= Sprint_canceled;

        CameraManager.OnStateChanged += CameraManager_OnStateChanged;
    }

    void Update()
    {
        GetMoveDirection();
        ApplyGravity();
        RotatePlayerTowardsCamera();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void CameraManager_OnStateChanged(CameraState _state)
    {
        if (_state == CameraState.Player) input.Player.Enable();
        else input.Player.Disable();
    }

    private void ApplyMovement()
    {
        rb.MovePosition(rb.position + smoothedMoveDirection * Time.fixedDeltaTime);
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

    private void GetMoveDirection()
    {
        Vector2 inputDirection = input.Player.Move.ReadValue<Vector2>().normalized;
        Vector3 moveDirection = transform.TransformDirection(new Vector3(inputDirection.x, 0, inputDirection.y) * movementSpeed);
        smoothedMoveDirection = Vector3.Lerp(smoothedMoveDirection, moveDirection, acceleration * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        IsGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, GroundLayer.value);

        if (IsGrounded && (TimeGroundedInSeconds += Time.deltaTime) > 0.3)
        {
            rb.AddRelativeForce(0, -15f * Time.deltaTime, 0, ForceMode.VelocityChange);
        }
    }

    private void Sprint_started(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        movementSpeed = sprintSpeed;
    }

    private void Sprint_canceled(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        movementSpeed = walkSpeed;
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        if (IsGrounded)
        {
            rb.AddRelativeForce(0, jumpForce, 0, ForceMode.VelocityChange);
            TimeGroundedInSeconds = 0;
        }
    }
}