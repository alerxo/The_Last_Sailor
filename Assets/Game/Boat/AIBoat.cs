using UnityEngine;

public class AIBoat : Boat
{
    public override void Destroyed()
    {
        ObjectPoolManager.Instance.ReleaseAIBoat(this);
    }

    private void Update()
    {
        Movement(Vector3.right);
        ChangeCannonAngle(1);
        FireLeft();
        FireRight();
    }
}