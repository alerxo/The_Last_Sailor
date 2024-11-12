using UnityEngine;

public class SteeringWheel : MonoBehaviour, IInteractable
{
    [Tooltip("The rotating part of the mesh")]
    public Transform rotatingPart;
    public Vector3 Position => transform.position;

    public void Interact()
    {
        CameraManager.Instance.SetState(CameraState.Boat);
    }
}
