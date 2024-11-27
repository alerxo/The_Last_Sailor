using UnityEngine;

public class PauseSoundScript : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pauseClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        UIManager.OnStateChanged += UImanager_OnUIStateChange;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UImanager_OnUIStateChange;
    }

    private void UImanager_OnUIStateChange (UIState state) 
    {

        if (state == UIState.Pause) 
        {
            audioSource.PlayOneShot (pauseClip);
        }
    }
}
