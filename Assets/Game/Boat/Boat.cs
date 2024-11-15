using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Boat : MonoBehaviour, IDamageable
{
    private const float SINK_DURATION = 30f;
    private const float SINK_BUOYANCY = 1.7f;
    private const float SINK_COM_MAX_X_CHANGE = 2f;
    private const float SINK_COM_MAX_Z_CHANGE = 7f;

    public event UnityAction OnDestroyed;
    public event UnityAction OnDamaged;

    [SerializeField] private float MaxHealth;
    [SerializeField] Transform COM;

    private Vector3 startCOM;
    private float health;
    public Engine Engine { get; private set; }
    public Buoyancy Buoyancy { get; private set; }
    public Rigidbody RigidBody { get; private set; }

    public virtual void Awake()
    {
        Engine = GetComponentInChildren<Engine>();
        Buoyancy = GetComponent<Buoyancy>();
        RigidBody = GetComponent<Rigidbody>();

        startCOM = COM.localPosition;
        RigidBody.centerOfMass = COM.localPosition;
        Engine.transform.localPosition = new Vector3(Engine.transform.localPosition.x, COM.localPosition.y, Engine.transform.localPosition.z);

        SetDefault();
    }

    private void Update() // ta bort efter 20/11
    {
        RigidBody.centerOfMass = COM.localPosition;
    }

    public void Damage(float _damage)
    {
        if ((health -= _damage) == 0)
        {
            OnDestroyed?.Invoke();
        }

        else
        {
            OnDamaged?.Invoke();
        }
    }

    public IEnumerator SinkAtSurface(Action _onComplete)
    {
        float startBuoyancy = Buoyancy.BuoyancyForce;
        Vector3 startCenterOfMass = COM.localPosition;
        Vector3 endCenterOfMass = COM.localPosition + new Vector3(UnityEngine.Random.Range(-SINK_COM_MAX_X_CHANGE, SINK_COM_MAX_X_CHANGE), 0, UnityEngine.Random.Range(-SINK_COM_MAX_Z_CHANGE, SINK_COM_MAX_Z_CHANGE));

        float duration = 0;

        while ((duration += Time.deltaTime) < SINK_DURATION)
        {
            float percentage = duration / SINK_DURATION;
            Buoyancy.BuoyancyForce = Mathf.Lerp(startBuoyancy, SINK_BUOYANCY, percentage);
            COM.localPosition = Vector3.Lerp(startCenterOfMass, endCenterOfMass, percentage);

            yield return null;
        }

        _onComplete();
    }

    public IEnumerator SinkToBottom(Action _onComplete)
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

    public void SetDefault()
    {
        health = MaxHealth;
        COM.localPosition = startCOM;
    }
}
