using Unity.VisualScripting;
using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    private const float MAX_ROTATION = 170;

    [Tooltip("The rotating part of the mesh")]
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private AudioClip audioClip;

    private AudioSource audioSource;
    private bool allowedPlaying;

    private void Update()
    {
        PlayBellSound();
    }

    private void PlayBellSound()
    {
        if (rotatingPart.localRotation.eulerAngles.z <= 20 && rotatingPart.localRotation.eulerAngles.z >= -20 && allowedPlaying == true)
        {
            allowedPlaying = false;
            audioSource.PlayOneShot(audioClip);
        }

        if (rotatingPart.localRotation.eulerAngles.z > 20 || rotatingPart.localRotation.eulerAngles.z < -20) 
        {
            allowedPlaying = true;   
        }
    }
}
