using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    private const float FORCE = 300;

    private const float COOLDOWN = 5;
    private const float ROTATION_SPEED = 0.7f;
    private const float PITCH_OFFSET = -4;
    private const float MAX_PITCH = 7.5f;
    public const float MAX_YAW = 20;

    public CannonState State { get; private set; }

    [Tooltip("The empty transform at the base of the cannon")]
    [SerializeField] private Transform mount;
    [Tooltip("The barrel mesh")]
    [SerializeField] private Transform barrel;
    [Tooltip("The explosion point, should be place in front of barrel and not colliding with it")]
    [SerializeField] private Transform explosionPoint;

    private ParticleSystem[] particleSystems;
    private AudioSource audioSource;

    private Vector3 localRotation, barrelRoation;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();

        SetPitch(0);
        SetYaw(0);
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

    public void SetPitch(float pitch)
    {
        barrelRoation.x = Mathf.Clamp(Mathf.Lerp(barrelRoation.x, -pitch * MAX_PITCH * 1.5f, ROTATION_SPEED * Time.deltaTime), -MAX_PITCH, MAX_PITCH);
        barrel.localRotation = Quaternion.Euler(barrelRoation + new Vector3(PITCH_OFFSET, 0, 0));
    }

    public void SetYaw(float yaw)
    {
        localRotation.y = Mathf.Clamp(Mathf.Lerp(localRotation.y, yaw * MAX_YAW * 1.5f, ROTATION_SPEED * Time.deltaTime), -MAX_YAW, MAX_YAW);
        mount.localRotation = Quaternion.Euler(localRotation);
    }

    public void Fire()
    {
        Cannonball cannonball = ObjectPoolManager.Instance.Spawn<Cannonball>(explosionPoint.position, Quaternion.identity);
        cannonball.GetComponent<Rigidbody>().AddForce(-barrel.up * FORCE, ForceMode.Impulse);
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
        yield return new WaitForSeconds(COOLDOWN);
        State = CannonState.Ready;
    }
}

public enum CannonState
{
    Ready,
    Reloading
}