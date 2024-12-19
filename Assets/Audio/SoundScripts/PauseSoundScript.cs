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
        AudioSource[] audios = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        if (state == UIState.Pause) 
        {
            foreach (AudioSource audio in audios)
            {
                audio.Pause();
            }
            audioSource.PlayOneShot (pauseClip);
        }
        if (state == UIState.Options) 
        {
            audioSource.PlayOneShot(pauseClip);
        }
        if (state == UIState.HUD)
        {
            foreach (AudioSource audio in audios)
            {
                if (!audio.isPlaying) 
                {
                    audio.UnPause();
                }
            }
        }
    }
}
