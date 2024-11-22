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

    [SerializeField] private BuoyancyPoints buoyancyPoints;
    private readonly List<Buoyancy> targets = new();
    private int currentTarget;

    private WaterSurface waterSurface;

    private NativeArray<float3> targetPositionBuffer;
    private NativeArray<float3> projectedPositionBuffer;
    private NativeArray<float> errorBuffer;
    private NativeArray<float3> candidatePositionBuffer;
    private NativeArray<int> stepCountBuffer;
    private NativeArray<float3> normalBuffer;
    private NativeArray<float3> directionBuffer;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        waterSurface = FindFirstObjectByType<WaterSurface>();

        targetPositionBuffer = new(buoyancyPoints.Values.Length, Allocator.Persistent);
        projectedPositionBuffer = new(buoyancyPoints.Values.Length, Allocator.Persistent);
        errorBuffer = new(buoyancyPoints.Values.Length, Allocator.Persistent);
        candidatePositionBuffer = new(buoyancyPoints.Values.Length, Allocator.Persistent);
        stepCountBuffer = new(buoyancyPoints.Values.Length, Allocator.Persistent);
        normalBuffer = new(buoyancyPoints.Values.Length, Allocator.Persistent);
        directionBuffer = new(buoyancyPoints.Values.Length, Allocator.Persistent);

        AddTarget(FindFirstObjectByType<Buoyancy>());
    }

    private void Update()
    {
        if (targets.Count > 0)
        {
            if (currentTarget >= targets.Count)
            {
                currentTarget = 0;
            }

            UpdateTargetDepthValues();

            currentTarget++;
        }
    }

    private void UpdateTargetDepthValues()
    {
        WaterSimSearchData waterSimSearchData = new();

        if (!waterSurface.FillWaterSearchData(ref waterSimSearchData))
        {
            return;
        }

        for (int i = 0; i < buoyancyPoints.Values.Length; i++)
        {
            targetPositionBuffer[i] = targets[currentTarget].transform.position + targets[currentTarget].transform.TransformVector(buoyancyPoints.Values[i]);
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

        JobHandle jobHandle = waterSimulationSearchJob.Schedule(buoyancyPoints.Values.Length, 1);
        jobHandle.Complete();

        for (int i = 0; i < buoyancyPoints.Values.Length; i++)
        {
            targets[currentTarget].DepthValues[i] = Mathf.Clamp(projectedPositionBuffer[i].y - targetPositionBuffer[i].y, 0, Buoyancy.POINT_SCALE.y) / Buoyancy.POINT_SCALE.y;
        }
    }

    public void AddTarget(Buoyancy _buoyancy)
    {
        if (!targets.Contains(_buoyancy))
        {
            targets.Add(_buoyancy);
        }
    }

    public void RemoveTarget(Buoyancy _buoyancy)
    {
        if (targets.Contains(_buoyancy))
        {
            targets.Remove(_buoyancy);
        }
    }
}