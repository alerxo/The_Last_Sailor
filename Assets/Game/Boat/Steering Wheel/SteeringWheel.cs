using Unity.VisualScripting;
using UnityEngine;

public class SteeringWheel : MonoBehaviour, IInteractable
{
    private const float MAX_ROTATION = 170;

    public Vector3 Position => transform.position;

    [Tooltip("The rotating part of the mesh")]
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private AudioClip audioClip;

    private InputSystem_Actions input;
    private Boat Boat;
    private AudioSource audioSource;
    private bool allowedPlaying;

    private void Awake()
    {
        Boat = GetComponentInParent<Boat>();

        input = new InputSystem_Actions();

        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.Disable();
        FirstPersonController.OnPlayerStateChanged -= FirstPersonController_OnPlayerStateChanged;
    }

    private void Update()
    {
        if (input.Player.Move.ReadValue<Vector2>().x != 0)
        {
            Boat.Engine.ChangeRudder(input.Player.Move.ReadValue<Vector2>().x);
        }
        PlayBellSound();
    }
    public void SetRotation(float rotation)
    {
        rotatingPart.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Lerp(-MAX_ROTATION, MAX_ROTATION, (rotation + 1) / 2)));
    }
    private void PlayBellSound()
    {
        if (rotatingPart.localRotation.eulerAngles.z <= 20 && rotatingPart.localRotation.eulerAngles.z >= -20 && allowedPlaying == true)
        {
            allowedPlaying = false;
            audioSource.PlayOneShot(audioClip);
            Debug.Log("Works");
        }

        if (rotatingPart.localRotation.eulerAngles.z > 20 || rotatingPart.localRotation.eulerAngles.z < -20) 
        {
            allowedPlaying = true;   
        }
    }

    public void Interact()
    {
        CameraManager.Instance.SetState(CameraState.SteeringWheel);
        FirstPersonController.instance.SetState(PlayerState.SteeringWheel);
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        if (_state == PlayerState.SteeringWheel) input.Player.Enable();
        else input.Player.Disable();
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        allowedPlaying = false;
    }
}
