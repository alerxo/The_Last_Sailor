using UnityEngine;

public class AIBoat : Boat
{
    [SerializeField] private Cannon[] leftCannons, rightCannons;

    public override void Destroyed()
    {
        ObjectPoolManager.Instance.ReleaseAIBoat(this);
    }

    private void Update()
    {
        ChangeMovement(new Vector2(1, 1));
        ChangeCannonAngle(new Vector2(0, 1));
        FireLeft();
        FireRight();
    }

    protected void ChangeCannonAngle(Vector2 _rotation)
    {
        foreach (Cannon cannon in leftCannons)
        {
            cannon.Rotate(_rotation);
        }

        foreach (Cannon cannon in rightCannons)
        {
            cannon.Rotate(_rotation);
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