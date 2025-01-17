using UnityEngine;

public class SeagullAmbiance : MonoBehaviour
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

    Transform playerT;
    private void Awake()
    {
        UIManager.OnStateChanged += UImanager_OnUIStateChange;
        playerT = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UImanager_OnUIStateChange;
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
        else 
        {
            allowedPlay = true;
        }
    }

    void Update()
    {
        float dittBalanseradeSeaGullVärde = 150;
        if (Vector3.Distance(playerT.position, transform.position) < dittBalanseradeSeaGullVärde)
        {
            PlayAmbiance();
        }
    }
    public void PlayAmbiance()
    {
        if (!audioSource.isPlaying && allowedPlay)
        {
            int n = Random.Range(0, ambianceSounds.Length);
            AudioClip clip = ambianceSounds[n];
            audioSource.clip = clip;
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.panStereo = Random.Range(minPaning, maxPaning);
            audioSource.PlayDelayed(Random.Range(minTimerUntillReplay, maxTimerUntillReplay));
        }
    }
}
