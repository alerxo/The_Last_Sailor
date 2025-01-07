using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cannon : MonoBehaviour, IUpgradeable
{
    private const float BOAT_LENGTH = 22f;
    private const float BOAT_HEIGHT = 10f;
    private const float BOAT_HEIGHT_OFFSET = 2f;
    private const float BOAT_WIDTH = 10f;
    private const float MIN_HEIGHT_OFFSET = -2f;

    public const float CANNONBALL_DAMAGE = 10f;

    public const float CANNONBALL_FORCE = 300;
    public const float CANNONBALL_MASS = 3;
    public const float CANNONBALL_DRAG = 0.05f;

    private const float COOLDOWN = 5;

    private const float PITCH_ROTATION_SPEED = 20f;
    private const float YAW_ROTATION_SPEED = 30f;
    private const float MAX_PITCH = 7.5f;
    private const float PITCH_OFFSET = -4;
    public const float MAX_YAW = 20;

    private const int MAX_PREDICTION_ITERATIONS = 1000;
    private const float PREDICTION_INCREMENT = 0.1f;
    private const float ITERATION_OVERLAP = 0.1f;

    public CannonState State { get; private set; }

    public UpgradeTier UpgradeTier { get; set; }
    public float UpgradeIncrease => 0.25f;
    public float GetUpgradeValue => CANNONBALL_DAMAGE + (CANNONBALL_DAMAGE * ((int)UpgradeTier * UpgradeIncrease));

    [Tooltip("The empty transform at the base of the cannon")]
    [SerializeField] private Transform mount;
    [Tooltip("The barrel mesh")]
    [SerializeField] private Transform barrel;
    [SerializeField] private float minLongRangeSoundBarrier;
    [SerializeField] private float maxShortRangeSoundBarrier;
    [Tooltip("The explosion point, should be place in front of barrel and not colliding with it")]
    [SerializeField] private Transform explosionPoint;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip sizzlingClip;
    [SerializeField] private AudioClip fireShortClip;
    [SerializeField] private AudioClip fireLongClip;

    [SerializeField] private Renderer barrelRenderer;

    private ParticleSystem[] particleSystems;
    [SerializeField] private AudioSource audioReloadSource;
    [SerializeField] private AudioSource audioShortSource;
    [SerializeField] private AudioSource audioLongSource;

    [SerializeField] private AudioSource audioMaxSource;
    [SerializeField] private AudioClip audioMaxClip;

    private bool maxOncePitch;
    private bool maxOnceYaw;

    private Vector3 localRotation, barrelRotation;

    private bool firstReload;
    private GameObject player;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        firstReload = true;

        player = GameObject.FindWithTag("Player");
        ChangePitchTowards(0);
        ChangeYawTowards(0);
        maxOncePitch = true;
        maxOnceYaw = true;
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
        if (barrelRotation.x == MAX_PITCH || barrelRotation.x == -MAX_PITCH)
        {
            if (maxOncePitch == true)
            {
                maxOncePitch = false;
                audioMaxSource.pitch = Random.Range(0.6f, 1.2f);
                audioMaxSource.PlayOneShot(audioMaxClip);
            }
        }
        else
        {
            maxOncePitch = true;
        }
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
        if (localRotation.y == MAX_YAW || localRotation.y == -MAX_YAW)
        {
            if (maxOnceYaw == true)
            {
                maxOnceYaw = false;
                audioMaxSource.pitch = Random.Range(0.6f, 1.2f);
                audioMaxSource.PlayOneShot(audioMaxClip);
            }
        }
        else
        {
            maxOnceYaw = true;
        }
    }

    public void ChangeYawTowards(float _yaw)
    {
        localRotation.y = Mathf.Clamp(Mathf.Lerp(localRotation.y, _yaw * MAX_YAW * 2f, Time.deltaTime), -MAX_YAW, MAX_YAW);
        mount.localRotation = Quaternion.Euler(localRotation);
    }

    public void Fire(CannonballOwner _cannonballOwner)
    {
        Cannonball cannonball = ObjectPoolManager.Instance.Spawn<Cannonball>(explosionPoint.position, Quaternion.identity);
        cannonball.GetComponent<Rigidbody>().AddForce(-barrel.up * CANNONBALL_FORCE, ForceMode.Impulse);
        cannonball.SetValues(GetComponentInParent<IDamageable>(), GetUpgradeValue, _cannonballOwner);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Play();
        }

        if(Vector3.Distance(this.transform.position, player.transform.position) <= maxShortRangeSoundBarrier) 
        {
            audioLongSource.pitch = Random.Range(0.9f, 1.1f);
            audioShortSource.PlayOneShot(fireShortClip);
        }
        if (Vector3.Distance(this.transform.position, player.transform.position) >= minLongRangeSoundBarrier)
        {
            audioLongSource.pitch = Random.Range(0.3f, 0.5f);
            audioLongSource.PlayOneShot(fireLongClip);
        }

        State = CannonState.Reloading;
        StartCoroutine(ReloadTimer());
        StartCoroutine(CooldownShader());
    }

    private IEnumerator ReloadTimer()
    {
        if (firstReload == false)
        {
            yield return new WaitForSeconds(1);
            audioReloadSource.PlayOneShot(reloadClip);
            yield return new WaitForSeconds(COOLDOWN - 2);
            audioReloadSource.PlayOneShot(sizzlingClip);
            yield return new WaitForSeconds(1);
        }

        else
        {
            yield return new WaitForSeconds(COOLDOWN);
            firstReload = false;
        }

        State = CannonState.Ready;
    }

    private IEnumerator CooldownShader()
    {
        MaterialPropertyBlock propertyBlock = new();
        barrelRenderer.GetPropertyBlock(propertyBlock);
        float f;
        float duration = 0;

        while ((duration += Time.deltaTime) < COOLDOWN)
        {
            f = Mathf.Lerp(1f, 0f, duration / COOLDOWN);
            propertyBlock.SetFloat("_EmissionStrength", f);
            barrelRenderer.SetPropertyBlock(propertyBlock);
            yield return null;
        }

        f = 0;
        propertyBlock.SetFloat("_EmissionStrength", f);
        barrelRenderer.SetPropertyBlock(propertyBlock);
    }

    public CannonPrediction GetHitPrediction(List<Vector3> _obstacles, Boat _target, Vector3 _predictedPosition)
    {
        Vector3 directon = -barrel.up;
        Vector3 startPosition = explosionPoint.position;
        float speed = CANNONBALL_FORCE;
        float mass = CANNONBALL_MASS;
        float drag = CANNONBALL_DRAG;
        float minHeight = _target.transform.position.y + MIN_HEIGHT_OFFSET;

        Vector3 velocity = directon * (speed / mass);
        Vector3 position = startPosition;
        Vector3 nextPosition;
        float overlap;

        Bounds target = new(_predictedPosition + new Vector3(0, BOAT_HEIGHT_OFFSET, 0), new Vector3(BOAT_WIDTH, BOAT_HEIGHT, BOAT_LENGTH));
        Bounds[] obstacles = new Bounds[_obstacles.Count];

        for (int i = 0; i < _obstacles.Count; i++)
        {
            obstacles[i] = new(_obstacles[i] + new Vector3(0, BOAT_HEIGHT_OFFSET * 2, 0), new Vector3(BOAT_WIDTH, BOAT_HEIGHT * 2.5f, BOAT_LENGTH * 1.5f));
        }

        for (int i = 0; i < MAX_PREDICTION_ITERATIONS; i++)
        {
            velocity = GetNextVelocity(velocity, drag, PREDICTION_INCREMENT);
            nextPosition = position + velocity * PREDICTION_INCREMENT;

            overlap = Vector3.Distance(position, nextPosition) * ITERATION_OVERLAP;

            Ray ray = new(position, velocity.normalized);

            foreach (Bounds bounds in obstacles)
            {
                if (bounds.IntersectRay(ray, out float obstacleDistance) && obstacleDistance <= overlap)
                {
                    return new CannonPrediction
                    {
                        Hit = null,
                        Point = nextPosition,
                    };
                }
            }

            if (target.IntersectRay(ray, out float targetDistance) && targetDistance <= overlap)
            {
                return new CannonPrediction
                {
                    Hit = _target,
                    Point = nextPosition,
                };
            }

            if (nextPosition.y < minHeight)
            {
                return new CannonPrediction
                {
                    Hit = null,
                    Point = nextPosition,
                };
            }

            position = nextPosition;
        }

        Debug.LogWarning("Reached end of prediction iterations");

        return new CannonPrediction
        {
            Hit = null,
            Point = position,
        };
    }

