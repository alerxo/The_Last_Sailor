using UnityEngine;

public interface IInteractable
{
    public void Interact();
    public Vector3 Position { get; }
    public bool CanInteract { get; }
}
