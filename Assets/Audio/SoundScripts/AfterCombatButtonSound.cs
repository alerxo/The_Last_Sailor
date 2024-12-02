using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AfterCombatButtonSound : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip repair;
    [SerializeField] AudioClip scrap;
    private void Awake()
    {
        PostCombatScreen.OnBoatRepaired += PostCombatScreen_Repaired;
        PostCombatScreen.OnBoatScrapped += PostCombatScreen_Scrap;
    }

    private void OnDestroy()
    {
        PostCombatScreen.OnBoatRepaired -= PostCombatScreen_Repaired;
        PostCombatScreen.OnBoatScrapped -= PostCombatScreen_Scrap;
    }
    private void PostCombatScreen_Repaired()
    {
        audioSource.pitch = (Random.Range(0.8f, 1.2f));
        audioSource.PlayOneShot(repair);
    }
    private void PostCombatScreen_Scrap()
    {
        audioSource.pitch = (Random.Range(0.8f, 1.2f));
        audioSource.PlayOneShot(scrap);
    }
}
