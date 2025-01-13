using UnityEngine;

public class BirdController : MonoBehaviour
{
    [SerializeField] int maxWandererBirds = 5;
    BirdFormation[] wanderers;
    [SerializeField] GameObject pfBirdFormation;
    Transform playerT;
    void Start()
    {
        wanderers = new BirdFormation[maxWandererBirds];
        for(int i = 0;i<maxWandererBirds;i++)
        {
            StartWanderer(i);
        }
        playerT =  GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        ControllWanderers();

    }

    void ControllWanderers()
    {
        for(int i = 0;i<wanderers.Length;i++) 
        {
            if(wanderers[i]!=null)
            if(Vector3.Distance(wanderers[i].transform.position, playerT.position)>600)
            {
                wanderers[i].SetWanderSpawn();
            }
            if(Vector3.Distance(wanderers[i].transform.position, playerT.position)>1000)//tror inte att den någonsin kommer att kallas men kan vara bra ifall
            {
                wanderers[i].KillFormation();
                StartWanderer(i);
            }
        }
        

    }

    void StartWanderer(int index)
    {
        wanderers[index] = Instantiate(pfBirdFormation.GetComponent<BirdFormation>());
        wanderers[index].transform.parent = transform;
        
    }

}
