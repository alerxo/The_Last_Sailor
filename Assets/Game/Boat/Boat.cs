using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Boat : MonoBehaviour, IDamageable, IUpgradeable
{
    private const float SINK_DURATION = 20f;
    private const float SINK_BUOYANCY = 1.6f;
    private const float SINK_COM_MAX_X_CHANGE = 4f;
    private const float SINK_COM_MAX_Z_CHANGE = 9f;

    public event UnityAction OnDestroyed;
    public event UnityAction OnDamaged;

    [SerializeField] private float MaxHealth;
    [SerializeField] Transform COM;
    [SerializeField] Transform DeathExplosion;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource destroyedAudioSource;
    [SerializeField] AudioSource burningAudioSource;
    [SerializeField] AudioClip destroyedSound;
    [SerializeField] AudioClip fireSound;
    [SerializeField] AudioClip explodeSound;

    private Vector3 defaultCOM;
    public string Name { get; private set; }
    public float Health { get; private set; }
    public bool IsDamaged => Health < MaxHealth;
    public bool IsSunk => Health <= 0;

    public CannonballOwner CannonballOwner => cannonballOwner;
    private CannonballOwner cannonballOwner;
    public bool CanDamage => !IsSunk;

    public Engine Engine { get; private set; }
    public Buoyancy Buoyancy { get; private set; }
    public Rigidbody RigidBody { get; private set; }

    private readonly Dictionary<UpgradeType, IUpgradeable[]> Upgradeables = new();
    public UpgradeTier UpgradeTier { get; set; }
    public float UpgradeIncrease => 0.25f;
    public float GetUpgradeValue => MaxHealth + (MaxHealth * ((int)UpgradeTier * UpgradeIncrease));

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

        SetDefault();
    }

    private void OnEnable()
    {
        SetUpgrade(UpgradeType.Hull, UpgradeTier.One);
        SetUpgrade(UpgradeType.Cannons, UpgradeTier.One);
    }

    public void Damage(float _damage)
    {
        audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(destroyedSound);

        if (Health == 0) return;

        if (Mathf.Clamp(Health -= _damage, 0, MaxHealth) <= 0)
        {
            playDeathExplosion(); // Flyttade dina ljud till metoden, ta bort detta när du har läst Jacob. /V

            OnDestroyed?.Invoke();
        }

        else
        {
            OnDamaged?.Invoke();
        }
    }
    private void playDeathExplosion()
    {
        destroyedAudioSource.PlayOneShot(explodeSound);
        burningAudioSource.clip = fireSound;
        burningAudioSource.loop = true;
        burningAudioSource.Play();

        foreach(ParticleSystem particle in DeathExplosion.GetComponentsInChildren<ParticleSystem>())
        {
            particle.Play();
        }

    }

    public int GetPercentageDurability()
    {
        return (int)(Health / GetUpgradeValue * 100);
    }

    public void Repair()
    {
        if (burningAudioSource.loop == true)
        {
            burningAudioSource.loop = false;
            burningAudioSource.Stop();
        }

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
        ResourceManager.Instance.UpgradeBoat();

        foreach (IUpgradeable upgradeable in Upgradeables[_type])
        {
            upgradeable.UpgradeTier++;
        }
    }

    public bool CanUpgrade(UpgradeType _type)
    {
        return GetTierOfUpgrade(_type) < UpgradeTier.Three && ResourceManager.Instance.CanUpgrade();
    }

    public UpgradeTier GetTierOfUpgrade(UpgradeType _type)
    {
        return Upgradeables[_type][0].UpgradeTier;
    }

    public string GetTierOfUpgradeAsString(UpgradeType _type)
    {
        return string.Concat(Enumerable.Repeat("I", 1 + (int)Upgradeables[_type][0].UpgradeTier));
    }

    public float GetUpgradeIncreasePercentage(UpgradeType _type)
    {
        return (int)(Upgradeables[_type][0].UpgradeIncrease * 100);
    }

    public float GetUpgradeModifierPercentage(UpgradeType _type)
    {
        return (int)((int)Upgradeables[_type][0].UpgradeTier * Upgradeables[_type][0].UpgradeIncrease * 100);
    }

    public string GetModifierDescription(UpgradeType _type)
    {
        switch (_type)
        {
            case UpgradeType.Hull:
                return "Durability";

            case UpgradeType.Cannons:
                return "Damage";

            default:
                Debug.LogError("Defaulted");
                return null;
        }
    }

    public void SetName(string _name)
    {
        Name = _name;
    }

    public void SetCannonBallOwner(CannonballOwner _cannonballOwner)
    {
        cannonballOwner = _cannonballOwner;
    }

    public void SetDefault()
    {
        Health = GetUpgradeValue;
        RigidBody.centerOfMass = defaultCOM;
    }
}