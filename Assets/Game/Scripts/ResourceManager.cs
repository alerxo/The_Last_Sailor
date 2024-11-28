using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public static event UnityAction<float> OnResourceAmountChanged;

    public const int GAIN_FROM_SCRAPPING_AMOUNT = 10;
    private const float COST_FOR_REPAIR_PER_DURABILITY = 0.1f;
    public float Amount { get; private set; }

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    public void BoatWasScrapped()
    {
        AddResource(GAIN_FROM_SCRAPPING_AMOUNT);
    }

    public void BoatWasSeized(int _cost)
    {
        BoatWasRepaired(_cost);
    }

    public void BoatWasRepaired(int _cost)
    {
        AddResource(-_cost);
    }

    public void AddResource(float _amount)
    {
        Amount += _amount;
        OnResourceAmountChanged?.Invoke(Amount);
    }

    public bool CanSeize(Boat _boat)
    {
        return CanRepair(_boat);
    }

    public bool CanRepair(Boat _boat)
    {
        return Amount >= GetRepairCost(_boat);
    }

    public static int GetRepairCost(Boat _boat)
    {
        return (int)((100 - _boat.GetPercentageHealth()) * COST_FOR_REPAIR_PER_DURABILITY);
    }
}