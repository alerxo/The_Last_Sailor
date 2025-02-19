using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

public class MouseSensitivityManager : MonoBehaviour
{
    public static MouseSensitivityManager Instance { get; private set; }

    public float PlayerMouseSensitivity { get; private set; } = 1f;
    public float SteeringWheelMouseSensitivity { get; private set; } = 1f;
    public float CannonMouseSensitivity { get; private set; } = 1f;

    [SerializeField] private CinemachineInputAxisController playerInputAxisController, steeringWheelInputAxisController;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        SetPlayerMouseSensitivty(PlayerPrefs.HasKey("PlayerMouseSensitivity") ? PlayerPrefs.GetFloat("PlayerMouseSensitivity") : 1);
        SetSteeringWheelMouseSensitivty(PlayerPrefs.HasKey("SteeringWheelMouseSensitivity") ? PlayerPrefs.GetFloat("SteeringWheelMouseSensitivity") : 1);
        SetCannonMouseSensitivty(PlayerPrefs.HasKey("CannonMouseSensitivity") ? PlayerPrefs.GetFloat("CannonMouseSensitivity") : 1);
    }

    public void SetPlayerMouseSensitivty(float _value)
    {
        PlayerMouseSensitivity = _value;

        foreach (var controller in playerInputAxisController.Controllers)
        {
            if (controller.Name == "Look X (Pan)")
            {
                controller.Input.Gain = _value;
            }

            else if (controller.Name == "Look Y (Tilt)")
            {
                controller.Input.Gain = -_value;
            }

            else
            {
                Debug.Log(controller.Name);
            }
        }

        PlayerPrefs.SetFloat("PlayerMouseSensitivity", PlayerMouseSensitivity);
    }

    public void SetSteeringWheelMouseSensitivty(float _value)
    {
        SteeringWheelMouseSensitivity = _value;

        foreach (var controller in steeringWheelInputAxisController.Controllers)
        {
            if (controller.Name == "Look X(Pan)")
            {
                controller.Input.Gain = _value;
            }

            else if (controller.Name == "Look Y(Pan)")
            {
                controller.Input.Gain = -_value;
            }
        }

        PlayerPrefs.SetFloat("SteeringWheelMouseSensitivity", SteeringWheelMouseSensitivity);
    }

    public void SetCannonMouseSensitivty(float _value)
    {
        CannonMouseSensitivity = _value;

        PlayerPrefs.SetFloat("MouseSensitivity", CannonMouseSensitivity);
    }

}

public enum MouseSensitivityType
{
    Player,
    SteeringWheel,
    Cannon
}
