using UnityEngine;

public class AICannonController : MonoBehaviour
{
    private const float MAX_RANGE = 200f;

    private AIBoat owner;
    private Cannon cannon;

    private float distance;
    private Vector3 cross;

    private void Awake()
    {
        cannon = GetComponentInParent<Cannon>();
        owner = GetComponentInParent<AIBoat>();
    }

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        if (owner.Target == null)
        {
            return;
        }

        distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(owner.Target.transform.position.x, owner.Target.transform.position.z));
        cannon.SetPitch(Mathf.Lerp(-1, 1, distance / MAX_RANGE));

        cross = Vector3.Cross((transform.position - owner.Target.transform.position).normalized, transform.forward);
        cannon.SetYaw(cross.y);
    }
}