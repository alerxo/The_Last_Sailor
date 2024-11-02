using UnityEngine;

public class BoatController : MonoBehaviour
{
    [SerializeField] private Engine leftEngine, rightEngine;
    [SerializeField] private Cannon[] leftCannons, rightCannons;

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

    private void Fire(Cannon[] cannons)
    {
        foreach (Cannon cannon in cannons)
        {
            if (cannon.State == CannonState.Ready)
            {
                cannon.Fire();
            }
        }
    }
}
