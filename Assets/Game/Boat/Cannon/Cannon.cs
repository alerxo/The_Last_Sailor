using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public CannonState State { get; private set; }

    [SerializeField] private float force, radius, upwardsModifier, fireCooldown;
    [SerializeField] private CannonBall cannonBallPrefab;
    [SerializeField] private Transform cannonBallSpawnPosition, explosionPosition;

    public void Fire()
    {
        CannonBall cannonBall = Instantiate(cannonBallPrefab, cannonBallSpawnPosition.position, Quaternion.identity);
        cannonBall.GetComponent<Rigidbody>().AddExplosionForce(force, explosionPosition.position, radius, upwardsModifier);

        State = CannonState.Reloading;
        StartCoroutine(ReloadTimer());
    }

    private IEnumerator ReloadTimer()
    {
        yield return new WaitForSeconds(fireCooldown);
        State = CannonState.Ready;
    }
}

public enum CannonState
{
    Ready,
    Reloading
}