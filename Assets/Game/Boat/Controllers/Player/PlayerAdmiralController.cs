using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
        Quaternion rotation = transform.rotation;
        Vector3 direction = rotation.eulerAngles;
        direction.x = 0;
        direction.z = 0;
        rotation = Quaternion.Euler(direction);

        Vector3 formationPosition = GetNextSubordinateForrmationPosition();
        Vector3 position = GetAvaliablePosition(formationPosition, rotation.eulerAngles);
        position.y = transform.position.y;

        AIBoatController subordinate = ObjectPoolManager.Instance.Spawn<AIBoatController_Allied>(position, rotation).GetComponent<AIBoatController>();
        subordinate.Boat.SetName(GetSubordinateName());
        subordinate.SetFormationPosition(formationPosition);
        subordinate.TrySetCommand(Command);
        AddSubordinate(subordinate.Boat);
    }

    private Vector3 GetAvaliablePosition(Vector3 _position, Vector3 _rotation)
    {
        Vector3 origin = transform.position + transform.TransformVector(_position);
        Vector3 position = origin;

        for (int i = 0; i < 1000; i++)
        {
            if (Physics.BoxCast(position, new Vector3(6f, 12f, 25f), _rotation))
            {
                position = origin + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * i;
            }

            else
            {
                return position;
            }
        }

        Debug.LogWarning("No avaliable position found");

        return origin;
    }

    public void SetDefaultFormation(Formation _formation)
    {
        defaultFormation = _formation;

        List<Vector3> positions = Formations.GetFleetPositions(defaultFormation, Subordinates.Count).ToList();
        List<AIBoatController> unassigned = Subordinates.GetRange(0, positions.Count);

        while (positions.Count > 0)
        {
            float bestDistance = float.MaxValue;
            AIBoatController bestBoatController = null;
            Vector3? bestPosition = null;

            foreach (AIBoatController boatController in unassigned)
            {
                foreach (Vector3 position in positions)
                {
                    float distance = Vector3.Distance(boatController.GetPositionRelativeToAdmiral(position), boatController.transform.position);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestBoatController = boatController;
                        bestPosition = position;
                    }
                }
            }

            if (bestBoatController == null || bestPosition == null)
            {
                Debug.LogError("Failed assigning formation");
                break;
            }

            bestBoatController.SetFormationPosition(bestPosition);
            positions.Remove(bestPosition.Value);
            unassigned.Remove(bestBoatController);
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
