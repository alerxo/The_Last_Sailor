using UnityEngine;

public class ReturnPlayerToShip : MonoBehaviour
{
    Transform playerReturnPosition;
    void Awake()
    {
        playerReturnPosition = GameObject.FindGameObjectWithTag("PlayerReturnPosition").transform;
    }

    void Update()
    {
        if(transform.position.y<-25)
        {
            transform.position = playerReturnPosition.position;
            transform.GetComponent<Rigidbody>().linearVelocity = playerReturnPosition.GetComponentInParent<Rigidbody>().linearVelocity;
        }
        
    }

}
