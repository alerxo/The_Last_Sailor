using UnityEngine;

public class PlayerAdmiralController : Admiral
{
    public PlayerBoatController PlayerBoatController { get; private set; }

    private int subordinateNumber = 0;

    public void Awake()
    {
        PlayerBoatController = GetComponent<PlayerBoatController>();

        SetName("Player");
    }

    public override string GetSubordinateName()
    {
        if (subordinateNumber == int.MaxValue) subordinateNumber = 0;

        return $"Allied Boat {++subordinateNumber}";
    }

    public void BuildBoat()
    {
        AIBoatController subordinate = ObjectPoolManager.Instance.Spawn<AIBoatController>(transform.position + -transform.forward * 40, transform.rotation);
        AddSubordinate(subordinate.Boat);
        subordinate.Boat.SetName(GetSubordinateName());
        subordinate.TrySetCommand(Command);
    }
}
