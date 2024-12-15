using UnityEngine;

public interface IUpgradeable
{
    public UpgradeTier UpgradeTier { get; set; }
    public float UpgradeIncrease { get; }
    public float GetUpgradeValue { get; }
}

public enum UpgradeTier
{
    One,
    Two,
    Three
}

public enum UpgradeType
{
    Hull,
    Cannons,
}