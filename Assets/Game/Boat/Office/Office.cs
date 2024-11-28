using UnityEngine;

public class Office : MonoBehaviour, IInteractable
{
    public Transform Transform => transform;
    public Vector3 Position => transform.position;
    public bool CanInteract => CombatManager.Instance.AdmiralInCombat == null;

    private void Awake()
    {
        CombatManager.OnAdmiralInCombatChanged += CombatManager_OnAdmiralInCombatChanged;
    }

    private void OnDestroy()
    {
        CombatManager.OnAdmiralInCombatChanged -= CombatManager_OnAdmiralInCombatChanged;
    }

    private void CombatManager_OnAdmiralInCombatChanged(Admiral _admiral)
    {
        if (_admiral != null && UIManager.Instance.State == UIState.Fleet)
        {
            UIManager.Instance.SetState(UIState.HUD);
            CameraManager.Instance.SetState(CameraState.Player);
            FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
        }
    }

    public void Interact()
    {
        UIManager.Instance.SetState(UIState.Fleet);
        CameraManager.Instance.SetState(CameraState.Fleet);
        FirstPersonController.Instance.SetState(PlayerState.Fleet);
    }
}