using UnityEngine;

public class CommandDeck : MonoBehaviour, IInteractable
{
    public Transform Transform => transform;
    public Vector3 Position => transform.position;
    public bool CanInteract => true;

    public void Interact() { }
}