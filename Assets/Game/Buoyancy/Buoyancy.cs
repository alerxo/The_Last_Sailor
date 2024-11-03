using System;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    public static readonly Vector3 POINT_SCALE = new(2f, 2f, 4f);

    public float BuoyancyForce = 1f;

    public BuoyancyPoints Points;
    public float PointMass { get; private set; }
    public Rigidbody Rb { get; private set; }

#if UNITY_EDITOR
    public BuoyancyDebugInfo Info = new();
#endif

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        PointMass = Rb.mass / Points.Values.Length;
    }

    private void OnEnable()
    {
        if(BuoyancyManager.Instance != null)
        {
            BuoyancyManager.Instance.SetBuffers();
        }   
    }

    private void OnDisable()
    {
        if (BuoyancyManager.Instance != null)
        {
            BuoyancyManager.Instance.SetBuffers(this);
        }
    }
}

#if UNITY_EDITOR
[Serializable]
public struct BuoyancyDebugInfo
{
    public float displacement;
    public float submergedPercentage;

    public void Reset()
    {
        displacement = 0;
        submergedPercentage = 0;
    }

    public void AddDisplacement(float _value)
    {
        displacement += _value;
    }

    public void MakeCalculations(float _Value)
    {
        submergedPercentage = displacement / _Value;
    }
}
#endif