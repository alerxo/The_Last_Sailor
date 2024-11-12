using UnityEngine;

public class SteeringWheel : MonoBehaviour, IInteractable
{
    public Transform rotatingPart;
    public Vector3 Position => transform.position;

    public void Interact()
    {
        CameraManager.Instance.SetState(CameraState.Boat);
    }
}
