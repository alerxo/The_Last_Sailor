using UnityEngine;

public class FirstPersonController : MonoBehaviour
{



    void Start()
    {
        rb=GetComponent<Rigidbody>();
        
    }
    private Rigidbody rb;
    void Update()
    {
        PlayerMove();
        PlayerRotation();

    }

    void FixedUpdate()
    {   
        rb.MovePosition(rb.position+smoothedMoveDir*Time.fixedDeltaTime);

    }
    private Vector3 smoothedMoveDir;

    [SerializeField]private LayerMask PlayerLayer;
    [SerializeField]private Transform GroundCheck;

    [SerializeField]private float PlayerSpeed;
    [SerializeField]private float SprintMult;

    [SerializeField]private float JumpHeight;

    [SerializeField]private float PlayerAcc;


    private bool Grounded;

    public float mouseSensitivity;
    public Transform camT;
    private void PlayerRotation() 
    {
        float yaw;
        float pitch;
		yaw = Input.GetAxisRaw ("Mouse X") *  mouseSensitivity*Time.deltaTime;
		pitch = Input.GetAxisRaw ("Mouse Y") * mouseSensitivity*Time.deltaTime;
        //transform.Rotate(0,yaw,0);

    }

    private float GroundForceTime;
    private void PlayerMove()
    {
        GroundForceTime +=Time.deltaTime;

        if(Physics.CheckSphere(GroundCheck.position,0.1f,~PlayerLayer))
        {
            Grounded=true;
        }
        else
        {
            Grounded=false;
        }

        Vector3 keyBoardInput = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")); 
        keyBoardInput=keyBoardInput.normalized;
        if(Input.GetKey("left shift"))
        {
            keyBoardInput *= SprintMult;
        }
        Vector3 travelingDir = transform.TransformDirection(keyBoardInput*PlayerSpeed);
        smoothedMoveDir = Vector3.Lerp(smoothedMoveDir,travelingDir,PlayerAcc*Time.deltaTime);

        if(Grounded)
        {
            if(Input.GetKeyDown("space"))
            {
                rb.AddRelativeForce(0,JumpHeight,0,ForceMode.VelocityChange);
                GroundForceTime=0;
            }
            else
            {
                if(GroundForceTime>0.3f)// detta görs för att man ska följa följa en slope neråt. ----!!!!!!--Det kanske kan påverka båtens flytkraft--!!!!!!!!!------
                {
                    rb.AddRelativeForce(0,-15f*Time.deltaTime,0,ForceMode.VelocityChange);
                }
                
            }


        }      
    }


}



