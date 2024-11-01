using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BuoyancyManager : MonoBehaviour
{
    private Bouyancy[] targets;
    private Vector3[] points;

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
        waterSurface = FindFirstObjectByType<WaterSurface>();

        SetBuffers();
    }

    private void SetBuffers()
    {
        List<Vector3> tempPoints = new();
        List<Bouyancy> tempTargets = new();

        foreach (Bouyancy bouyancy in FindObjectsByType<Bouyancy>(FindObjectsSortMode.None))
        {
            int start = tempPoints.Count;

            tempTargets.Add(bouyancy);
            tempPoints.AddRange(bouyancy.Points.Values);

            bouyancy.SetIndex(start, tempPoints.Count);
        }

        targets = tempTargets.ToArray();
        points = tempPoints.ToArray();

        targetPositionBuffer = new(points.Length, Allocator.Persistent);
        projectedPositionBuffer = new(points.Length, Allocator.Persistent);
        errorBuffer = new(points.Length, Allocator.Persistent);
        candidatePositionBuffer = new(points.Length, Allocator.Persistent);
        stepCountBuffer = new(points.Length, Allocator.Persistent);
        normalBuffer = new(points.Length, Allocator.Persistent);
        directionBuffer = new(points.Length, Allocator.Persistent);
    }

    private void FixedUpdate()
    {
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
            if (i >= targets[current].EndIndex)
            {
                current++;
            }

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
            if (i >= targets[current].EndIndex)
            {
                current++;
            }

            if (targetPositionBuffer[i].y - projectedPositionBuffer[i].y < 0)
            {
                targets[current].Target.AddForceAtPosition(new Vector3(0f, targets[current].PointMass * targets[current].bouyancy, 0f), targetPositionBuffer[i], ForceMode.Force);
            }
        }

#if UNITY_EDITOR
        if (!IsDebugMode) return;

        current = 0;

        targets[current].Info.Reset();

        for (int i = 0; i < points.Length; i++)
        {
            if (i >= targets[current].EndIndex)
            {
                targets[current].Info.CalculateSubmergedPercentage(targets[current].Target.mass);
                current++;
                targets[current].Info.Reset();
            }

            if (targetPositionBuffer[i].y - projectedPositionBuffer[i].y < 0)
            {
                targets[current].Info.AddDisplacement(targets[current].PointMass);
                DebugUtil.DrawBox(targetPositionBuffer[i], transform.rotation, Bouyancy.POINT_SCALE * 0.99f, Color.red, Time.fixedDeltaTime);
            }

            else
            {
                DebugUtil.DrawBox(targetPositionBuffer[i], transform.rotation, Bouyancy.POINT_SCALE * 0.99f, Color.green, Time.fixedDeltaTime);
            }
        }

        targets[current].Info.CalculateSubmergedPercentage(targets[current].Target.mass);
#endif
    }
}
