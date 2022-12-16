using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    private Animator animator;
    public CharacterController controller;

    public float turnSpeed = 0.1f;
    private float playerSpeed = 12f;
    public float walkSpeed = 12f;
    public float runSpeed = 15f;
    public float gravity = -9.81f;
    Vector3 velocity;

    public Transform[] checkpoints = new Transform[10];
    public Transform playerCamera;

    private int targetCheckpoint;
    private Quaternion targetRoation;
    private int state = 0; // 0 = idle, 1 = moving

    //ActionQueue Variables
    private List<ScriptAction> actionQueue = new List<ScriptAction>();
    private int currentAction = 0;

    bool growlQueued = false;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (state == 0)
        {
            animator.SetBool("movingForward", false);
            lookAtPlayer();
        }
        else if (state == 1)
        {
            animator.SetBool("movingForward", true);
           
            lookAtCheckpoint();

            Vector3 move = transform.forward;

            controller.Move(move * playerSpeed * Time.deltaTime);
            transform.position += (move * playerSpeed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
            //transform.position += (velocity * Time.deltaTime);

            float xDistance, zDistance;
            xDistance = Mathf.Abs(checkpoints[targetCheckpoint].position.x - transform.position.x);
            zDistance = Mathf.Abs(checkpoints[targetCheckpoint].position.z - transform.position.z);

        
            if (xDistance < 2 && zDistance < 2)
            {
                //Go Idle
                state = 0;
                Debug.Log("Arrived at checkpoint " + targetCheckpoint);
            }
        }

        if (growlQueued)
        {
            GetComponent<AudioSource>().Play();
            Debug.Log("Growl");
            growlQueued = false;
        }

    }

    public void setMoveTarget(int checkpoint, bool running)
    {
        animator.SetBool("walk", !running);
        if (running)
        {
            playerSpeed = runSpeed;
        }
        else { playerSpeed = walkSpeed; }
    
        targetCheckpoint = checkpoint;
        state = 1;
    }
    public void lookAtCheckpoint()
    {
        if (transform.rotation != targetRoation)
        {
            Vector3 targetDirection = checkpoints[targetCheckpoint].position - transform.position;
            targetRoation = Quaternion.LookRotation(targetDirection);

            Quaternion tempRot = Quaternion.Lerp(transform.rotation, targetRoation, turnSpeed * Time.deltaTime);
            tempRot.x = 0;
            tempRot.z = 0;

            transform.rotation = tempRot;
        }
    }
    public void lookAtPlayer()
    {
        Vector3 targetDirection = playerCamera.position - transform.position;
        targetRoation = Quaternion.LookRotation(targetDirection);
        if (transform.rotation != targetRoation)
        {


            Quaternion tempRot = Quaternion.Lerp(transform.rotation, targetRoation, turnSpeed * Time.deltaTime);
            tempRot.x = 0;
            tempRot.z = 0;

            transform.rotation = tempRot;
        }
    }

    public void addCheckpointLook()
    {
        actionQueue.Add(new LookAction("Checkpoint", targetCheckpoint));
    }

    public void addCheckpointMove(int checkpoint, bool running)
    {
        actionQueue.Add(new MoveAction((int)checkpoints[checkpoint].position.x, (int)checkpoints[checkpoint].position.y, running));
    }

    public void enableMonster()
    {
        enabled = true;
    }

    public void growl()
    {
        growlQueued = true;
    }

}
