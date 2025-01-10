using System.Collections.Generic;
using UnityEngine;

public class Office : MonoBehaviour, IInteractable
{
    public Transform Transform => transform;
    public Vector3 Position => transform.position;
    public bool CanInteract => PlayerBoatController.Instance.AdmiralController.Enemy == null;

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

        CombatManager.OnAdmiralInCombatChanged += CombatManager_OnAdmiralInCombatChanged;
    }

    private void OnDestroy()
    {
        CombatManager.OnAdmiralInCombatChanged -= CombatManager_OnAdmiralInCombatChanged;
    }

    private void CombatManager_OnAdmiralInCombatChanged(Admiral _admiral)
    {
        if (_admiral != null && UIManager.Instance.GetState() == UIState.Fleet)
        {
            UIManager.Instance.SetState(UIState.HUD);
            CameraManager.Instance.SetState(CameraState.Player);
            FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
        }
    }

    public void Interact()
    {
        UIManager.Instance.SetState(UIState.Fleet);
        FirstPersonController.Instance.SetState(PlayerState.Fleet);
    }
}