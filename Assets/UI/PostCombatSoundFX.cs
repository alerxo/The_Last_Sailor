using Unity.VisualScripting;
using UnityEngine;

public class PostCombatSoundFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winClip;
    private void Awake()
    {
        UIManager.OnStateChanged += UIManager_PostCombatStarts;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_PostCombatStarts;
    }
    private void UIManager_PostCombatStarts(UIState state)
    {
        if (state == UIState.PostCombat)
        {
            audioSource.PlayOneShot(winClip);
        }
    }
}
