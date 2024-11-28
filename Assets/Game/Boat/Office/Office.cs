using UnityEngine;

public class Office : MonoBehaviour, IInteractable
{
    public Transform Transform => transform;
    public Vector3 Position => transform.position;
    public bool CanInteract => true;

    public void Interact()
    {
        UIManager.Instance.SetState(UIState.Fleet);
        CameraManager.Instance.SetState(CameraState.Fleet);
        FirstPersonController.Instance.SetState(PlayerState.Inactive);
    }
}