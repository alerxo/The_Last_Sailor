using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class MusicSoundSettings : MonoBehaviour
{
    [SerializeField] Slider soundSlider;
    [SerializeField] AudioMixer masterMixer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetVolume(PlayerPrefs.GetFloat("SavedMusicVolume", 100));
    }

    public void SetVolume(float _value)
    {
        if (_value < 1)
        {
            _value = .001f;
        }
        RefreshSlider(_value);
        PlayerPrefs.SetFloat("SavedMusicVolume", _value);
        masterMixer.SetFloat("MusicVolume", Mathf.Log10(_value / 100) * 20f);
    }
    public void SetVolumeFromSlider()
    {
        SetVolume(soundSlider.value);
    }

    public void RefreshSlider(float _value)
    {
        soundSlider.value = _value;
    }
}
