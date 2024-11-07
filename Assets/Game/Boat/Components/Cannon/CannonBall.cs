using UnityEngine;

public class Cannonball : MonoBehaviour
{
    private CannonballState state;
    private IDamageable ignore;

    [SerializeField] private float damage;
    [SerializeField] private AudioSource boatImpact, waterImpact;

    private const float waterHeight = -5f;
    private float destructionTimer;
    private const float destructionCooldown = 5f;

    private void OnEnable()
    {
        SetState(CannonballState.Flying);
    }

    private void OnDisable()
    {
        SetState(CannonballState.Disabled);
        destructionTimer = 0;
    }

    public void Update()
    {
        DebugUtil.DrawBox(transform.position, Quaternion.identity, Vector3.one, Color.red, Time.deltaTime);

        switch (state)
        {
            case CannonballState.PendingDestruction:

                if ((destructionTimer += Time.deltaTime) > destructionCooldown)
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
            _damageable.Damage(damage);
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
            case CannonballState.Destruction:
                ObjectPoolManager.Instance.ReleaseCannonball(this);
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