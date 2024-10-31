using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Bouyancy : MonoBehaviour
{
    public const float POINT_SIZE = 4f;
    public static readonly Vector3 POINT_SCALE = new(POINT_SIZE, POINT_SIZE, POINT_SIZE);

    [SerializeField] private float bouyancy = 1f;

    private WaterSurface waterSurface;

    private NativeArray<float3> targetPositionBuffer;
    private NativeArray<float3> projectedPositionBuffer;
    private NativeArray<float> errorBuffer;
    private NativeArray<float3> candidatePositionBuffer;
    private NativeArray<int> stepCountBuffer;
    private NativeArray<float3> normalBuffer;
    private NativeArray<float3> directionBuffer;

    [SerializeField] private Transform pointParent;
    private Transform[] points;
    private float massPerPoint;
    private Rigidbody target;

    public BuoyancyDebugInfo Info = new();

    private void Awake()
    {
        target = GetComponent<Rigidbody>();
        waterSurface = FindFirstObjectByType<WaterSurface>();

        points = pointParent.GetComponentsInChildren<Transform>();
        massPerPoint = target.mass / points.Length;

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
        Info.Reset();

        WaterSimSearchData waterSimSearchData = new();
        if (!waterSurface.FillWaterSearchData(ref waterSimSearchData)) return;

        for (int i = 0; i < points.Length; i++)
        {
            targetPositionBuffer[i] = points[i].position;
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

        for (int i = 0; i < points.Length; ++i)
        {
            float submergedDepth = points[i].position.y - projectedPositionBuffer[i].y;

            if (submergedDepth < 0)
            {
                target.AddForceAtPosition(new Vector3(0f, massPerPoint * bouyancy, 0f), points[i].position, ForceMode.Force);

                Info.AddDisplacement(massPerPoint);
                DebugUtil.DrawBox(points[i].position, transform.rotation, POINT_SCALE * 0.99f, Color.red, Time.fixedDeltaTime);
            }

            else
            {
                DebugUtil.DrawBox(points[i].position, transform.rotation, POINT_SCALE * 0.99f, Color.green, Time.fixedDeltaTime);
            }
        }

        Info.CalculateSubmergedPercentage(target.mass);
    }
}

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
