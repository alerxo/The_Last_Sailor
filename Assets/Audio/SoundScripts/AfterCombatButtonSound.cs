using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AfterCombatButtonSound : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip repair;
    [SerializeField] AudioClip scrap;
    [SerializeField] AudioClip Seize;
    private void Awake()
    {
        PostCombatScreen.OnBoatRepaired += PostCombatScreen_Repaired;
        PostCombatScreen.OnBoatScrapped += PostCombatScreen_Scrap;
        PostCombatScreen.OnBoatSeized += PostCombatScreen_Seized;
    }

    private void OnDestroy()
    {
        PostCombatScreen.OnBoatRepaired -= PostCombatScreen_Repaired;
        PostCombatScreen.OnBoatScrapped -= PostCombatScreen_Scrap;
        PostCombatScreen.OnBoatSeized -= PostCombatScreen_Seized;
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
    private void PostCombatScreen_Seized()
    {
        audioSource.pitch = (Random.Range(0.8f, 1.2f));
        audioSource.PlayOneShot(Seize);
    }
}
