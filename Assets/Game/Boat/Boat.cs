using UnityEngine;
using UnityEngine.Events;

public class Boat : MonoBehaviour, IDamageable
{
    public event UnityAction OnDestroyed;
    public event UnityAction OnDamaged;

    [SerializeField] private float MaxHealth;
    [SerializeField] Transform COM;
    private float health;
    private Engine engine;

    public virtual void Awake()
    {
        engine = GetComponentInChildren<Engine>();
        GetComponent<Rigidbody>().centerOfMass = COM.localPosition;
    }
    void Update()// ta bort efter 20/11
    {
        GetComponent<Rigidbody>().centerOfMass = COM.localPosition; 
    }

    private void OnEnable()
    {
        health = MaxHealth;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
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

    public void ChangeMovement(Vector2 _direction)
    {
        engine.ChangeMovement(_direction);
    }
}
