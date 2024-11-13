using UnityEngine;

public abstract class Boat : MonoBehaviour, IDamageable
{
    protected abstract float MaxHealth { get; }
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
            Destroyed();
        }

        else
        {
            OnHit();
        }
    }

    protected virtual void OnHit() { }

    public abstract void Destroyed();

    public void ChangeMovement(Vector2 _direction)
    {
        engine.ChangeMovement(_direction);
    }
}
