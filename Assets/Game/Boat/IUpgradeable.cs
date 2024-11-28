using UnityEngine;

public interface IUpgradeable
{
    public UpgradeTier UpgradeTier { get; set; }
    public float GetUpgradeValue();
}

public enum UpgradeTier
{
    First,
    Second,
    Third
}

public enum UpgradeType
{
    Hull,
    Engine,
    Cannons
}