#if UNITY_EDITOR
    public void DebugDrawTrajectory(Boat _target, Vector3 _predictedPosition)
    {
        Vector3 directon = -barrel.up;
        Vector3 startPosition = explosionPoint.position;
        float speed = CANNONBALL_FORCE;
        float mass = CANNONBALL_MASS;
        float drag = CANNONBALL_DRAG;
        float minHeight = _target.transform.position.y + MIN_HEIGHT_OFFSET;

        Vector3 velocity = directon * (speed / mass);
        Vector3 position = startPosition;
        Vector3 nextPosition;

        Bounds bounds = new(_predictedPosition + new Vector3(0, BOAT_HEIGHT_OFFSET, 0), new Vector3(BOAT_WIDTH, BOAT_HEIGHT, BOAT_LENGTH));

        DebugUtil.DrawBox(bounds.center, _target.transform.rotation, bounds.size, Color.red, Time.deltaTime);

        for (int i = 0; i < MAX_PREDICTION_ITERATIONS; i++)
        {
            velocity = GetNextVelocity(velocity, drag, PREDICTION_INCREMENT);
            nextPosition = position + velocity * PREDICTION_INCREMENT;

            Debug.DrawLine(position, nextPosition, Color.yellow, Time.deltaTime);

            if (nextPosition.y < minHeight)
            {
                return;
            }

            position = nextPosition;
        }
    }
#endif

    private Vector3 GetNextVelocity(Vector3 _velocity, float _drag, float _increment)
    {
        _velocity += Physics.gravity * _increment;
        _velocity *= Mathf.Clamp01(1f - (_drag * _increment));

        return _velocity;
    }
}

public struct CannonPrediction
{
    public Boat Hit;
    public Vector3 Point;
}

public enum CannonState
{
    Ready,
    Reloading
}