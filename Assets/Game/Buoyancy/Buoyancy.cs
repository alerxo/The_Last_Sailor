using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    public static readonly Vector3 POINT_SCALE = new(4f, 3f, 6f);

    public float BuoyancyForce = 1f;
    private float startBuoyancy;

    public BuoyancyPoints Points;

    [HideInInspector] public float[] DepthValues;

    public float PointMass { get; private set; }
    public Rigidbody RigidBody { get; private set; }

#if UNITY_EDITOR
    [SerializeField] private bool IsDebugMode;
    public float displacement;
    public float submergedPercentage;
#endif

    private void Awake()
    {
        RigidBody = GetComponent<Rigidbody>();
        PointMass = RigidBody.mass / Points.Values.Length;
        startBuoyancy = BuoyancyForce;

        DepthValues = new float[Points.Values.Length];

        SetDefault();
    }

    private void OnEnable()
    {
        if (BuoyancyManager.Instance != null)
        {
            BuoyancyManager.Instance.AddTarget(this);
        }
    }

    private void OnDisable()
    {
        if (BuoyancyManager.Instance != null)
        {
            BuoyancyManager.Instance.RemoveTarget(this);
        }
    }

    private void FixedUpdate()
    {
        AddForce();
    }

    private void AddForce()
    {
        Vector3 force = Vector3.zero;
        Vector3 position = Vector3.zero;
        float positionCount = 0;

        for (int i = 0; i < Points.Values.Length; i++)
        {
            if (DepthValues[i] > 0)
            {
                force += new Vector3(0f, PointMass * BuoyancyForce * DepthValues[i] * -Physics.gravity.y, 0f);
                position += transform.TransformVector(Points.Values[i]) * DepthValues[i];
                positionCount += DepthValues[i];
            }
        }

        if (positionCount > 0)
        {
            RigidBody.AddForceAtPosition(force, transform.position + (position / positionCount), ForceMode.Force);
        }

        DebugDraw();
    }

    private void DebugDraw()
    {
#if UNITY_EDITOR
        if (!IsDebugMode) return;

        displacement = 0;
        submergedPercentage = 0;

        for (int i = 0; i < Points.Values.Length; i++)
        {
            if (DepthValues[i] > 0)
            {
                displacement += PointMass * DepthValues[i];
                DebugUtil.DrawBox(transform.position + transform.TransformVector(Points.Values[i]), transform.rotation, POINT_SCALE * DepthValues[i], Color.green, Time.fixedDeltaTime);
            }

            else
            {
                DebugUtil.DrawBox(transform.position + transform.TransformVector(Points.Values[i]), transform.rotation, POINT_SCALE * 0.05f, Color.red, Time.fixedDeltaTime);
            }
        }

        submergedPercentage = displacement / RigidBody.mass;
#endif
    }

    public void SetDefault()
    {
        BuoyancyForce = startBuoyancy;
    }
}