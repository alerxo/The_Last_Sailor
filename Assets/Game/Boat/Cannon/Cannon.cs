using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public const float CANNONBALL_FORCE = 300;
    public const float CANNONBALL_MASS = 3;
    public const float CANNONBALL_DRAG = 0.05f;

    private const float COOLDOWN = 5;
    private const float ROTATION_SPEED = 0.7f;
    private const float PITCH_OFFSET = -4;
    private const float MAX_PITCH = 7.5f;
    public const float MAX_YAW = 20;

    private const int PREDICTION_ITERATIONS = 1000;
    private const float PREDICTION_INCREMENT = 0.025f;

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
        yield return new WaitForSeconds(COOLDOWN);
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