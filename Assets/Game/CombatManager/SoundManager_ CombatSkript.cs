using UnityEngine;

public class SoundManager_CombatSkript : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningClip;
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
        if (_admiral != null)
        {
            this.transform.position = _admiral.transform.position + new Vector3 (0,80,0);
            audioSource.PlayOneShot(warningClip);
        }
    }
}
