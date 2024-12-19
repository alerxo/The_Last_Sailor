using UnityEngine;

public class FlagSoundScript : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource.clip = audioClip;
        audioSource.time = Random.Range(0.0f, audioSource.clip.length);
        audioSource.pitch = Random.Range(0.8f, 1.1f);
        audioSource.Play();
    }
}
