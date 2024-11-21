using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public const float CANNONBALL_FORCE = 300;
    public const float CANNONBALL_MASS = 3;
    public const float CANNONBALL_DRAG = 0.05f;

    private const float COOLDOWN = 5;

    private const float PITCH_ROTATION_SPEED = 20f;
    private const float YAW_ROTATION_SPEED = 30f;
    private const float MAX_PITCH = 7.5f;
    private const float PITCH_OFFSET = -4;
    public const float MAX_YAW = 20;

    private const int PREDICTION_ITERATIONS = 1000;
    private const float PREDICTION_INCREMENT = 0.025f;

    private bool firstReload;

    public CannonState State { get; private set; }

    [Tooltip("The empty transform at the base of the cannon")]
    [SerializeField] private Transform mount;
    [Tooltip("The barrel mesh")]
    [SerializeField] private Transform barrel;
    [Tooltip("The explosion point, should be place in front of barrel and not colliding with it")]
    [SerializeField] private Transform explosionPoint;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip sizzlingClip;

    private ParticleSystem[] particleSystems;
    private AudioSource audioSource;

    private Vector3 localRotation, barrelRotation;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        firstReload = true;

        ChangePitchTowards(0);
        ChangeYawTowards(0);
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

    public void ChangePitch(float _pitch)
    {
        barrelRotation.x = Mathf.Clamp(barrelRotation.x + (-_pitch * PITCH_ROTATION_SPEED * Time.deltaTime), -MAX_PITCH, MAX_PITCH);
        barrel.localRotation = Quaternion.Euler(barrelRotation + new Vector3(PITCH_OFFSET, 0, 0));
    }

    public void ChangePitchTowards(float _pitch)
    {
        barrelRotation.x = Mathf.Clamp(Mathf.Lerp(barrelRotation.x, -_pitch * MAX_PITCH * 2f, Time.deltaTime), -MAX_PITCH, MAX_PITCH);
        barrel.localRotation = Quaternion.Euler(barrelRotation + new Vector3(PITCH_OFFSET, 0, 0));
    }

    public void ChangeYaw(float _yaw)
    {
        localRotation.y = Mathf.Clamp(localRotation.y + (_yaw * YAW_ROTATION_SPEED * Time.deltaTime), -MAX_YAW, MAX_YAW);
        mount.localRotation = Quaternion.Euler(localRotation);
    }

    public void ChangeYawTowards(float _yaw)
    {
        localRotation.y = Mathf.Clamp(Mathf.Lerp(localRotation.y, _yaw * MAX_YAW * 2f, Time.deltaTime), -MAX_YAW, MAX_YAW);
        mount.localRotation = Quaternion.Euler(localRotation);
    }

    public void Fire()
    {
        Cannonball cannonball = ObjectPoolManager.Instance.Spawn<Cannonball>(explosionPoint.position, Quaternion.identity);
        cannonball.GetComponent<Rigidbody>().AddForce(-barrel.up * CANNONBALL_FORCE, ForceMode.Impulse);
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
        if (firstReload == false)
        {
            yield return new WaitForSeconds(COOLDOWN - 3);
            audioSource.PlayOneShot(sizzlingClip);
            yield return new WaitForSeconds(1);
            audioSource.PlayOneShot(reloadClip);
            yield return new WaitForSeconds(2);
        }
        else 
        {
            yield return new WaitForSeconds(COOLDOWN);
            firstReload = false;
        }

        State = CannonState.Ready;
    }

    public Vector3 GetHitPrediction(float minYPos)
    {
        Vector3 directon = -barrel.up;
        Vector3 startPosition = explosionPoint.position;
        float speed = CANNONBALL_FORCE;
        float mass = CANNONBALL_MASS;
        float drag = CANNONBALL_DRAG;

        Vector3 velocity = directon * (speed / mass);
        Vector3 position = startPosition;
        Vector3 nextPosition;

        for (int i = 0; i < PREDICTION_ITERATIONS; i++)
        {
            velocity = GetNextVelocity(velocity, drag, PREDICTION_INCREMENT);
            nextPosition = position + velocity * PREDICTION_INCREMENT;

            if (nextPosition.y < minYPos)
            {
                return nextPosition;
            }

            position = nextPosition;
        }

        Debug.LogWarning("Reached end of prediction iterations");

        return position;
    }

#if UNITY_EDITOR
    public void GetHitPrediction_IsDebugMode(float minYPos)
    {
        Vector3 directon = -barrel.up;
        Vector3 startPosition = explosionPoint.position;
        float speed = CANNONBALL_FORCE;
        float mass = CANNONBALL_MASS;
        float drag = CANNONBALL_DRAG;

        Vector3 velocity = directon * (speed / mass);
        Vector3 position = startPosition;
        Vector3 nextPosition;

        for (int i = 0; i < PREDICTION_ITERATIONS; i++)
        {
            velocity = GetNextVelocity(velocity, drag, PREDICTION_INCREMENT);
            nextPosition = position + velocity * PREDICTION_INCREMENT;

            Debug.DrawLine(position, nextPosition, Color.yellow, Time.deltaTime);

            if (nextPosition.y < minYPos)
            {
                return;
            }

            position = nextPosition;
        }
    }
#endif

    private Vector3 GetNextVelocity(Vector3 velocity, float drag, float increment)
    {
        velocity += Physics.gravity * increment;
        velocity *= Mathf.Clamp01(1f - (drag * increment));

        return velocity;
    }
}

public enum CannonState
{
    Ready,
    Reloading
}