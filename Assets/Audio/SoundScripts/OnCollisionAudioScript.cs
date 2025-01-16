using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class OnCollisionAudioScript : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BoatCollisionTag")) 
        {
            if (collision.relativeVelocity.magnitude > 4 && !audioSource.isPlaying)
            {
                audioSource.transform.position = collision.contacts[0].point;
                audioSource.pitch = Random.Range(0.6f, 1.0f);
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }
        }
        
    }
}
