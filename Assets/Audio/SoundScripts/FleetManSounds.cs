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
        FleetScreen.OnBoatBuilt += FleetScreen_OnBoatBuilt;
        FleetScreen.OnBoatRepaired += FleetScreen_OnBoatRepaired;
    }
    private void OnDestroy()
    {
        FleetScreen.OnBoatUpgraded -= FleetScreen_OnBoatUpgraded;
        FleetScreen.OnBoatBuilt -= FleetScreen_OnBoatBuilt;
        FleetScreen.OnBoatRepaired -= FleetScreen_OnBoatRepaired;
    }

    private void FleetScreen_OnBoatUpgraded()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(upgradeClip);
    }
    private void FleetScreen_OnBoatBuilt()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(buildShipClip);
    }
    private void FleetScreen_OnBoatRepaired()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(repairClip);
    }
}
