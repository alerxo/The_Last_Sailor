using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public static event UnityAction<float> OnResourceAmountChanged;

    private const int GAIN_PER_ENEMY_SUNK = 20;
    private const int COST_FOR_BUILD = 10;
    private const float COST_FOR_REPAIR_PER_DURABILITY = 0.05f;

    public float Amount { get; private set; } = 1000f;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    public void AddResource(float _amount)
    {
        Amount += _amount;
        OnResourceAmountChanged?.Invoke(Amount);
    }

    public float GetEnemyFleetWorth()
    {
        return CombatManager.Instance.Enemy.Fleet.Count * GAIN_PER_ENEMY_SUNK;
    }

    #region Repair

    public static int GetRepairCost(Boat _boat)
    {
        return (int)((100 - _boat.GetPercentageDurability()) * COST_FOR_REPAIR_PER_DURABILITY);
    }

    public bool CanRepair(Boat _boat)
    {
        return Amount >= GetRepairCost(_boat);
    }

    public void ReparBoat(Boat _boat)
    {
        _boat.Repair();
        AddResource(-GetRepairCost(_boat));
    }

    #endregion

    #region Build

    public int GetBuildCost()
    {
        return COST_FOR_BUILD;
    }

    public bool CanBuild()
    {
        return Amount >= GetBuildCost();
    }

    public void BuildPlayerBoat()
    {
        PlayerBoatController.Instance.AdmiralController.BuildBoat();
        AddResource(-COST_FOR_BUILD);
    }

    #endregion 
}