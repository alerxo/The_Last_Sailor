using System.Collections;
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

    private const int MAX_UPDATE_COUNT = 36;
    private const int HIGH_QUALITY_UPDATE_COUNT = 34;

    [SerializeField] private BuoyancyPoints buoyancyPoints;

    private readonly List<Buoyancy> targets = new();

    private readonly List<Buoyancy> highQuality = new();
    private readonly List<Buoyancy> lowQuality = new();

    private int highQualityIndex = 0;
    private int lowQualityIndex = 0;

    private const int HIGH_QUALITY_RANGE = 700;

    private bool isQualityCheckRunning = false;

    private WaterSurface waterSurface;

    private NativeArray<float3> targetPositionBuffer;
    private NativeArray<float3> projectedPositionBuffer;
    private NativeArray<float> errorBuffer;
    private NativeArray<float3> candidatePositionBuffer;
    private NativeArray<int> stepCountBuffer;
    private NativeArray<float3> normalBuffer;
    private NativeArray<float3> directionBuffer;

    private Transform player;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        waterSurface = FindFirstObjectByType<WaterSurface>();

        targetPositionBuffer = new(buoyancyPoints.Values.Length * MAX_UPDATE_COUNT, Allocator.Persistent);
        projectedPositionBuffer = new(buoyancyPoints.Values.Length * MAX_UPDATE_COUNT, Allocator.Persistent);
        errorBuffer = new(buoyancyPoints.Values.Length * MAX_UPDATE_COUNT, Allocator.Persistent);
        candidatePositionBuffer = new(buoyancyPoints.Values.Length * MAX_UPDATE_COUNT, Allocator.Persistent);
        stepCountBuffer = new(buoyancyPoints.Values.Length * MAX_UPDATE_COUNT, Allocator.Persistent);
        normalBuffer = new(buoyancyPoints.Values.Length * MAX_UPDATE_COUNT, Allocator.Persistent);
        directionBuffer = new(buoyancyPoints.Values.Length * MAX_UPDATE_COUNT, Allocator.Persistent);

        AddTarget(FindFirstObjectByType<Buoyancy>());
    }

    private void Start()
    {
        player = PlayerBoatController.Instance.transform;
    }

    private void Update()
    {
        if (!isQualityCheckRunning)
        {
            isQualityCheckRunning = true;
            StartCoroutine(CheckQuality());
        }

        GetTargets();
        UpdateTargetsDepthValues();
    }

    private IEnumerator CheckQuality()
    {
        for (int i = 0; i < lowQuality.Count; i++)
        {
            float distance = Vector3.Distance(player.position, lowQuality[i].transform.position);

            if (distance <= HIGH_QUALITY_RANGE)
            {
                highQuality.Add(lowQuality[i]);
                lowQuality.RemoveAt(i);
                i--;
            }

            yield return null;
        }

        for (int i = 0; i < highQuality.Count; i++)
        {
            float distance = Vector3.Distance(player.position, highQuality[i].transform.position);

            if (distance > HIGH_QUALITY_RANGE)
            {
                lowQuality.Add(highQuality[i]);
                highQuality.RemoveAt(i);
                i--;
            }

            yield return null;
        }

        isQualityCheckRunning = false;
    }

    private void GetTargets()
    {
        targets.Clear();

        for (int i = 0; i < HIGH_QUALITY_UPDATE_COUNT; i++)
        {
            if (highQualityIndex >= highQuality.Count)
            {
                highQualityIndex = 0;
                break;
            }

            targets.Add(highQuality[highQualityIndex]);
            highQualityIndex++;
        }

        for (int i = targets.Count; i < MAX_UPDATE_COUNT; i++)
        {
            if (lowQualityIndex >= lowQuality.Count)
            {
                lowQualityIndex = 0;
                break;
            }

            targets.Add(lowQuality[lowQualityIndex]);
            lowQualityIndex++;
        }

        Assert.IsTrue(targets.Count <= MAX_UPDATE_COUNT);
    }

    private void UpdateTargetsDepthValues()
    {
        WaterSimSearchData waterSimSearchData = new();

        if (!waterSurface.FillWaterSearchData(ref waterSimSearchData))
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            int offset = buoyancyPoints.Values.Length * i;

            for (int j = 0; j < buoyancyPoints.Values.Length; j++)
            {
                targetPositionBuffer[offset + j] = targets[i].transform.position + targets[i].transform.TransformVector(buoyancyPoints.Values[j]);
            }
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

        JobHandle jobHandle = waterSimulationSearchJob.Schedule(buoyancyPoints.Values.Length * MAX_UPDATE_COUNT, 1);
        jobHandle.Complete();

        for (int i = 0; i < targets.Count; i++)
        {
            int offset = buoyancyPoints.Values.Length * i;

            for (int j = 0; j < buoyancyPoints.Values.Length; j++)
            {
                targets[i].DepthValues[j] = Mathf.Clamp(projectedPositionBuffer[offset + j].y - targetPositionBuffer[offset + j].y, 0, Buoyancy.POINT_SCALE.y) / Buoyancy.POINT_SCALE.y;
            }
        }
    }

    public void AddTarget(Buoyancy _buoyancy)
    {
        if (!lowQuality.Contains(_buoyancy) && !highQuality.Contains(_buoyancy))
        {
            lowQuality.Add(_buoyancy);
        }
    }

    public void RemoveTarget(Buoyancy _buoyancy)
    {
        if (lowQuality.Contains(_buoyancy))
        {
            lowQuality.Remove(_buoyancy);
        }

        if (highQuality.Contains(_buoyancy))
        {
            highQuality.Remove(_buoyancy);
        }
    }

    public Vector3 GetPointOnWater(Vector3 _target)
    {
        if(waterSurface == null)
        {
            return _target;
        }

        WaterSearchParameters searchParameters = new();
        WaterSearchResult searchResult = new();

        searchParameters.startPositionWS = searchResult.candidateLocationWS;
        searchParameters.targetPositionWS = _target;
        searchParameters.error = 0.01f;
        searchParameters.maxIterations = 8;

        waterSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult);

        return searchResult.projectedPositionWS;
    }
}