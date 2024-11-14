using UnityEngine;
using UnityEngine.Events;

public class Boat : MonoBehaviour, IDamageable
{
    public event UnityAction OnDestroyed;
    public event UnityAction OnDamaged;

    [SerializeField] private float MaxHealth;
    private float health;
    private Engine engine;

    public virtual void Awake()
    {
        engine = GetComponentInChildren<Engine>();
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
