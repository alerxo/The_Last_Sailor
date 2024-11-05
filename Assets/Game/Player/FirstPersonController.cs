using Unity.Mathematics;
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
        RotatePlayerTowardsCamera();

        DontLetPlayerSink();//Endast ett test på en metod, kan ta bort
    }

    void DontLetPlayerSink()
    {
        float whenToFloat = -3;
        if(transform.position.y<whenToFloat)
        {
            float f = whenToFloat-transform.position.y;
            if(rb.linearVelocity.y<0)
            {
                f = math.clamp(f,30,2000);
            }
            rb.AddForce(0,f*Time.deltaTime,0,ForceMode.Impulse);

        }
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



    public Transform camT;
    private void RotatePlayerTowardsCamera()
    {
        if (camT != null)
        {
            Vector3 cameraForward = camT.transform.forward;
            cameraForward.y = 0f; 
            if (cameraForward != Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(cameraForward);
                transform.rotation = newRotation;
            }

        }
        
    }


    private bool Grounded;
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



