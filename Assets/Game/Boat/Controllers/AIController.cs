using UnityEngine;

public class AIController : BoatController
{
    private void Update()
    {
        Movement(Vector3.right);
        ChangeCannonAngle(1);

        FireLeft();
        FireRight();
    }
}