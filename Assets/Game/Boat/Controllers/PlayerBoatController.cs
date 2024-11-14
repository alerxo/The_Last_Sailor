using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBoatController : MonoBehaviour
{
    private Boat boat;

    private void Awake()
    {
        boat = GetComponent<Boat>();

        boat.OnDestroyed += Boat_OnDestroyed;
        boat.OnDamaged += Boat_OnDamaged;
    }

    private void Boat_OnDestroyed()
    {
        SceneManager.LoadScene("Game");
    }

    private void Boat_OnDamaged()
    {
        CameraManager.Instance.ShakeCamera(1.5f, 0.7f);
    }
}
