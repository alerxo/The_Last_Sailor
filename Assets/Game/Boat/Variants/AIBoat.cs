using UnityEngine;

public class AIBoat : Boat
{
    [SerializeField] private Cannon[] leftCannons, rightCannons;
    private Vector3? destination;
    private const float APROACH_DISTANCE = 20f;

    private float distance;
    private Vector3 cross;

    public override void Destroyed()
    {
        ObjectPoolManager.Instance.Release(this);
    }

    private void Update()
    {
        SetDestination(FindFirstObjectByType<PlayerBoat>().transform.position);
        ChangeMovement(GetMovementDirection());
        FireLeft();
        FireRight();
    }

    private Vector2 GetMovementDirection()
    {
        Vector2 movement = Vector2.zero;

        if (destination == null)
        {
            return movement;
        }

        distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destination.Value.x, destination.Value.z));
        movement.y = distance < APROACH_DISTANCE ? distance / APROACH_DISTANCE : 1;

        cross = Vector3.Cross((transform.position - destination.Value).normalized, transform.forward);
        movement.x = cross.y;

        return movement;
    }

    public void SetDestination(Vector3 _destination)
    {
        destination = _destination;
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