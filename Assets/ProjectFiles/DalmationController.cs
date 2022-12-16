using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DalmationController : MonoBehaviour
{
    //Animator
    private Animator animator;
    //Character Controller
    public CharacterController controller;
    //LaserPointer End
    public Transform laserPointerEnd;
    //Sound
    private AudioSource soundOut;

    //"Bools" for tracking TODO at actionSteps
    private bool lookingAtTarget = false;
    private bool atTargetPosition = false;
    private bool checkPointActionDone = false;
    private bool canSeeLaser = true;
    private bool atLaserPointer = false;
    private bool lookingAtLaser = false;
    private bool laserInteraction = false;

    //Time & Trust
    private float animationDelay = 0f;
    private float trust = 0f; //As trust gets higher the dog will come closer to the laser pointer


    //After arriving at a checkpoint look at corresponding look target
    public Transform[] moveCheckpoints;
    public Transform[] lookCheckpoints;
    //Once both moveCheckpoint and lookCheckpoint is achieved wait delay then proceed to next step
    public float[] checkPointDelay;
    public string[] checkPointAction; //NONE, BITE, JUMPBITE, GROWL

    private int actionStep = 0; //index in checkpoint arrays
    private int checkPointActionStep = 0;

    private float currentDelay = 0;

    //Speeds
    public float turnSpeed = 5f;
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float xRotateSpeed = 10f;
    public float gravity = -9.81f;
    public bool running = false;

    Vector3 velocity;

    //Targets
    private Transform targetPosition;
    private Transform lookTarget;

    //Feet
    public GameObject frontFeet;
    public GameObject backFeet;

    //Sounds
    public AudioClip barking;
    public AudioClip step;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        soundOut = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();

        
    }
    private void OnEnable()
    {

        if (moveCheckpoints[actionStep] != null)
        {
            targetPosition = moveCheckpoints[actionStep];
            lookTarget = lookCheckpoints[actionStep];
        }
    }

    // Update is called once per frame
    void Update()
    {
        updateBools();
        

        if (canSeeLaser)
        {
            if (trust < 100) //trust = 0 - 100 based on seconds spent in sight of laser
            {
                trust += Time.deltaTime;
            }
            if (!lookingAtLaser)
            {
                turnToLaser();
            }
            if (!atLaserPointer && !laserInteraction)
            {
                animator.SetBool("movingForward", true);
                playWalking(true);
                moveToLaser();
            }
            else
            {
                animator.SetBool("movingForward", false);
                playWalking(false);
                //Playing animations
                if (animationDelay <= 0)
                {
                    laserInteraction = true;
                   // int upperRange = (int)((4 / 100) * trust); 
                    //if(upperRange < 1) { upperRange = 1; }//min 1
                    int animation = Random.Range(0, 4); //TODO affect by trust
                    switch (animation)
                    {
                        case 0:
                            animator.Play("Fight Idle");
                            animationDelay = 4f;
                            break;
                        case 1:
                            animator.Play("Barking");
                            soundOut.clip = barking;
                            soundOut.Play();
                            animationDelay = 2f;
                            break;
                        case 2:
                            animator.Play("Jump Low");
                            animationDelay = 2f;
                            break;
                        case 3:
                            animator.Play("Jump High");
                            animationDelay = 2f;
                            break;
                    }
                }
                else
                {
                    animationDelay -= Time.deltaTime;
                    if (animationDelay <= 0) { laserInteraction = false; }
                }
            }
        }
        else
        {
            if (trust < 100) //trust = 0 - 100 based on seconds spent in sight of laser
            {
                trust += (Time.deltaTime/4); //Gain trust at 1/4 rate if not using laser pointer
            }
            if (atTargetPosition)
            {
                animator.SetBool("movingForward", false);
                playWalking(false);
                if (lookingAtTarget)
                {
                    switch (checkPointActionStep)
                    {
                        case 0: //Perform animation
                            executeCheckPointAction();
                            checkPointActionStep++;
                            currentDelay = checkPointDelay[actionStep];
                            break;
                        case 1: //Delay - Must be longer than animation
                            if (currentDelay <= 0)
                            {
                                checkPointActionStep++;
                            }
                            else
                            {
                                currentDelay -= Time.deltaTime;
                            }
                            break;
                        case 2: //Increment actionStep and reset checkPointActionStep
                            actionStep++;
                            if (actionStep < moveCheckpoints.Length)
                            {
                                targetPosition = moveCheckpoints[actionStep];
                                lookTarget = lookCheckpoints[actionStep];
                            }
                            checkPointActionStep = 0;
                            resetBools();
                            break;
                    }
                }
                else
                {
                    turnToLookTarget();
                }

            }
            else
            {
                animator.SetBool("movingForward", true);
                playWalking(true);
                turnToMoveTarget();
                moveToTarget();
            }

        }
        applyGravity();
        // applyTerrainTilt(); TODO fix so rotation isn't weird on other angles
    }

    //"Bools" FUCTIONS
    private void updateBools()
    {
        if (targetPosition != null)
        {
            if (Vector3.Distance(targetPosition.position, transform.position) < 4)
            {
                atTargetPosition = true;
            }
            else
            {
                atTargetPosition = false;
            }
        }
        if (lookTarget != null)
        {
            Vector3 target = lookTarget.position;
            target.y = transform.position.y;
            Vector3 targetDirection = target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            float angle = Quaternion.Angle(targetRotation, transform.rotation);
            if (angle < 2)
            {
                lookingAtTarget = true;
            }
            else
            {
                lookingAtTarget = false;
            }
        }
        //CHECK IF LASER IN VISION RANGE
        if (Vector3.Distance(laserPointerEnd.position, transform.position) < 10) 
        {
            canSeeLaser = true;
        }
        else
        {
            canSeeLaser = false;
        }
        if (canSeeLaser && laserPointerEnd != null)
        {
            if (Vector3.Distance(laserPointerEnd.position, transform.position) < 4) //TODO affect by trust
            {
                atLaserPointer = true;
            }
            else
            {
                atLaserPointer = false;
            }
            Vector3 target = laserPointerEnd.position;
            target.y = transform.position.y;
            Vector3 targetDirection = target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            float angle = Quaternion.Angle(targetRotation, transform.rotation);
            if (angle < 2)
            {
                lookingAtLaser = true;
            }
            else
            {
                lookingAtLaser = false;
            }


        }
        
    }

    private void resetBools()
    {
        lookingAtTarget = false;
        atTargetPosition = false;
        atLaserPointer = false;
        checkPointActionDone = false;
    }


    //TURNING FUCTIONS
    private void turnToLookTarget()
    {
        if (lookTarget != null)
        {
            Vector3 targetDirection = lookTarget.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            if (transform.rotation != targetRotation)
            {
                Quaternion tempRot = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                //Only y rotation
                tempRot.x = 0;
                tempRot.z = 0;

                transform.rotation = tempRot;
            }
        }
    }

    private void turnToMoveTarget()
    {
        if (targetPosition != null)
        {
            Vector3 targetDirection = targetPosition.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            if (transform.rotation != targetRotation)
            {
                Quaternion tempRot = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                //Only y rotation
                tempRot.x = 0;
                tempRot.z = 0;

                transform.rotation = tempRot;
            }
        }
    }

    private void turnToLaser()
    {
        if (laserPointerEnd != null)
        {
            Vector3 targetDirection = laserPointerEnd.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            if (transform.rotation != targetRotation)
            {
                Quaternion tempRot = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                //Only y rotation
                tempRot.x = 0;
                tempRot.z = 0;

                transform.rotation = tempRot;
            }
        }
    }

    //MOVING FUNCTIONS
    private void moveToTarget()
    {
        if (targetPosition != null)
        {
            Vector3 move = Vector3.Normalize(targetPosition.position - transform.position);
            move.y = 0;

            controller.Move(move * walkSpeed * Time.deltaTime);
            transform.position += (move * walkSpeed * Time.deltaTime);
        }
    }
    private void moveToLaser()
    {
        if (laserPointerEnd != null)
        {
            Vector3 move = Vector3.Normalize(laserPointerEnd.position - transform.position);
            move.y = 0;

            controller.Move(move * walkSpeed * Time.deltaTime);
            transform.position += (move * walkSpeed * Time.deltaTime);
        }
    }

    //PYSICS FUNCTIONS
    private void applyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    private void applyTerrainTilt()
    {
        //alter angle of model based on terrain
        RaycastHit frontHit, backHit;
        float xRotation;

        if (Physics.Raycast(frontFeet.transform.position, -Vector3.up, out frontHit, 7f) && Physics.Raycast(backFeet.transform.position, -Vector3.up, out backHit, 7f))
        {
            float Opp = frontHit.distance - backHit.distance;
            float Adj = 1.2f;

            float theta = Mathf.Atan(Opp / Adj); //in radians
            float thetaDeg = Mathf.Rad2Deg * theta;

            xRotation = -thetaDeg;
        }
        else
        {//rotate towards normal rotation
            xRotation = 0;
        }

        Quaternion targetRotation = Quaternion.Euler(xRotation, 0, 0);
        Quaternion slerpRotation = Quaternion.Slerp(transform.rotation, targetRotation, xRotateSpeed * Time.deltaTime);
        transform.rotation = new Quaternion(slerpRotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
    }

    //ANIMATION FUNCTIONS
    private void executeCheckPointAction()
    {
        if (checkPointAction[actionStep].Equals("NONE"))
        {
            //SKIP
        }
        else if (checkPointAction[actionStep].Equals("BARK"))
        {
            animator.Play("Barking");
        }
        else if (checkPointAction[actionStep].Equals("LJUMP"))
        {
            animator.Play("Jump Low");
        }
        else if (checkPointAction[actionStep].Equals("HJUMP"))
        {
            animator.Play("Jump High");
        }
        else if (checkPointAction[actionStep].Equals("HJUMP"))
        {
            animator.Play("Jump High");
        }
    }


    //Audio FUNCTIONS
    private void playWalking(bool enabled)
    {
        if (enabled)
        {
            soundOut.Stop();
            soundOut.clip = step;
            soundOut.loop = true;
            soundOut.Play();
        }
        else
        {
            soundOut.loop = false;
            soundOut.Stop();
        }
    }

}
