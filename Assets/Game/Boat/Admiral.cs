using System.Collections.Generic;
using UnityEngine;

public abstract class Admiral : MonoBehaviour
{
    public string Name { get; private set; } = "Admiral Johnson";
    public Boat Owner { get; private set; }

    public Admiral Enemy { get; protected set; }

    public readonly List<Boat> Fleet = new();

    public virtual void Awake()
    {
        Owner = GetComponent<Boat>();
        Fleet.Add(Owner);
    }

    public void SetEnemy(Admiral _enemy)
    {
        Enemy = _enemy;
    }
}
