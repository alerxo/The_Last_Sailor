using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Bouyancy : MonoBehaviour
{
    [SerializeField] private float Strength = 2f;
    [SerializeField] private float Depth = 1f;
    [SerializeField] private float AerialDrag = 0.01f;
    [SerializeField] private float SubmergedDrag = 1f;

    private WaterSurface waterSurface;
    private WaterSearchParameters waterSearchParameters = new();
    private WaterSearchResult waterSearchResult = new();

    [SerializeField] private Transform[] points;

    private Rigidbody target;

    private void Awake()
    {
        target = GetComponent<Rigidbody>();
        waterSurface = FindFirstObjectByType<WaterSurface>();
    }

    private void FixedUpdate()
    {
        float submerged = 0f;

        foreach (Transform point in points)
        {
            Debug.DrawLine(point.position, point.position + (Vector3.up * 5), Color.red, Time.fixedDeltaTime);

            waterSearchParameters.startPositionWS = waterSearchResult.candidateLocationWS;
            waterSearchParameters.targetPositionWS = point.position;
            waterSearchParameters.error = 0.01f;
            waterSearchParameters.maxIterations = 8;
            waterSurface.ProjectPointOnWaterSurface(waterSearchParameters, out waterSearchResult);

            target.AddForceAtPosition(Physics.gravity / points.Length, point.position, ForceMode.Acceleration);

            float depth = point.position.y - waterSearchResult.projectedPositionWS.y;

            if (depth < 0)
            {
                float displacement = Mathf.Clamp01((waterSearchResult.projectedPositionWS.y - point.position.y) / Depth) * Strength;
                target.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacement, 0f), point.position, ForceMode.Acceleration);
                target.AddForce(displacement * Time.fixedDeltaTime * -target.linearVelocity, ForceMode.VelocityChange);
                target.AddTorque(displacement * Time.fixedDeltaTime * -target.angularVelocity, ForceMode.VelocityChange);

                submerged += 1f / points.Length;
            }
        }

        target.linearDamping = Mathf.Lerp(AerialDrag, SubmergedDrag, submerged);
    }
}
