using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Bouyancy : MonoBehaviour
{
    [SerializeField] private float bouyancy = 1f;
    [SerializeField] private float hullDepth = 1f;

    private WaterSurface waterSurface;
    private WaterSearchParameters waterSearchParameters = new();
    private WaterSearchResult waterSearchResult = new();

    [SerializeField] private Transform[] points;
    private Rigidbody target;

    [SerializeField] private float averageSubmergedDepth, averageSubmergedPercentage, totalDisplacement;

    private void Awake()
    {
        target = GetComponent<Rigidbody>();
        waterSurface = FindFirstObjectByType<WaterSurface>();
    }

    private void FixedUpdate()
    {
        averageSubmergedDepth = 0f;
        averageSubmergedPercentage = 0f;
        totalDisplacement = 0f;

        foreach (Transform point in points)
        {
            waterSearchParameters.startPositionWS = waterSearchResult.candidateLocationWS;
            waterSearchParameters.targetPositionWS = point.position;
            waterSearchParameters.error = 0.01f;
            waterSearchParameters.maxIterations = 8;
            waterSurface.ProjectPointOnWaterSurface(waterSearchParameters, out waterSearchResult);

            float submergedDepth = point.position.y - waterSearchResult.projectedPositionWS.y;
            float submergedPercentage = -(Mathf.Clamp(submergedDepth, -hullDepth, 0) / hullDepth);

            if (submergedPercentage > 0)
            {
                float displacement = submergedPercentage * target.mass * bouyancy / points.Length;
                target.AddForceAtPosition(new Vector3(0f, displacement, 0f), point.position, ForceMode.Force);

                totalDisplacement += displacement;
            }

            averageSubmergedPercentage += submergedPercentage / points.Length;
            averageSubmergedDepth += submergedDepth / points.Length;

            Debug.DrawLine(point.position, point.position + (Vector3.up * hullDepth), Color.red, Time.fixedDeltaTime);
        }
    }
}
