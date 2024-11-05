using UnityEngine;

public abstract class Boat : MonoBehaviour, IDamageable
{
    [SerializeField] private Engine engine;
    [SerializeField] private float maxHealth;
    private float health;

    private void OnEnable()
    {
        health = maxHealth;
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

    protected void ChangeMovement(Vector2 _direction)
    {
        engine.ChangeMovement(_direction);
    }
}
