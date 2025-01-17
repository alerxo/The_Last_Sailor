using UnityEngine;

public class AmbianceMultipleSoundScript : MonoBehaviour
{
    [SerializeField] AudioClip[] ambianceSounds;
    [SerializeField] float maxTimerUntillReplay;
    [SerializeField] float minTimerUntillReplay;
    [SerializeField] AudioSource audioSource;

    [SerializeField] float minPaning;
    [SerializeField] float maxPaning;

    [SerializeField] float minPitch;
    [SerializeField] float maxPitch;

    bool allowedPlay;

    private void Awake()
    {
        UIManager.OnStateChanged += UImanager_OnUIStateChange;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UImanager_OnUIStateChange;
    }
    void Update()
    {
        PlayAmbiance();
    }
    private void UImanager_OnUIStateChange(UIState state)
    {
        if (state == UIState.Pause)
        {
            allowedPlay = false;
        }
        else if (state == UIState.Options)
        {
            allowedPlay = false;
        }
        else if (state == UIState.TitleScreen)
        {
            allowedPlay = false;
        }
        else if (state == UIState.Controls)
        {
            allowedPlay = false;
        }
        else
        {
            allowedPlay = true;
        }
    }
    public void PlayAmbiance()
    {
        if (!audioSource.isPlaying && allowedPlay)
        {
            int n = Random.Range(1, ambianceSounds.Length);
            AudioClip clip = ambianceSounds[n];
            audioSource.clip = clip;
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.panStereo = Random.Range(minPaning, maxPaning);
            audioSource.PlayDelayed(Random.Range(minTimerUntillReplay, maxTimerUntillReplay));
            ambianceSounds[n] = ambianceSounds[0];
            ambianceSounds[0] = clip;
        }
    }
}
