using UnityEngine;

public abstract class Boat : MonoBehaviour, IDamageable
{
    [SerializeField] private Engine leftEngine, rightEngine;
    [SerializeField] private Cannon[] leftCannons, rightCannons;

    [SerializeField] private float maxHealth = 100;
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

    protected void Movement(Vector3 _direction)
    {
        if (_direction.y > 0)
        {
            leftEngine.SetState(EngineState.Accelerating);
            rightEngine.SetState(EngineState.Accelerating);
        }

        else if (_direction.x > 0)
        {
            leftEngine.SetState(EngineState.Accelerating);
            rightEngine.SetState(EngineState.Decelerating);
        }

        else if (_direction.x < 0)
        {
            leftEngine.SetState(EngineState.Decelerating);
            rightEngine.SetState(EngineState.Accelerating);
        }

        else
        {
            leftEngine.SetState(EngineState.Decelerating);
            rightEngine.SetState(EngineState.Decelerating);
        }
    }

    protected void ChangeCannonAngle(float _value)
    {
        foreach (Cannon cannon in leftCannons)
        {
            cannon.ChangeAngle(_value);
        }

        foreach (Cannon cannon in rightCannons)
        {
            cannon.ChangeAngle(_value);
        }
    }

    protected void FireLeft()
    {
        Fire(leftCannons);
    }

    protected void FireRight()
    {
        Fire(rightCannons);
    }

    private void Fire(Cannon[] _cannons)
    {
        foreach (Cannon cannon in _cannons)
        {
            if (cannon.State == CannonState.Ready)
            {
                cannon.Fire();
            }
        }
    }
}
