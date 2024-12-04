using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.Audio;

public class SoundSettingsManager : MonoBehaviour
{
    public static SoundSettingsManager Instance { get; private set; }

    [SerializeField] AudioMixer mixer;

    void Start()
    {
        SetMusicVolume(PlayerPrefs.GetFloat("SavedMusicVolume", 100));
        SetAmbianceVolume(PlayerPrefs.GetFloat("SavedAmbianceVolume", 100));
        SetSFXVolume(PlayerPrefs.GetFloat("SavedSFXVolume", 100));
        SetMasterVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 100));
    }

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    public void SetMusicVolume(float _value)
    {
        if (_value < 1)
        {
            _value = .001f;
        }
        PlayerPrefs.SetFloat("SavedMusicVolume", _value);
        mixer.SetFloat("MusicVolume", Mathf.Log10(_value / 100) * 20f);
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("SavedMusicVolume", 100);
    }

    public void SetAmbianceVolume(float _value)
    {
        if (_value < 1)
        {
            _value = .001f;
        }
        PlayerPrefs.SetFloat("SavedAmbianceVolume", _value);
        mixer.SetFloat("AmbianceVolume", Mathf.Log10(_value / 100) * 20f);
    }

    public float GetAmbianceVolume()
    {
        return PlayerPrefs.GetFloat("SavedAmbianceVolume", 100);
    }

    public void SetSFXVolume(float _value)
    {
        if (_value < 1)
        {
            _value = .001f;
        }
        PlayerPrefs.SetFloat("SavedSFXVolume", _value);
        mixer.SetFloat("SFXVolume", Mathf.Log10(_value / 100) * 20f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SavedSFXVolume", 100);
    }
    public void SetMasterVolume(float _value)
    {
        if (_value < 1)
        {
            _value = .001f;
        }
        PlayerPrefs.SetFloat("SavedMasterVolume", _value);
        mixer.SetFloat("MasterVolume", Mathf.Log10(_value / 100) * 20f);
    }
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat("SavedMasterVolume", 100);
    }
}
