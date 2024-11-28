using UnityEngine;

public interface IUpgradeable
{
    public UpgradeTier UpgradeTier { get; set; }
    public float GetUpgradeValue();
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
    Engine,
    Cannons
}