using UnityEngine;

public class PlayerAdmiralController : Admiral
{
    public PlayerBoatController BoatController { get; private set; }

    public override void Awake()
    {
        base.Awake();

        BoatController = GetComponent<PlayerBoatController>();

        SetName("Player");
    }
}
