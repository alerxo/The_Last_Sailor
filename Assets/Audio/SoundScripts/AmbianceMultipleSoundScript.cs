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
    void Update()
    {
        PlayAmbiance();
    }
    public void PlayAmbiance()
    {
        if (!audioSource.isPlaying)
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
