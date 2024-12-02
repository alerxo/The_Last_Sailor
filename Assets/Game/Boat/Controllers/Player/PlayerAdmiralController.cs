using UnityEngine;

public class PlayerAdmiralController : Admiral
{
    public PlayerBoatController BoatController { get; private set; }

    public void Awake()
    {
        BoatController = GetComponent<PlayerBoatController>();

        SetName("Player");
    }
}
