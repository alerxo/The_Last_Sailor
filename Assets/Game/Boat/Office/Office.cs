using System.Collections.Generic;
using UnityEngine;

public class Office : MonoBehaviour, IInteractable
{
    public Transform Transform => transform;
    public Vector3 Position => transform.position;
    public bool CanInteract => true;

    [SerializeField] private Transform[] deskMeshes;
    public Renderer[] GetRenderers => renderers;
    private Renderer[] renderers;

    private void Awake()
    {
        List<Renderer> list = new();

        foreach(Transform t in deskMeshes)
        {
            list.AddRange(t.GetComponentsInChildren<Renderer>());
        }

        renderers = list.ToArray();
    }

    public void Interact()
    {
        UIManager.Instance.SetState(UIState.Fleet);
        FirstPersonController.Instance.SetState(PlayerState.Fleet);
    }
}