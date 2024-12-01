using Unity.Mathematics;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    private const float WATER_HEIGHT = -5f;
    private const float DESTRUCTION_COOLDOWN = 3f;

    private float damage;

    private CannonballState state;
    private IDamageable ignore;

    private float destructionTimer;

    [SerializeField] private AudioSource boatImpact, waterImpact;
    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject waterSplash;

    private Rigidbody rb;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = rb.GetComponentInChildren<MeshRenderer>();
    }

    private void OnEnable()
    {
        SetState(CannonballState.Active);
    }

    private void OnDisable()
    {
        SetState(CannonballState.Disabled);
    }

    public void Update()
    {
        switch (state)
        {
            case CannonballState.Active:
                ActiveState();
                break;

            case CannonballState.PendingDestruction:
                PendingDestructionState();
                break;
        }
    }

    private void PendingDestructionState()
    {
        if ((destructionTimer -= Time.deltaTime) <= 0)
        {
            SetState(CannonballState.Destruction);
        }
    }

    private void ActiveState()
    {
        if (state == CannonballState.Active && transform.position.y < WATER_HEIGHT)
        {
            Destroy(Instantiate(waterSplash,transform.position,quaternion.Euler(0,0,0)),10); // lär behöva enablas / disablas med canon kulan så det poolas. Jag vet inte riktigt hur jag gör det med din kod /viktor
            waterImpact.Play();
            SetState(CannonballState.PendingDestruction);
        }
    }

    private void OnCollisionEnter(Collision _collision)
    {
        if (state == CannonballState.PendingDestruction) return;

        IDamageable _damageable = _collision.gameObject.GetComponentInParent<IDamageable>();

        if (_damageable != ignore && _damageable != null)
        {
            Destroy(Instantiate(explosion,transform.position,transform.rotation),5); // lär behöva enablas / disablas med canon kulan så det poolas. Jag vet inte riktigt hur jag gör det med din kod /viktor

            _damageable.Damage(damage);
            boatImpact.Play();
            SetState(CannonballState.PendingDestruction);
        }
    }

    public void SetValues(IDamageable _ignore, float _damage)
    {
        ignore = _ignore;
        damage = _damage;
    }

    public void SetState(CannonballState _state)
    {
        state = _state;

        switch (state)
        {
            case CannonballState.Disabled:
                destructionTimer = DESTRUCTION_COOLDOWN;
                break;

            case CannonballState.Active:
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
        }
    }
}

public enum CannonballState
{
    Disabled,
    Active,
    PendingDestruction,
    Destruction
}