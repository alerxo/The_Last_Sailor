using System.Collections.Generic;
using UnityEngine;

public abstract class Admiral : MonoBehaviour
{
    public Boat Owner { get; private set; }

    public Admiral Enemy {  get; protected set; }

    public virtual void Awake()
    {
        Owner = GetComponent<Boat>();
    }

    public void SetEnemy(Admiral _enemy)
    {
        Enemy = _enemy;
    }
}
