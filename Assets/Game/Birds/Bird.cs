using System;
using UnityEditor.Rendering;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public bool glide = false;
    public Transform targetFormation;
    [SerializeField] Animator animator;

    [SerializeField] float minGlideTime = 2;
    [SerializeField] float maxGlideTime = 9;
    float nextGlide;
    float nextGlideTimer = 0;

    bool dying = false;

    [SerializeField] float birdSpeed = 2.2f;

    Vector3 targetScale;
    Vector3 startScale;
    void Start()
    {
        float rScale = UnityEngine.Random.Range(0.85f, 1.15f);
        targetScale = transform.GetChild(0).localScale * rScale;
        if (rScale < 1f)
        {
            nextGlide /= 3;
        }
        
        transform.GetChild(0).localScale = Vector3.zero;
        nextGlide = getNextGlide();
        
    }
    public void Begin()
    {
        
    }

    void Update()
    {
        nextGlideTimer += Time.deltaTime;
        if (nextGlideTimer > nextGlide)
        {
            nextGlideTimer = 0;
            nextGlide = getNextGlide();
            if (glide)
            {
                nextGlide /= 2;
            }

            SetGlide(!glide);

        }

        if (targetFormation != null)
        {
            updatePositionRotation();
        }


        if(dying)
        {
            transform.GetChild(0).localScale = Vector3.Lerp(transform.GetChild(0).localScale,Vector3.zero,0.3f*Time.deltaTime);
        }
        else
        {
            transform.GetChild(0).localScale = Vector3.Lerp(transform.GetChild(0).localScale,targetScale,0.3f*Time.deltaTime);
        }

    }

    Vector3 targetPositionOffset = Vector3.zero;
    Vector3 targetPositionOffesApplied = Vector3.zero;
    float newOffserTimer = 20;
    float nextChange = 3;
    void updatePositionRotation()
    {
        newOffserTimer += Time.deltaTime;
        if (newOffserTimer > nextChange)
        {
            nextChange = UnityEngine.Random.Range(1f, 4f);
            newOffserTimer = 0;
            targetPositionOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        }
        targetPositionOffesApplied = Vector3.Lerp(targetPositionOffesApplied, targetPositionOffset, birdSpeed / 20 * Time.deltaTime);
        transform.LookAt(targetFormation.position + targetPositionOffesApplied);
        transform.position = Vector3.Lerp(transform.position, targetFormation.position + targetPositionOffesApplied, birdSpeed * Time.deltaTime);

    }


    public void SetGlide(bool newGlide)
    {
        glide = newGlide;
        animator.SetBool("Glide", newGlide);

    }
    float getNextGlide()
    {
        return UnityEngine.Random.Range(minGlideTime, maxGlideTime);
    }



    public void die()
    {
        Destroy(gameObject,8f);
        dying = true;
    }


}
