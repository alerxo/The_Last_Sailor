using UnityEngine;

public class PlayerAdmiralController : Admiral
{
    public PlayerBoatController PlayerBoatController { get; private set; }

    private static readonly int[] subodinateCaps = { 0, 3, 7, 16, 32 };

    public const int MAX_SUBORDINATE_UPGRADE = 3;
    public int SuborinateUpgradeIndex { get; private set; } = 1;

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
        Vector3 position = transform.position + transform.TransformVector(formationPosition);
        position.y = transform.position.y;

        AIBoatController subordinate = ObjectPoolManager.Instance.Spawn<AIBoatController>(position, transform.rotation);
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

    public int GetSubordinateCap => subodinateCaps[SuborinateUpgradeIndex];
    public int GetSubordinateCapIncrease => subodinateCaps[SuborinateUpgradeIndex + 1] - subodinateCaps[SuborinateUpgradeIndex];
    public int GetSubordinateUpgradeCost => ResourceManager.GetSubordinateCapUpgradeCost(SuborinateUpgradeIndex);
    public bool CanUpgradeSubodinateCap => ResourceManager.Instance.CanUpgradeSubordinateCap(SuborinateUpgradeIndex) && SuborinateUpgradeIndex < MAX_SUBORDINATE_UPGRADE;
    public bool CanBuild => Subordinates.Count < GetSubordinateCap;

    public void UpgradeSuborniateCap()
    {
        ResourceManager.Instance.AddResource(-GetSubordinateUpgradeCost);
        SuborinateUpgradeIndex++;
    }
}
