using UnityEngine;

public class FleetManSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip buildShipClip;
    [SerializeField] private AudioClip repairClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        FleetScreen.OnBoatUpgraded += FleetScreen_OnBoatUpgraded;
    }
    private void OnDestroy()
    {
        FleetScreen.OnBoatUpgraded -= FleetScreen_OnBoatUpgraded;
    }

    private void FleetScreen_OnBoatUpgraded()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(upgradeClip);
    }
}
