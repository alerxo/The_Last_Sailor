using UnityEngine;

public class Cannonball : MonoBehaviour
{
    private const float DAMAGE = 10;
    private const float waterHeight = -5f;
    private const float destructionCooldown = 3f;

    private CannonballState state;
    private IDamageable ignore;

    private float destructionTimer;

    [SerializeField] private AudioSource boatImpact, waterImpact;

    private Rigidbody rb;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = rb.GetComponentInChildren<MeshRenderer>();
    }

    private void OnEnable()
    {
        SetState(CannonballState.Flying);
    }

    private void OnDisable()
    {
        SetState(CannonballState.Disabled);
    }

    public void Update()
    {
        switch (state)
        {
            case CannonballState.PendingDestruction:

                if ((destructionTimer -= Time.deltaTime) <= 0)
                {
                    SetState(CannonballState.Destruction);
                }

                break;

            case CannonballState.Flying:

                if (state == CannonballState.Flying && transform.position.y < waterHeight)
                {
                    waterImpact.Play();
                    SetState(CannonballState.PendingDestruction);
                }

                break;
        }
    }

    private void OnCollisionEnter(Collision _collision)
    {
        if (state == CannonballState.PendingDestruction) return;

        IDamageable _damageable = _collision.gameObject.GetComponentInParent<IDamageable>();

        if (_damageable != ignore && _damageable != null)
        {
            _damageable.Damage(DAMAGE);
            boatImpact.Play();
            SetState(CannonballState.PendingDestruction);
        }
    }

    public void SetIgnore(IDamageable _ignore)
    {
        ignore = _ignore;
    }

    public void SetState(CannonballState _state)
    {
        state = _state;

        switch (state)
        {
            case CannonballState.Flying:
                rb.useGravity = true;
                meshRenderer.enabled = true;
                break;

            case CannonballState.PendingDestruction:
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                meshRenderer.enabled = false;
                break;

            case CannonballState.Destruction:
                ObjectPoolManager.Instance.Release(this);
                break;

            case CannonballState.Disabled:
                destructionTimer = destructionCooldown;
                break;

        }
    }
}

public enum CannonballState
{
    Flying,
    PendingDestruction,
    Destruction,
    Disabled
}