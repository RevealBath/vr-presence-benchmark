using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;

    public GameObject frontFeet;
    public GameObject backFeet;

    public ScriptAction[] actions;
    private int currentAction = 0;

    bool movingForward = true;
    bool idle = false;

    public float creatureSpeed = 12f;
    public float gravity = -9.81f;
    public float rotateSpeed = 10f;
   
    Vector3 velocity;
    Quaternion targetRotation;

    float rotX, rotY, rotZ;
    Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        rotX = transform.rotation.eulerAngles.x;
        rotY = transform.rotation.eulerAngles.y;
        rotZ = transform.rotation.eulerAngles.z;

        setMoveTarget();
    }

    // Update is called once per frame
    void Update()
    {
        if (!idle)
        {
            if (movingForward)
            {
                animator.SetBool("movingForward", true);

                Vector3 move = transform.forward;

                controller.Move(move * creatureSpeed * Time.deltaTime);
            }
            else { animator.SetBool("movingForward", false); }

            //Gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            //alter angle of model based on terrain
            RaycastHit frontHit, backHit;

            if (Physics.Raycast(frontFeet.transform.position, -Vector3.up, out frontHit, 7f) && Physics.Raycast(backFeet.transform.position, -Vector3.up, out backHit, 7f))
            {
                float Opp = frontHit.distance - backHit.distance;
                float Adj = 1.2f;

                float theta = Mathf.Atan(Opp / Adj); //in radians
                float thetaDeg = Mathf.Rad2Deg * theta;

                rotX = -thetaDeg;
            }
            else
            {//rotate towards normal rotation
                rotX = 0;
            }

           
            if (actions[currentAction].completed)
            {
                currentAction++;
                if (currentAction == actions.Length)
                {
                    idle = true;
                }
                else
                {
                    //target checkpoint
                    setMoveTarget();
                }
            }
            else
            {
                

                targetRotation = Quaternion.Euler(rotX, rotY, rotZ);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

                if (actions[currentAction].actionName.Equals("Checkpoint"))
                {
                    moveTowardsCheckpoint();

                    if (transform.rotation != targetRotation)
                    {
                        lookAtCheckpoint();
                    }
                }
            }
        }

    }
    public void lookAtCheckpoint()
    {
        Vector3 targetDirection = targetPosition - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);

        Debug.Log(Mathf.Abs(transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y));

        if (Mathf.Abs(transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y) > 1f)
        {
            animator.SetBool("turning", true);
            if ((lookRotation.y - transform.rotation.y) > 0)
            {
                animator.SetBool("turningRight", true);
            }
            else { animator.SetBool("turningRight", false); }

            rotY = lookRotation.eulerAngles.y;
        }
        else
        {
            animator.SetBool("turning", false);
            Debug.Log("turning false");
        }
    }
    public void moveTowardsCheckpoint()
    {
        movingForward = true;

        //check Distance

        float xDistance, zDistance;
        xDistance = Mathf.Abs(actions[currentAction].parameters[0] - transform.position.x);
        zDistance = Mathf.Abs(actions[currentAction].parameters[1] - transform.position.z);


        if (xDistance < 2 && zDistance < 2)
        {
            actions[currentAction].completed = true;
            movingForward = false;
        }
    }
    public void setMoveTarget()
    {
        targetPosition = new Vector3(actions[currentAction].parameters[0], 0, actions[currentAction].parameters[1]);



    }

}