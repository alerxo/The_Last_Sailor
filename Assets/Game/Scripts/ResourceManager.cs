using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public static event UnityAction<float> OnResourceAmountChanged;

    public const int GAIN_FROM_SCRAPPING_AMOUNT = 10;
    private const float COST_FOR_REPAIR_PER_DURABILITY = 0.1f;
    private const int COST_FOR_BUILD = 10;
    public float Amount { get; private set; } = 1000;

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

    public static int GetRepairCost(Boat _boat)
    {
        return (int)((100 - _boat.GetPercentageDurability()) * COST_FOR_REPAIR_PER_DURABILITY);
    }

    public bool CanRepair(Boat _boat)
    {
        return Amount >= GetRepairCost(_boat);
    }

    public void BoatWasRepaired(Boat _boat)
    {
        AddResource(-GetRepairCost(_boat));
    }

    public int GetBuildCost()
    {
        return COST_FOR_BUILD;
    }

    public bool CanBuild()
    {
        return Amount >= GetBuildCost();
    }

    public void BoatWasBuilt()
    {
        AddResource(-COST_FOR_BUILD);
    }
}