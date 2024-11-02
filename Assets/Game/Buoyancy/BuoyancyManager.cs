using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.HighDefinition;

public class BuoyancyManager : MonoBehaviour
{
    public static BuoyancyManager Instance { get; private set; }

    private Vector3[] points;
    private Buoyancy[] targets;
    private int[] endIndexes;

    private WaterSurface waterSurface;

    private NativeArray<float3> targetPositionBuffer;
    private NativeArray<float3> projectedPositionBuffer;
    private NativeArray<float> errorBuffer;
    private NativeArray<float3> candidatePositionBuffer;
    private NativeArray<int> stepCountBuffer;
    private NativeArray<float3> normalBuffer;
    private NativeArray<float3> directionBuffer;

    public bool IsDataSet { get; private set; } = false;

#if UNITY_EDITOR
    [SerializeField] private bool IsDebugMode;
#endif

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        waterSurface = FindFirstObjectByType<WaterSurface>();
    }

    private void FixedUpdate()
    {
        if (targets.Length == 0) return;

        GetData();

        if (IsDataSet)
        {
            AddForce();
        }
    }

    private void GetData()
    {
        WaterSimSearchData waterSimSearchData = new();
        if (!waterSurface.FillWaterSearchData(ref waterSimSearchData)) return;

        int current = 0;

        for (int i = 0; i < points.Length; i++)
        {
            if (i == endIndexes[current]) current++;

            targetPositionBuffer[i] = targets[current].transform.position + targets[current].transform.TransformVector(points[i]);
        }

        WaterSimulationSearchJob waterSimulationSearchJob = new()
        {
            simSearchData = waterSimSearchData,

            targetPositionWSBuffer = targetPositionBuffer,
            startPositionWSBuffer = targetPositionBuffer,
            maxIterations = 8,
            error = 0.01f,

            projectedPositionWSBuffer = projectedPositionBuffer,
            errorBuffer = errorBuffer,
            candidateLocationWSBuffer = candidatePositionBuffer,
            stepCountBuffer = stepCountBuffer,
            normalWSBuffer = normalBuffer,
            directionBuffer = directionBuffer
        };

        JobHandle jobHandle = waterSimulationSearchJob.Schedule(points.Length, 1);
        jobHandle.Complete();

        IsDataSet = true;
    }

    private void AddForce()
    {
        int current = 0;

        for (int i = 0; i < points.Length; i++)
        {
            if (i >= endIndexes[current]) current++;

            float depth = Mathf.Clamp(projectedPositionBuffer[i].y - targetPositionBuffer[i].y, 0, Buoyancy.POINT_SCALE.y) / Buoyancy.POINT_SCALE.y;

            if (depth > 0)
            {
                Vector3 force = new(0f, targets[current].PointMass * targets[current].BuoyancyForce * depth * -Physics.gravity.y, 0f);

                targets[current].Rb.AddForceAtPosition(force, targetPositionBuffer[i], ForceMode.Force);
            }
        }

#if UNITY_EDITOR
        if (!IsDebugMode) return;

        foreach (Buoyancy target in targets)
        {
            target.Info.Reset();
        }

        current = 0;

        for (int i = 0; i < points.Length; i++)
        {
            if (i >= endIndexes[current])
            {
                current++;
            }

            float depth = Mathf.Clamp(projectedPositionBuffer[i].y - targetPositionBuffer[i].y, 0, Buoyancy.POINT_SCALE.y) / Buoyancy.POINT_SCALE.y;

            if (depth > 0)
            {
                Vector3 force = new(0f, targets[current].PointMass * targets[current].BuoyancyForce * depth * -Physics.gravity.y, 0f);
                targets[current].Info.AddBuoancy(force);
                targets[current].Info.AddDisplacement(targets[current].PointMass);
                DebugUtil.DrawBox(targetPositionBuffer[i], targets[current].transform.rotation, Buoyancy.POINT_SCALE * depth, Color.green, Time.fixedDeltaTime);
            }

            else
            {
                DebugUtil.DrawBox(targetPositionBuffer[i], targets[current].transform.rotation, Buoyancy.POINT_SCALE * 0.05f, Color.red, Time.fixedDeltaTime);
            }
        }

        foreach (Buoyancy target in targets)
        {
            target.Info.MakeCalculations(target.Rb.mass);
        }
#endif
    }

    public void SetBuffers(Buoyancy _ignore = null)
    {
        List<Vector3> tempPoints = new();
        List<Buoyancy> tempTargets = new();
        List<int> tempEndIndexes = new();

        foreach (Buoyancy buoancy in FindObjectsByType<Buoyancy>(FindObjectsSortMode.None))
        {
            if (_ignore != null && buoancy == _ignore) continue;

            tempPoints.AddRange(buoancy.Points.Values);
            tempTargets.Add(buoancy);
            tempEndIndexes.Add(tempPoints.Count);
        }

        targets = tempTargets.ToArray();
        endIndexes = tempEndIndexes.ToArray();
        points = tempPoints.ToArray();

        targetPositionBuffer = new(points.Length, Allocator.Persistent);
        projectedPositionBuffer = new(points.Length, Allocator.Persistent);
        errorBuffer = new(points.Length, Allocator.Persistent);
        candidatePositionBuffer = new(points.Length, Allocator.Persistent);
        stepCountBuffer = new(points.Length, Allocator.Persistent);
        normalBuffer = new(points.Length, Allocator.Persistent);
        directionBuffer = new(points.Length, Allocator.Persistent);
    }
}
