using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public CannonState State { get; private set; }

    [SerializeField] private float force, fireCooldown;
    [SerializeField] private Transform cannonballSpawnPosition, explosionPosition;

    private ParticleSystem[] particleSystems;
    private AudioSource audioSource;

    private float angle;
    private const float minAngle = 0f;
    private const float maxAngle = 35f;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        State = CannonState.Ready;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void ChangeAngle(float _value)
    {
        angle = Mathf.Clamp(angle + _value, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(angle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    public void Fire()
    {
        Cannonball cannonball = ObjectPoolManager.Instance.SpawnCannonball(cannonballSpawnPosition.position, Quaternion.identity);
        cannonball.GetComponent<Rigidbody>().AddExplosionForce(force, explosionPosition.position, 0, 0);
        cannonball.SetIgnore(GetComponentInParent<IDamageable>());

        foreach(ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Play();
        }

        audioSource.Play();

        State = CannonState.Reloading;
        StartCoroutine(ReloadTimer());
    }

    private IEnumerator ReloadTimer()
    {
        yield return new WaitForSeconds(fireCooldown);
        State = CannonState.Ready;
    }
}

public enum CannonState
{
    Ready,
    Reloading
}