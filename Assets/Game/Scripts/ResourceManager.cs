using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public static event UnityAction<float> OnResourceAmountChanged;

    public const int UPGRADE_COST = 5;
    private const int BUILD_NEW_BOAT_COST = 10;
    private const int MAX_REPAIR_COST = 5;

    private static readonly int[] subodinateUpgradeCosts = { 0, 10, 30, 50, 50 };

    public float Amount { get; private set; } = 0f;

#if UNITY_EDITOR
    private InputSystem_Actions input;
#endif

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

#if UNITY_EDITOR
        input = new();
        input.Player.AddResource.performed += AddResource_performed;
        input.Player.Enable();
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        input.Disable();
        input.Player.AddResource.performed -= AddResource_performed;
#endif
    }

#if UNITY_EDITOR
    private void AddResource_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        AddResource(100);
    }
#endif

    public void AddResource(float _amount)
    {
        Amount += _amount;
        OnResourceAmountChanged?.Invoke(Amount);
    }

    public float GetEnemyFleetWorth()
    {
        return CombatManager.Instance.GetRoundResourceWorth();
    }

    #region Repair

    public static int GetRepairCost(Boat _boat)
    {
        return _boat.IsDamaged ? Mathf.FloorToInt(Mathf.Lerp(MAX_REPAIR_COST, 1, (float)_boat.GetPercentageDurability() / 100)) : 0;
    }

    public bool CanRepair(Boat _boat)
    {
        return GetRepairCost(_boat) != 0 && Amount >= GetRepairCost(_boat);
    }

    public void RepairBoat(Boat _boat)
    {
        AddResource(-GetRepairCost(_boat));
        _boat.Repair();
    }

    public static int GetRepairAllCost()
    {
        int total = 0;

        foreach (Boat boat in PlayerBoatController.Instance.AdmiralController.Fleet)
        {
            total += GetRepairCost(boat);
        }

        return total;
    }

    public bool CanRepairAll()
    {
        return GetRepairAllCost() != 0 && Amount >= GetRepairAllCost();
    }

    public void RepairAll()
    {
        foreach (Boat boat in PlayerBoatController.Instance.AdmiralController.Fleet)
        {
            RepairBoat(boat);
        }
    }

    #endregion

    #region Build

    public int GetBuildCost()
    {
        return BUILD_NEW_BOAT_COST;
    }

    public bool CanBuild()
    {
        return Amount >= GetBuildCost();
    }

    public void BuildPlayerBoat()
    {
        PlayerBoatController.Instance.AdmiralController.BuildBoat();
        AddResource(-BUILD_NEW_BOAT_COST);
    }

    #endregion

    #region Upgrade

    public static int GetUpgradeCost()
    {
        return UPGRADE_COST;
    }

    public static int GetSubordinateCapUpgradeCost(int _index)
    {
        return subodinateUpgradeCosts[_index];
    }

    public bool CanUpgradeSubordinateCap(int _index)
    {
        return Amount >= GetSubordinateCapUpgradeCost(_index);
    }

    public bool CanUpgrade()
    {
        return Amount >= GetUpgradeCost();
    }

    public void UpgradeBoat()
    {
        AddResource(-GetUpgradeCost());
    }

    #endregion
}