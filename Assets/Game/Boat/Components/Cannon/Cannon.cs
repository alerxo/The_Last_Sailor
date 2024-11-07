using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Cannon : MonoBehaviour
{
    public CannonState State { get; private set; }

    [SerializeField] private float force, fireCooldown, rotationSpeed, minPitch, maxPitch, maxYaw;
    [SerializeField] private Transform local, cannonballSpawnPosition, explosionPosition;

    private ParticleSystem[] particleSystems;
    private AudioSource audioSource;

    private Vector3 current;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        State = CannonState.Reloading;
        StartCoroutine(ReloadTimer());
    }

    private void OnDisable()
    {
        StopCoroutine(ReloadTimer());
    }

    public void Rotate(Vector2 _rotation)
    {
        current.x = Mathf.Clamp(current.x + (-_rotation.y * rotationSpeed * Time.deltaTime), -maxPitch, -minPitch);
        current.y = Mathf.Clamp(current.y + (_rotation.x * rotationSpeed * Time.deltaTime), -maxYaw, maxYaw);
        local.localRotation = Quaternion.Euler(current);
    }

    public void Fire()
    {
        Cannonball cannonball = ObjectPoolManager.Instance.Spawn<Cannonball>(cannonballSpawnPosition.position, Quaternion.identity);
        cannonball.GetComponent<Rigidbody>().AddExplosionForce(force, explosionPosition.position, 0, 0);
        cannonball.SetIgnore(GetComponentInParent<IDamageable>());

        foreach (ParticleSystem particleSystem in particleSystems)
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