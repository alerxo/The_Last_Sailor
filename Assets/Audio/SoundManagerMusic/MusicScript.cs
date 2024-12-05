using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class MusicScript : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource battleMusicSource;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip battleClip;
    [SerializeField] private float musicVolume;
    [SerializeField] private float minDelayBetweenTracks;
    [SerializeField] private float maxDelayBetweenTracks;
    bool turnOffMusic;
    bool turnOffBattleMusic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicSource.volume = musicVolume;
        musicSource.clip = musicClip;
        battleMusicSource.volume = musicVolume;
        battleMusicSource.clip = battleClip;
        turnOffMusic = true;
        turnOffBattleMusic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (turnOffMusic == false) 
        {
            PlayMusic();
            if (musicSource.volume <= musicVolume) 
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
        if (turnOffBattleMusic == false)
        {
            if (musicSource.isPlaying) 
            {
                musicSource.Stop();
            }
            PlayBattleMusic();
            if (battleMusicSource.volume <= musicVolume)
            {
                battleMusicSource.volume = battleMusicSource.volume + 0.001f;
            }
        }
        if (turnOffBattleMusic == true)
        {
            if (battleMusicSource.volume != 0f)
            {
                battleMusicSource.volume = battleMusicSource.volume - 0.01f;
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
    void PlayBattleMusic()
    {
        if (!battleMusicSource.isPlaying)
        {
            battleMusicSource.PlayDelayed(Random.Range(minDelayBetweenTracks, maxDelayBetweenTracks));
        }
    }
    private void Awake()
    {
        CombatManager.OnAdmiralInCombatChanged += CombatManager_OnAdmiralInCombatChanged;
        UIManager.OnStateChanged += UIManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        CombatManager.OnAdmiralInCombatChanged -= CombatManager_OnAdmiralInCombatChanged;
        UIManager.OnStateChanged -= UIManager_OnStateChanged;
    }
    private void UIManager_OnStateChanged(UIState state) 
    {
        if (state != UIState.TitleScreen)
        {
        turnOffMusic = false;
        }
        if (state == UIState.PostCombat)
        {
            turnOffBattleMusic = true;
        }
    }
    private void CombatManager_OnAdmiralInCombatChanged(Admiral _admiral)
    {
        if (_admiral != null)
        {
            turnOffMusic = true;
            turnOffBattleMusic = false;
        }
        if (_admiral == null)
        {
            turnOffMusic = false;
            turnOffBattleMusic = true;
        }
    }
}
