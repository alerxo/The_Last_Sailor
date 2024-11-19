using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    private const float MAX_ROTATION = 170;

    [Tooltip("The rotating part of the mesh")]
    [SerializeField] private Transform rotatingPart;

    public void SetRotation(float rotation)
    {
        rotatingPart.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Lerp(-MAX_ROTATION, MAX_ROTATION, (rotation + 1) / 2)));
    }
}
