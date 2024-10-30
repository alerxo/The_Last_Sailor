using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    private InputSystem_Actions input;

    [SerializeField] private CannonPosition position;
    [SerializeField] private float force, radius, upwardsModifier, fireCooldown;
    [SerializeField] private CannonBall cannonBallPrefab;
    [SerializeField] private Transform cannonBallSpawnPosition, explosionPosition;

    private void Awake()
    {
        input = new InputSystem_Actions();

        if (position == CannonPosition.Left) input.Player.LeftFire.performed += Fire;
        else input.Player.RightFire.performed += Fire;

        input.Player.Enable();
    }

    private void OnDestroy()
    {
        if (position == CannonPosition.Left) input.Player.LeftFire.performed -= Fire;
        else input.Player.RightFire.performed -= Fire;

        StopAllCoroutines();
    }

    private void Fire(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        CannonBall cannonBall = Instantiate(cannonBallPrefab, cannonBallSpawnPosition.position, Quaternion.identity);
        cannonBall.GetComponent<Rigidbody>().AddExplosionForce(force, explosionPosition.position, radius, upwardsModifier);
        input.Player.Disable();
        StartCoroutine(FireCooldown());
    }

    private IEnumerator FireCooldown()
    {
        yield return new WaitForSeconds(fireCooldown);
        input.Player.Enable();
    }
}

public enum CannonPosition
{
    Left,
    Right
}