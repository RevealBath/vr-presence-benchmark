using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DalmationControllerV2 : MonoBehaviour
{
    //Animator
    private Animator animator;
    //Character Controller
    public CharacterController controller;
    //LaserPointer End
    public Transform laserPointerEnd;
    //Sound
    private SoundManager soundOut;

    public ScenarioTracker tracker;

    private List<DogActions> dogActions;

    private bool sceneEnded;

    //"Bools" for tracking TODO at actionSteps
    private bool lookingAtLookTarget = false;
    private bool lookingAtMoveTarget = false;
    private bool atTargetPosition = false;

    private bool checkPointActionDone = false;
    private bool atLaserPointer = false;
    private bool lookingAtLaser = false;
    private bool laserInteraction = false;
    private bool interestedInLaser = false;

    private bool turningRight = false;
    private bool turningRightLook = false;
    private bool turningRightLaser = false;

    private bool animationStarted = false;
    private bool animationFinished = false;
    
    //Checkpoints
    public DalmationCheckpoint[] checkPoints;

    private int actionStep = 0; //index in checkpoint arrays
    private int checkPointActionStep = 0;

    private float currentDelay = 0;
    private float interest = 0f;

    //Speeds
    public float tiltSpeed = 10f;
    public float gravity = -9.81f;
    public bool running = true;

    Vector3 velocity;

    //Targets
    private Transform targetPosition;
    private Transform lookTarget;

    private Vector3 laserSnapshot;

    //Feet
    public GameObject frontFeet;
    public GameObject backFeet;

    public GameObject cube;

    //file
    private string PN;
    private string dogFilePath;
    public int animationInUse;
    public bool walking;
    public bool turnLeft;
    public bool turnRight;
    public int soundtrack;
    public int count;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        soundOut = GetComponent<SoundManager>();
        controller = GetComponent<CharacterController>();
        dogActions = new List<DogActions>();

        GameObject PNH = GameObject.Find("PN Holder");
        string PN = PNH.GetComponent<MenuControl>().getPN();
        //PN = "TODO";

        dogFilePath = "results/PN " + PN + " dogInteractions.csv";

    }
    private void OnEnable()
    {

        if (checkPoints[actionStep].location != null)
        {
            targetPosition = checkPoints[actionStep].location;
            lookTarget = checkPoints[actionStep].look;
            running = checkPoints[actionStep].run;
        }
    }

    // Update is called once per frame
    void Update()
    {
        soundtrack = 0;
        animationInUse = 0;

        updateBools();
        if (currentDelay <= 0)
        {
            if (interestedInLaser)
            {
                if (canSeeLaserPointer())
                {
                    if (!atLaserPointer)
                    {
                        animator.SetBool("Walking", true);
                        walking = true;
                        if (!lookingAtLaser)
                        {
                            if (turningRightLaser)
                            {
                                animator.SetBool("TurnRight", true);
                                turnRight = true;
                                animator.SetBool("TurnLeft", false);
                                turnLeft = false;
                            }
                            else
                            {
                                animator.SetBool("TurnRight", false);
                                turnRight = false;
                                animator.SetBool("TurnLeft", true);
                                turnLeft = true;
                            }
                        }
                        else
                        {
                            animator.SetBool("TurnRight", false);
                            turnRight = false;
                            animator.SetBool("TurnLeft", false);
                            turnLeft = false;
                        }
                    }
                    else
                    {
                        animator.SetBool("Walking", false);
                        walking = false;
                        animator.SetBool("TurnRight", false);
                        turnRight = false;
                        animator.SetBool("TurnLeft", false);
                        turnLeft = false;
                    }

                    //Perform animation then reset interest bool (not interest float)


                    if (atLaserPointer)
                    {
                        animator.SetBool("Walking", false);
                        walking = false;
                        animator.SetBool("Running", false);
                        animationInUse = 0;
                        //Turn to look Target
                        if (!lookingAtLaser)
                        {
                            if (turningRightLaser)
                            {
                                animator.SetBool("TurnRight", true);
                                turnRight = true;
                                animator.SetBool("TurnLeft", false);
                                turnLeft = false;
                            }
                            else
                            {
                                animator.SetBool("TurnRight", false);
                                turnRight = false;
                                animator.SetBool("TurnLeft", true);
                                turnLeft = true;
                            }
                        }
                        else
                        {
                            animator.SetBool("TurnRight", false);
                            turnRight = false;
                            animator.SetBool("TurnLeft", false);
                            turnLeft = false;
                            if (!animationStarted)
                            {
                                executeLaserAction(); 
                                animationStarted = true;
                                currentDelay = 0.4f;
                            }

                            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                            {

                                if (currentDelay <= 0)
                                {
                                    animationFinished = true;
                                }
                            }

                            if (animationFinished)
                            {
                                animationFinished = false;
                                animationStarted = false;
                                randomLooseInterest();
                            }



                        }
                    }
                }
            }
            else
            {
                if (!atTargetPosition)
                {
                    if (running)
                    {
                        animator.SetBool("Walking", false);
                        walking = false;
                        animator.SetBool("Running", true);
                    }
                    else
                    {
                        animator.SetBool("Running", false);
                        animator.SetBool("Walking", true);
                        walking = true;
                    }

                    if (!lookingAtMoveTarget)
                    {
                        if (turningRight)
                        {
                            animator.SetBool("TurnRight", true);
                            turnRight = true;
                            animator.SetBool("TurnLeft", false);
                            turnLeft = false;
                        }
                        else
                        {
                            animator.SetBool("TurnRight", false);
                            turnRight = false;
                            animator.SetBool("TurnLeft", true);
                            turnLeft = true;
                        }
                    }
                    else
                    {
                        animator.SetBool("TurnRight", false);
                        turnRight = false;
                        animator.SetBool("TurnLeft", false);
                        turnLeft = false;
                    }
                }
                else
                {
                    animator.SetBool("Walking", false);
                    walking = false;
                    animator.SetBool("TurnRight", false);
                    turnRight = false;
                    animator.SetBool("TurnLeft", false);
                    turnLeft = false;
                    animator.SetBool("Running", false);
                    //Turn to look Target
                    if (!lookingAtLookTarget)
                    {
                        if (turningRightLook)
                        {
                            animator.SetBool("TurnRight", true);
                            turnRight = true;
                            animator.SetBool("TurnLeft", false);
                            turnLeft = false;
                        }
                        else
                        {
                            animator.SetBool("TurnRight", false);
                            turnRight = false;
                            animator.SetBool("TurnLeft", true);
                            turnLeft = true;
                        }
                    }
                    else
                    {
                        animator.SetBool("TurnRight", false);
                        turnRight = false;
                        animator.SetBool("TurnLeft", false);
                        turnLeft = false;
                        if (!animationStarted)
                        {
                            executeCheckPointAction();
                            animationStarted = true;
                            currentDelay = checkPoints[actionStep].delay;
                        }

                        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        {
                            if (currentDelay <= 0)
                            {
                                animationFinished = true;
                            }
                        }

                        if (animationFinished)
                        {
                            incrementCheckpoint();
                            animationFinished = false;
                            animationStarted = false;
                        }



                    }
                }
            }
        }
        else { currentDelay -= Time.deltaTime;}
        updateInterest();

        applyGravity();
        // applyTerrainTilt(); TODO fix so rotation isn't weird on other angles
    }

    //"Bools" FUCTIONS
    private void updateBools()
    {
        if (targetPosition != null)
        {
            
            Vector3 target = targetPosition.position;
            target.y = transform.position.y;
            //AT TARGET CHECK
            if (Vector3.Distance(target, transform.position) < 3)
            {
                atTargetPosition = true;
                Debug.Log("At target Position");
            }
            else
            {
                atTargetPosition = false;
            }
            //LOOKING AT TARGET

             
            Vector3 targetDirection = target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            Vector3 crossProduct = Vector3.Cross(transform.rotation * Vector3.forward, targetRotation * Vector3.forward);
            if (crossProduct.y >= 0)
            {
                turningRight = true;
            }
            else
            {
                turningRight = false;
            }


            float angle = Quaternion.Angle(targetRotation, transform.rotation);
            if (angle < 10)
            {
                lookingAtMoveTarget = true;
            }
            else
            {
                lookingAtMoveTarget = false;
            }


        }
        if (lookTarget != null)
        {
            Vector3 target = lookTarget.position;
            target.y = transform.position.y;
            Vector3 targetDirection = target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            Vector3 crossProduct = Vector3.Cross(transform.rotation * Vector3.forward, targetRotation * Vector3.forward);
            if (crossProduct.y >= 0)
            {
                turningRightLook = true;
            }
            else
            {
                turningRightLook = false;
            }
            

            float angle = Quaternion.Angle(targetRotation, transform.rotation);
            if (angle < 10)
            {
                lookingAtLookTarget = true;
            }
            else
            {
                lookingAtLookTarget = false;
            }
        }
        if (laserSnapshot != null)
        {
            Vector3 target = laserSnapshot;
            target.y = transform.position.y;
            Vector3 targetDirection = target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (Vector3.Distance(target, transform.position) < 4)
            {
                atLaserPointer = true;
            }
            else
            {
                atLaserPointer = false;
            }

            Vector3 crossProduct = Vector3.Cross(transform.rotation * Vector3.forward, targetRotation * Vector3.forward);
            if (crossProduct.y >= 0)
            {
                turningRightLaser = true;
            }
            else
            {
                turningRightLaser = false;
            }


            float angle = Quaternion.Angle(targetRotation, transform.rotation);
            if (angle < 10)
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
        lookingAtLookTarget = false;
        lookingAtLaser = false;
        atTargetPosition = false;
        atLaserPointer = false;
        checkPointActionDone = false;
    }


    //CHECKPOINT FUNCTIONS
    private void incrementCheckpoint()
    {
        actionStep += 1;
        if (actionStep > checkPoints.Length) { actionStep = 0; } //TODO End Scenario
        targetPosition = checkPoints[actionStep].location;
        lookTarget = checkPoints[actionStep].look;

        running = checkPoints[actionStep].run;

        checkPointActionStep = 0;
        resetBools();
    }

    //LASER POINTER FUNCTIONS
    private bool canSeeLaserPointer()
    {
        Vector3 temp = laserPointerEnd.position;
        temp.y = transform.position.y;

        Vector3 directionToPointer = temp - transform.position;

        float distanceToPointer = Vector3.Distance(transform.position, temp);
        float dotProduct = Vector3.Dot(directionToPointer, transform.forward);

        if (dotProduct > 0 && distanceToPointer < 10)
        {
            return true;
        }

        return false;
    }
    private void updateInterest()
    {
        if (canSeeLaserPointer())
        {
            interest += Time.deltaTime;
        }
        else
        {
            interest -= Time.deltaTime * 5; //interest in laser pointer decreases quickly if it can't be seen
        }
        if (interest > 1.5f)
        {
            if (!interestedInLaser)
            {
                //tracker.logMessage("Dog became interested in the laser");
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog became interested in the laser"
                });
                //animator.CrossFade("Fight Idle", 0.25f);
                //soundOut.Play("FightBark");
                laserSnapshot = laserPointerEnd.transform.position;
            }
            interestedInLaser = true;
        }
        else
        {
            if (interestedInLaser)
            {
                //tracker.logMessage("Dog lost interest in the laser");
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog lost interest in the laser"
                });
            }
            interestedInLaser = false;
        }
        if (interest > 10f) { interest = 10f; } //Needs cap to allow disinterest
        if (interest < 0f) { interest = 0f; }
    }
    private void randomLooseInterest()
    {
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < 20)
        {
            interest = 0;
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
        Quaternion slerpRotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
        transform.rotation = new Quaternion(slerpRotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
    }
    public void endScene()
    {
        if (!sceneEnded)
        {
            StreamWriter writer = new StreamWriter(dogFilePath);
            writer.WriteLine("id,timestamp,interaction");

            for (int i = 0; i < dogActions.Count; i++)
            {
                writer.WriteLine(dogActions[i].id + "," + dogActions[i].timeStamp.ToString("dd/MM/yyyy hh:mm:ss.fff") + "," +
                    dogActions[i].interaction);
            }
            writer.WriteLine("Final," + dogActions.Count);
            writer.Flush();
            writer.Close();
        }
    }
    //ANIMATION FUNCTIONS
    private void executeCheckPointAction()
    {
        if (checkPoints[actionStep].action.Equals("NONE"))
        {
            //SKIP
            //tracker.logMessage("Dog at checkpoint");
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                timeStamp = DateTime.Now,
                interaction = "Dog at checkpoint"
            });
        }
        else if (checkPoints[actionStep].action.Equals("BARK"))
        {
            //tracker.logMessage("Dog at checkpoint, performed bark animation");
            animator.CrossFade("Bark", 0.25f);
            animationInUse = 4;
            soundOut.Play("5Bark");
            soundtrack = 1;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                timeStamp = DateTime.Now,
                interaction = "Dog at checkpoint. performed bark animation"
            });
        }
        else if (checkPoints[actionStep].action.Equals("STRAFE"))
        {
            //tracker.logMessage("Dog at checkpoint, performed side step animation");
            animator.CrossFade("SideStep", 0.25f);
            animationInUse = 5;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                timeStamp = DateTime.Now,
                interaction = "Dog at checkpoint. performed side step animation"
            });
        }
        else if (checkPoints[actionStep].action.Equals("DIG"))
        {
            //tracker.logMessage("Dog at checkpoint, performed dig animation");
            animator.CrossFade("Dig", 0.25f);
            animationInUse = 6;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                timeStamp = DateTime.Now,
                interaction = "Dog at checkpoint. performed dig animation"
            });
        }
        else if (checkPoints[actionStep].action.Equals("FIGHT"))
        {
            //tracker.logMessage("Dog at checkpoint, performed fight animation");
            animator.CrossFade("Fight Idle", 0.25f);
            animationInUse = 7;
            soundOut.Play("FightBark");
            soundtrack = 2;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                timeStamp = DateTime.Now,
                interaction = "Dog at checkpoint. performed fight animation and barked"
            });
        }
        else if (checkPoints[actionStep].action.Equals("JUMP"))
        {
            //tracker.logMessage("Dog at checkpoint, performed jump attack animation");
            animator.CrossFade("Jump Attack", 0.25f);
            animationInUse = 8;
            soundOut.Play("Bark");
            soundtrack = 3;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                timeStamp = DateTime.Now,
                interaction = "Dog at checkpoint. performed jump attack animation and barked"
            });
        }
        else if (checkPoints[actionStep].action.Equals("SCRATCH"))
        {
            //tracker.logMessage("Dog at checkpoint, performed scratch animation");
            animator.CrossFade("Scratch", 0.25f);
            animationInUse = 9;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                timeStamp = DateTime.Now,
                interaction = "Dog at checkpoint. performed scratch animation"
            });
        }
    }

    private void executeLaserAction()
    {
        int animation = UnityEngine.Random.Range(0, 7); //TODO affect by trust
        switch (animation)
        {
            case 0:
                //tracker.logMessage("Dog interacted with laser, performed fight animation");
                animator.CrossFade("Fight Idle", 0.25f);
                animationInUse = 7;
                soundOut.Play("FightBark");
                soundtrack = 4;
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog interacted with laser. performed fight animation and barked"
                });
                break;
            case 1:
                //tracker.logMessage("Dog interacted with laser, performed jump attack animation");
                animator.CrossFade("Jump Attack", 0.25f);
                animationInUse = 8;
                soundOut.Play("Bark");
                soundtrack = 3;
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog interacted with laser. performed jump attack animation and barked"
                });
                break;
            case 2:
                //tracker.logMessage("Dog interacted with laser, performed side step animation");
                animator.CrossFade("SideStep", 0.25f);
                animationInUse = 5;
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog interacted with laser. performed side step animation"
                });
                break;
            case 3:
                //tracker.logMessage("Dog interacted with laser, performed bark animation");
                animator.CrossFade("Bark", 0.25f);
                animationInUse = 4;
                soundOut.Play("5Bark");
                soundtrack = 1;
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog interacted with laser. performed bark animation"
                });
                break;
            case 4:
                //tracker.logMessage("Dog interacted with laser, performed side step jump animation");
                animator.CrossFade("SideStepJump", 0.25f);
                animationInUse = 10;
                soundOut.Play("Bark");
                soundtrack = 3;
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog interacted with laser. performed side step jump animation and barked"
                });
                break;
            case 5:
                //tracker.logMessage("Dog interacted with laser, performed jump attack animation");
                animator.CrossFade("Jump Attack", 0.25f);
                animationInUse = 8;
                soundOut.Play("Bark");
                soundtrack = 3;
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog interacted with laser. performed jump attack animation and barked"
                });
                break;
            case 6:
                //tracker.logMessage("Dog interacted with laser. performed jump attack animation");
                animator.CrossFade("Jump Attack", 0.25f);
                animationInUse = 8;
                soundOut.Play("Bark");
                soundtrack = 3;
                dogActions.Add(new DogActions
                {
                    id = dogActions.Count,
                    timeStamp = DateTime.Now,
                    interaction = "Dog interacted with laser. performed jump attack animation"
                });

                break;
        }
    }

}

public class DogActions
{
    public int id;
    public DateTime timeStamp;
    public string interaction;
}