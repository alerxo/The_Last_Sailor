using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class InteractionCollider : MonoBehaviour
{
    public static event UnityAction<IInteractable> OnInteractableChanged;

    private const int LOOK_ANGLE = 45;
    private readonly List<IInteractable> interactablesInRange = new();
    private IInteractable current;
    private FirstPersonController player;

    private InputSystem_Actions input;

    private void Awake()
    {
        player = GetComponent<FirstPersonController>();

        input = new();
        input.Player.Enable();
        input.Player.Interact.performed += Interact_performed;

        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void OnDestroy()
    {
        input.Player.Disable();
        input.Player.Interact.performed -= Interact_performed;

        FirstPersonController.OnPlayerStateChanged += FirstPersonController_OnPlayerStateChanged;
    }

    private void Update()
    {
        if (interactablesInRange.Count == 0 || CameraManager.Instance.State != CameraState.Player)
        {
            if (current != null)
            {
                current = null;
                OnInteractableChanged?.Invoke(current);
            }

            return;
        }

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < interactablesInRange.Count; i++)
        {
            if (!interactablesInRange[i].CanInteract || !IsPlayerFacing(interactablesInRange[i]))
            {
                continue;
            }

            if (closest == null)
            {
                closest = interactablesInRange[i];
                closestDistance = GetDistance(interactablesInRange[i]);

                continue;
            }

            float distance = GetDistance(interactablesInRange[i]);

            if (distance < closestDistance)
            {
                current = interactablesInRange[i];
                closestDistance = distance;
            }
        }

        if (closest != current)
        {
            current = closest;
            OnInteractableChanged?.Invoke(current);
        }
    }

    private bool IsPlayerFacing(IInteractable interactable)
    {
        return Vector3.Angle(interactable.Position - player.transform.position, player.transform.forward) < LOOK_ANGLE;
    }

    private float GetDistance(IInteractable interactable)
    {
        return Vector3.Distance(transform.position, interactable.Position);
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.TryGetComponent(out IInteractable interactable))
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.gameObject.TryGetComponent(out IInteractable interactable))
        {
            interactablesInRange.Remove(interactable);
        }
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        switch (FirstPersonController.instance.State)
        {
            case PlayerState.FirstPerson:
                current?.Interact();
                break;

            case PlayerState.Cannon:
            case PlayerState.SteeringWheel:
            case PlayerState.Throttle:
                CameraManager.Instance.SetState(CameraState.Player);
                FirstPersonController.instance.SetState(PlayerState.FirstPerson);
                break;
        }
    }

    private void FirstPersonController_OnPlayerStateChanged(PlayerState _state)
    {
        if (_state != PlayerState.Inactive) input.Player.Enable();
        else input.Player.Disable();
    }
}
