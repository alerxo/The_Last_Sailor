using System;
using UnityEngine;
using UnityEngine.Audio;
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
                int n = Random.Range(1, audioClips.Length);
                AudioClip clip = audioClips[n];
                audioSource.clip = clip;
                audioSource.pitch = Random.Range(0.6f, 1.2f);
                audioSource.PlayOneShot(clip);
                audioClips[n] = audioClips[0];
                audioClips[0] = clip;
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

