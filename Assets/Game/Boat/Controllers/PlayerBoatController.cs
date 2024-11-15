using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBoatController : MonoBehaviour
{
    private Boat boat;

    private void Awake()
    {
        boat = GetComponent<Boat>();

        boat.OnDamaged += Boat_OnDamaged;
        boat.OnDestroyed += Boat_OnDestroyed;
    }

    private void Boat_OnDamaged()
    {
        CameraManager.Instance.ShakeCamera(1.5f, 0.7f);
    }

    private void Boat_OnDestroyed()
    {
        StartCoroutine(boat.SinkAtSurface(OnSunk));
    }

    private void OnSunk()
    {
        SceneManager.LoadScene("Game");
    }
}
