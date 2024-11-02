using System.Collections;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    private Health ignore;

    [SerializeField] private float lifeTime, damage;
    [SerializeField] private AudioSource boatImpact, waterImpact;
    private MeshRenderer meshRenderer;
    private SphereCollider sphereCollider;
    private Rigidbody rigidBody;

    private const float waterHeight = 0;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        sphereCollider = GetComponentInChildren<SphereCollider>();
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(LifeTimeTimer());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Update()
    {
        if (rigidBody.IsSleeping()) return;

        if (transform.position.y < waterHeight)
        {
            waterImpact.Play();
            Destruction();
        }
    }

    public void SetIgnore(Health _ignore)
    {
        ignore = _ignore;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Health health))
        {
            if (health == ignore) return;

            health.Damage(damage);
            boatImpact.Play();
            Destruction();
        }
    }

    private IEnumerator LifeTimeTimer()
    {
        yield return new WaitForSeconds(lifeTime);
        Destruction();
    }

    private void Destruction()
    {
        rigidBody.Sleep();
        sphereCollider.enabled = false;
        meshRenderer.enabled = false;

        StartCoroutine(DestroyTimer());
    }

    private IEnumerator DestroyTimer()
    {

        while (boatImpact.isPlaying || waterImpact.isPlaying)
        {
            yield return null;
        }

        Destroy(gameObject);
    }
}