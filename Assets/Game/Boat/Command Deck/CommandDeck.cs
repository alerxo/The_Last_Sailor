using UnityEngine;

public class CommandDeck : MonoBehaviour, IInteractable
{
    public Transform Transform => transform;
    public Vector3 Position => transform.position;
    public bool CanInteract => true;

    public void Interact()
    {
        UIManager.Instance.SetState(UIState.Command);
        FirstPersonController.Instance.SetState(PlayerState.Inactive);
        CameraManager.Instance.SetState(CameraState.Fleet);
    }
}