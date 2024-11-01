using System.Collections;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    [SerializeField] private float lifeTime, damage;

    private void Start()
    {
        StartCoroutine(LifeTimeTimer());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Health health))
        {
            health.Damage(damage);
        }
    }

    private IEnumerator LifeTimeTimer()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
