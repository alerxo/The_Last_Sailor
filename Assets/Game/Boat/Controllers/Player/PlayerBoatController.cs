using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class PlayerBoatController : MonoBehaviour
{
    public static PlayerBoatController Instance;

    public PlayerAdmiralController AdmiralController { get; private set; }
    public Boat Boat { get; private set; }

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        AdmiralController = GetComponent<PlayerAdmiralController>();
        Boat = GetComponent<Boat>();

        Boat.OnDamaged += Boat_OnDamaged;
        Boat.OnDestroyed += Boat_OnDestroyed;
    }

    private void Boat_OnDamaged()
    {
        CameraManager.Instance.ShakeCamera(0.3f, 2.5f, 0.5f, 0.1f);
    }

    private void Boat_OnDestroyed()
    {
        StartCoroutine(Boat.SinkAtSurface(OnSunk));
    }

    private void OnSunk()
    {
        SceneManager.LoadScene("Game");
    }
}
