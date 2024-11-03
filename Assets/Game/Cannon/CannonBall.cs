using System.Collections;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    private CannonballState state;
    private IDamageable ignore;

    [SerializeField] private float damage;
    [SerializeField] private AudioSource boatImpact, waterImpact;

    private const float waterHeight = -5f;

    private void OnEnable()
    {
        state = CannonballState.Flying;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public void Update()
    {
        if (state == CannonballState.PendingDestruction && (!boatImpact.isPlaying && !waterImpact.isPlaying))
        {
            ObjectPoolManager.Instance.ReleaseCannonball(this);
        }

        else if (state == CannonballState.Flying && transform.position.y < waterHeight)
        {
            waterImpact.Play();
            state = CannonballState.PendingDestruction;
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
            state = CannonballState.PendingDestruction;
        }
    }

    public void SetIgnore(IDamageable _ignore)
    {
        ignore = _ignore;
    }
}

public enum CannonballState
{
    Flying,
    PendingDestruction
}