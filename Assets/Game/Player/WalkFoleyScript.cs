using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WalkFoleyScript : MonoBehaviour
{
    public AudioClip[] audioClips;
    public float minTimeBetweenFootsteps;
    public float maxTimeBetweenFootsteps;

    public float minTimeBetweenFootstepsNormal = 0.3f;
    public float maxTimeBetweenFootstepsNormal = 0.6f;

    public float minTimeBetweenFootstepsSprint = 0.2f;
    public float maxTimeBetweenFootstepsSprint = 0.4f;

    private AudioSource audioSource;
    private bool isWalking;
    private float timeSinceLastFootstep;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        minTimeBetweenFootsteps = minTimeBetweenFootstepsNormal;
        maxTimeBetweenFootsteps = maxTimeBetweenFootstepsNormal;
    }

    private void Update()
    {
        if (isWalking)
        {
            if(Time.time - timeSinceLastFootstep >= Random.Range(minTimeBetweenFootsteps,maxTimeBetweenFootsteps))
            {
                AudioClip footstepSound = audioClips[Random.Range(0, audioClips.Length)];
                audioSource.PlayOneShot(footstepSound);
                timeSinceLastFootstep = Time.time;
            }
        }
    }
    public void StartWalking() 
    {
        isWalking = true;
    }
    public void StopWalking()
    {
        isWalking = false;
    }
}

