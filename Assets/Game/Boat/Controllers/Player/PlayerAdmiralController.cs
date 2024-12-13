using UnityEngine;

public class PlayerAdmiralController : Admiral
{
    public PlayerBoatController PlayerBoatController { get; private set; }

    private int subordinateNumber = 0;

    private Formation defaultFormation;

    public void Awake()
    {
        PlayerBoatController = GetComponent<PlayerBoatController>();

        SetName("Player");
        SetDefaultFormation(Formation.Line);
        SetCommandForSubordinates(Command.Follow);
    }

    public override string GetSubordinateName()
    {
        if (subordinateNumber == int.MaxValue) subordinateNumber = 0;

        return $"Allied Boat {++subordinateNumber}";
    }

    public void BuildBoat()
    {
        Vector3 formationPosition = GetNextSubordinateForrmationPosition();
        AIBoatController subordinate = ObjectPoolManager.Instance.Spawn<AIBoatController>(transform.position + transform.TransformVector(formationPosition), transform.rotation);
        subordinate.Boat.SetName(GetSubordinateName());
        subordinate.SetFormationPosition(formationPosition);
        subordinate.TrySetCommand(Command);
        AddSubordinate(subordinate.Boat);
    }

    public void SetDefaultFormation(Formation _formation)
    {
        defaultFormation = _formation;
        Vector3[] positions = Formations.GetFleetPositions(defaultFormation, Subordinates.Count);

        for (int i = 0; i < Subordinates.Count; i++)
        {
            Subordinates[i].SetFormationPosition(positions[i]);
        }
    }

    private Vector3 GetNextSubordinateForrmationPosition()
    {
        return Formations.GetFleetPositions(defaultFormation, Subordinates.Count + 1)[Subordinates.Count];
    }
}
