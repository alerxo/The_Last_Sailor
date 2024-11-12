using UnityEngine;

public class AIBoat : Boat
{
    [SerializeField] private Cannon[] leftCannons, rightCannons;
    private Vector3? destination;

    private const float APROACH_DISTANCE = 10f;
    private const float ENGAGEMENT_RANGE = 50f;

    private Boat target;
    private float distance;
    private Vector3 cross;

#if UNITY_EDITOR
    [SerializeField] protected bool isDebugMode;
#endif

    public override void Destroyed()
    {
        ObjectPoolManager.Instance.Release(this);
    }

    private void Update()
    {
        if(target == null) return;

        SetDestination();
        ChangeMovement(GetMovementDirection());

        FireLeft();
        FireRight();

#if UNITY_EDITOR
        if (isDebugMode)
        {
            DebugUtil.DrawBox(destination.Value, Quaternion.identity, Vector3.one, Color.green, Time.deltaTime);
        }
#endif
    }

    private Vector2 GetMovementDirection()
    {
        Vector2 movement = Vector2.zero;

        if (destination == null)
        {
            return movement;
        }

        distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destination.Value.x, destination.Value.z));
        movement.y = distance < APROACH_DISTANCE ? distance / (APROACH_DISTANCE * 0.25f) : 1;

        cross = Vector3.Cross((transform.position - destination.Value).normalized, transform.forward);
        movement.x = cross.y;

        return movement;
    }

    public void SetDestination()
    {
        if(Vector3.Distance(target.transform.position + (target.transform.right * ENGAGEMENT_RANGE), transform.position) < 
            Vector3.Distance(target.transform.position - (target.transform.right * ENGAGEMENT_RANGE), transform.position))
        {
            destination = target.transform.position + (target.transform.right * ENGAGEMENT_RANGE);
        }

        else
        {
            destination = target.transform.position - (target.transform.right * ENGAGEMENT_RANGE);
        }
    }

    public void SetTarget(Boat _target)
    {
        target = _target;
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