using System;
using UnityEngine;

public class Bouyancy : MonoBehaviour
{
    public const float POINT_SIZE = 2f;
    public static readonly Vector3 POINT_SCALE = new(POINT_SIZE, POINT_SIZE, POINT_SIZE);

    public float bouyancy = 1f;
    public int StartIndex { get; private set; }
    public int EndIndex { get; private set; }

    public BuoyancyPoints Points;
    public float PointMass {  get; private set; }
    public Rigidbody Target {  get; private set; }

#if UNITY_EDITOR
    public BuoyancyDebugInfo Info = new();
#endif

    private void Awake()
    {
        Target = GetComponent<Rigidbody>();
        PointMass = Target.mass / Points.Values.Length;
    }

    public void SetIndex(int start, int end)
    {
        StartIndex = start;
        EndIndex = end;
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

    public void AddDisplacement(float value)
    {
        displacement += value;
    }

    public void CalculateSubmergedPercentage(float mass)
    {
        submergedPercentage = displacement / mass;
    }
}
#endif