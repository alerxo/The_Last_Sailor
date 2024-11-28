using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Boat : MonoBehaviour, IDamageable
{
    private const float SINK_DURATION = 20f;
    private const float SINK_BUOYANCY = 1.6f;
    private const float SINK_COM_MAX_X_CHANGE = 4f;
    private const float SINK_COM_MAX_Z_CHANGE = 9f;

    public event UnityAction OnDestroyed;
    public event UnityAction OnDamaged;

    [SerializeField] private float MaxHealth;
    [SerializeField] Transform COM;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip destroyedSound;

    private Vector3 defaultCOM;
    public string Name { get; private set; }
    public float Health { get; private set; }
    public bool IsDamaged => Health < MaxHealth;
    public bool IsSunk => Health <= 0;
    public Engine Engine { get; private set; }
    public Buoyancy Buoyancy { get; private set; }
    public Rigidbody RigidBody { get; private set; }

    public virtual void Awake()
    {
        Engine = GetComponentInChildren<Engine>();
        Buoyancy = GetComponent<Buoyancy>();
        RigidBody = GetComponent<Rigidbody>();

        defaultCOM = COM.localPosition;
        RigidBody.centerOfMass = defaultCOM;
        Engine.transform.localPosition = new Vector3(Engine.transform.localPosition.x, COM.localPosition.y, Engine.transform.localPosition.z);

        SetDefault();
    }

    public void Damage(float _damage)
    {
        if (Health == 0) return;

        if (Mathf.Clamp(Health -= _damage, 0, MaxHealth) <= 0)
        {
            OnDestroyed?.Invoke();
        }

        else
        {
            OnDamaged?.Invoke();
            audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(destroyedSound);
        }
    }

    public int GetPercentageHealth()
    {
        return (int)(Health / MaxHealth * 100);
    }

    public void Repair()
    {
        StopAllCoroutines();
        SetDefault();
        Buoyancy.SetDefault();
    }

    public void StartSinkAtSurface()
    {
        StartCoroutine(SinkAtSurface());
    }

    private IEnumerator SinkAtSurface()
    {
        float startBuoyancy = Buoyancy.BuoyancyForce;
        Vector3 startCenterOfMass = RigidBody.centerOfMass;
        Vector3 endCenterOfMass = RigidBody.centerOfMass + new Vector3(UnityEngine.Random.Range(-SINK_COM_MAX_X_CHANGE, SINK_COM_MAX_X_CHANGE), 0, UnityEngine.Random.Range(-SINK_COM_MAX_Z_CHANGE, SINK_COM_MAX_Z_CHANGE));

        float duration = 0;

        while ((duration += Time.deltaTime) < SINK_DURATION)
        {
            float percentage = duration / SINK_DURATION;
            Buoyancy.BuoyancyForce = Mathf.Lerp(startBuoyancy, SINK_BUOYANCY, percentage);
            RigidBody.centerOfMass = Vector3.Lerp(startCenterOfMass, endCenterOfMass, percentage);

            yield return null;
        }
    }

    public void StartSinkToBottom(Action _onComplete)
    {
        StartCoroutine(SinkToBottom(_onComplete));
    }

    private IEnumerator SinkToBottom(Action _onComplete)
    {
        float startBuoyancy = Buoyancy.BuoyancyForce;
        float duration = 0;

        while ((duration += Time.deltaTime) < SINK_DURATION)
        {
            float percentage = duration / SINK_DURATION;
            Buoyancy.BuoyancyForce = Mathf.Lerp(startBuoyancy, 0, percentage);

            yield return null;
        }

        _onComplete();
    }

    public void Upgrade(UpgradeType _type)
    {

    }

    public bool CanUpgrade(UpgradeType _type)
    {
        return true;
    }

    public void SetName(string _name)
    {
        Name = _name;
    }

    public void SetDefault()
    {
        Health = MaxHealth;
        RigidBody.centerOfMass = defaultCOM;
    }
}

public enum UpgradeType
{
    Hull,
    Engine,
    Cannons
}