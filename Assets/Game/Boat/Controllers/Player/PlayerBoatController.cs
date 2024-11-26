using UnityEngine;
using UnityEngine.Assertions;

public class PlayerBoatController : MonoBehaviour
{
    public static PlayerBoatController Instance;

    public PlayerAdmiralController AdmiralController { get; private set; }
    public Boat Boat { get; private set; }

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        Boat = GetComponent<Boat>();
        AdmiralController = GetComponent<PlayerAdmiralController>();
        AdmiralController.SetOwner(Boat);

        Boat.OnDamaged += Boat_OnDamaged;
        Boat.OnDestroyed += Boat_OnDestroyed;
    }

    private void Boat_OnDamaged()
    {
        CameraManager.Instance.ShakeCamera(0.3f, 2.5f, 0.5f, 0.1f);
    }

    private void Boat_OnDestroyed()
    {
        Boat.StartSinkAtSurface();
    }
}
