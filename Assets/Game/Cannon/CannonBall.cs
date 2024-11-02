using System.Collections;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    [SerializeField] private float lifeTime, damage;
    private Health ignore;

    private void Start()
    {
        StartCoroutine(LifeTimeTimer());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void SetIgnore(Health _ignore)
    {
        ignore = _ignore;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Health health) && health != ignore)
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
