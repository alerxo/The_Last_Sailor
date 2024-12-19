using UnityEngine;

public class UIInteractionSoundScript : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private void Awake()
    {
        UIManager.OnUIButtonHovered += UImanager_OnUIButtonHovered;
        UIManager.OnUIButtonClicked += UImanager_OnUIButtonClicked;
    }
    private void OnDestroy()
    {
        UIManager.OnUIButtonHovered -= UImanager_OnUIButtonHovered;
        UIManager.OnUIButtonClicked -= UImanager_OnUIButtonClicked;
    }
    private void UImanager_OnUIButtonHovered() 
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(hoverClip);
    }
    private void UImanager_OnUIButtonClicked()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clickClip);
    }
}
