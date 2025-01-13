using Unity.Mathematics;
using UnityEngine;

public class BirdFormation : MonoBehaviour
{
    [SerializeField] private Transform[] formations;
    private Transform[] birds;
    [SerializeField] private GameObject bird;
    [SerializeField] bool shouldItDieAfterSpawn = false;

    Transform playerT;


    [SerializeField] float formationSpeed = 2f;
    bool wander = true;
    void Awake()
    {
        playerT = GameObject.FindGameObjectWithTag("Player").transform;

        int birdsSpawned = UnityEngine.Random.Range(formations.Length / 2, formations.Length);
        birds = new Transform[birdsSpawned];
        for (int i = 0; i < birdsSpawned; i++)
        {
            birds[i] = Instantiate(bird, formations[i].position, transform.rotation).transform;
            birds[i].GetComponent<Bird>().targetFormation = formations[i];
            birds[i].parent = GameObject.FindGameObjectWithTag("BirdController").transform;
        }
        if (wander)
        {
            SetWanderSpawn();
        }


    }


    void Update()
    {
        if (wander)
        {
            transform.position += transform.forward * formationSpeed * Time.deltaTime;
        }
        if(shouldItDieAfterSpawn)
        {
            if(Vector3.Distance(transform.position, playerT.transform.position)>500)
            {
                KillFormation();
            }
        }
    }

    public void SetWanderSpawn()
    {
        float height = UnityEngine.Random.Range(35f, 80f);
        float xDir = UnityEngine.Random.Range(-10f, 10f);
        float zDir = UnityEngine.Random.Range(-10f, 10f);
        float spawnDistance = UnityEngine.Random.Range(150, 350);

        Vector3 spawnDir = new Vector3(xDir, 0, zDir).normalized;
        if(!shouldItDieAfterSpawn)
        {
            transform.position = new Vector3(playerT.position.x + (spawnDir * spawnDistance).x, height, playerT.position.z + (spawnDir * spawnDistance).z);

            transform.LookAt(playerT.position);
            float randomFlightDirOffset = UnityEngine.Random.Range(-30f, 30f);
            transform.rotation = quaternion.Euler(0, transform.eulerAngles.y + randomFlightDirOffset, 0);
        }

        

        foreach(Transform bird in birds)
        {
            bird.position = transform.position;
        }

    }

    void Wander()
    {
        transform.position += transform.forward * formationSpeed * Time.deltaTime;
    }

    public void KillFormation()
    {
        Destroy(gameObject, 10);
        foreach (Transform bird in birds)
        {
            if(bird!=null)
            bird.GetComponent<Bird>().die();
        }
    }


}
