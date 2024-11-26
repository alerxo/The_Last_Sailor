using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class MusicScript : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float musicVolume;
    [SerializeField] private float minDelayBetweenTracks;
    [SerializeField] private float maxDelayBetweenTracks;
    bool turnOffMusic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicSource.volume = musicVolume;
        musicSource.clip = musicClip;
        turnOffMusic = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (turnOffMusic == false) 
        {
            PlayMusic();
            if (musicSource.volume != 0.2f) 
            {
                musicSource.volume = musicSource.volume + 0.001f;
            }
        }
        if (turnOffMusic == true)
        {
            if (musicSource.volume != 0f)
            {
                musicSource.volume = musicSource.volume - 0.01f;
            }
        }
    }
    void PlayMusic() 
    {
        if (!musicSource.isPlaying)
        {
            musicSource.PlayDelayed(Random.Range(minDelayBetweenTracks, maxDelayBetweenTracks));
        }
    }
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
            turnOffMusic = true;
        }
    }
}
