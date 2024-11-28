using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Boat : MonoBehaviour, IDamageable, IUpgradeable
{
    public const int UPGRADE_COST = 5;

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

    private readonly Dictionary<UpgradeType, IUpgradeable[]> Upgradeables = new();
    public UpgradeTier UpgradeTier { get; set; }

    public virtual void Awake()
    {
        Engine = GetComponentInChildren<Engine>();
        Buoyancy = GetComponent<Buoyancy>();
        RigidBody = GetComponent<Rigidbody>();

        defaultCOM = COM.localPosition;
        RigidBody.centerOfMass = defaultCOM;
        Engine.transform.localPosition = new Vector3(Engine.transform.localPosition.x, COM.localPosition.y, Engine.transform.localPosition.z);

        Upgradeables.Add(UpgradeType.Hull, new IUpgradeable[] { this });
        Upgradeables.Add(UpgradeType.Cannons, GetComponentsInChildren<Cannon>());
        Upgradeables.Add(UpgradeType.Engine, new IUpgradeable[] { Engine });

        SetDefault();
    }

    private void OnEnable()
    {
        SetUpgrade(UpgradeType.Hull, UpgradeTier.One);
        SetUpgrade(UpgradeType.Cannons, UpgradeTier.One);
        SetUpgrade(UpgradeType.Engine, UpgradeTier.One);
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
        return (int)(Health / GetUpgradeValue() * 100);
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

    public void SetUpgrade(UpgradeType _type, UpgradeTier _tier)
    {
        foreach (IUpgradeable upgradeable in Upgradeables[_type])
        {
            upgradeable.UpgradeTier = _tier;
        }
    }

    public void Upgrade(UpgradeType _type)
    {
        foreach (IUpgradeable upgradeable in Upgradeables[_type])
        {
            upgradeable.UpgradeTier++;
        }

        ResourceManager.Instance.AddResource(-UPGRADE_COST);
    }

    public bool CanUpgrade(UpgradeType _type)
    {
        return GetTierOfUpgrade(_type) < UpgradeTier.Three && ResourceManager.Instance.Amount >= UPGRADE_COST;
    }

    public UpgradeTier GetTierOfUpgrade(UpgradeType _type)
    {
        return Upgradeables[_type][0].UpgradeTier;
    }

    public void SetName(string _name)
    {
        Name = _name;
    }

    public void SetDefault()
    {
        Health = GetUpgradeValue();
        RigidBody.centerOfMass = defaultCOM;
    }

    public float GetUpgradeValue()
    {
        switch (UpgradeTier)
        {
            case UpgradeTier.One:
                return MaxHealth;

            case UpgradeTier.Two:
                return MaxHealth * 1.2f;

            case UpgradeTier.Three:
                return MaxHealth * 1.5f;

            default:
                Debug.LogError("Defaulted in GetUpgradeTier");
                return 0;
        }
    }
}