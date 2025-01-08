using UnityEngine;

public interface IInteractable
{
    public void Interact();
    public Transform Transform { get; }
    public Vector3 Position { get; }
    public bool CanInteract { get; }

    public Renderer[] GetRenderers { get; }
}
