using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public CannonState State { get; private set; }

    [SerializeField] private float force, fireCooldown;
    [SerializeField] private CannonBall cannonBallPrefab;
    [SerializeField] private Transform cannonBallSpawnPosition, explosionPosition;

    private float angle;
    private const float minAngle = 0f;
    private const float maxAngle = 40f;

    public void ChangeAngle(float _value)
    {
        angle = Mathf.Clamp(angle + _value, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(angle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    public void Fire()
    {
        CannonBall cannonBall = Instantiate(cannonBallPrefab, cannonBallSpawnPosition.position, Quaternion.identity);
        cannonBall.GetComponent<Rigidbody>().AddExplosionForce(force, explosionPosition.position, 0, 0);

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