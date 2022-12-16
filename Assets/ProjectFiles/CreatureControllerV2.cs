using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CreatureControllerV2 : MonoBehaviour
{
    //Animator
    private Animator animator;
    //Character Controller
    public CharacterController controller;
    //
    SoundManager soundOut;
    public GameObject flashLight;
    public ScenarioTracker tracker;

    public int animationInUse;
    public bool walking;
    public int soundtrack;
    public int count;

    private List<DogActions> dogActions;

    private bool sceneEnded;

    //"Bools" for tracking TODO at actionSteps
    private bool lookingAtTarget = false;
    private bool atTargetPosition = false;
    private bool checkPointActionDone = false;
    private bool lookingAtEscape = false;
    private bool atTargetEscape = false;
    private bool escaping = false;

    //After arriving at a checkpoint look at corresponding look target

    public Checkpoint[] checkpoints;

    public int actionStep = 0; //index in checkpoint arrays
    private int checkPointActionStep = 0;
    private int escapeActionStep = 0;

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
    private Transform targetEscape;
    private Transform lookTarget;

    //Feet
    public GameObject frontFeet;
    public GameObject backFeet;

    private string PN;
    private string dogFilePath;

    public DateTime now;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        soundOut = GetComponent<SoundManager>();

        dogActions = new List<DogActions>();
        GameObject PNH = GameObject.Find("PN Holder");
        string PN = PNH.GetComponent<MenuControl>().getPN();
        //PN = "TODO";
        dogFilePath = "results/PN " + PN + " wolfInteractions.csv";

    }
    private void OnEnable()
    {

        if (checkpoints[actionStep].location != null)
        {
            targetPosition = checkpoints[actionStep].location;
            targetEscape = checkpoints[actionStep].escape;
            lookTarget = checkpoints[actionStep].look;
        }
    }

    // Update is called once per frame
    void Update()
    {
        now = DateTime.Now;
        soundtrack = 0;
        animationInUse = 0;
        updateBools();
        if (!escaping)
        {
            if (checkpoints[actionStep].run)
            {
                animator.SetBool("walk", false);
                walking = false;
            }
            else
            {
                animator.SetBool("walk", true);
                walking = true;
            }
            if (atTargetPosition)
            {
                animator.SetBool("movingForward", false);
                animationInUse = 0;
                if (lookingAtTarget)
                {
                    switch (checkPointActionStep)
                    {
                        case 0: //Perform animation
                            executeCheckPointAction();
                            playCheckPointSound();
                            checkPointActionStep++;
                            currentDelay = checkpoints[actionStep].delay;
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
                            if (actionStep < checkpoints.Length)
                            {
                                targetPosition = checkpoints[actionStep].location;
                                targetEscape = checkpoints[actionStep].escape;
                                lookTarget = checkpoints[actionStep].look;
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
                animationInUse = 1;
                turnToMoveTarget();
                moveToTarget(checkpoints[actionStep].run);
            }
        }
        else
        {
            animator.SetBool("walk", false);
            walking = false;
            switch (escapeActionStep)
            {
                case 0://Flinch
                    monsterFlinch();
                    currentDelay = 1f;
                    escapeActionStep++;
                    break;
                case 1://Pause for animation
                    if (currentDelay <= 0)
                    {
                        escapeActionStep++;
                    }
                    else
                    {
                        currentDelay -= Time.deltaTime;
                    }
                    break;
                case 2://Move
                    if (!atTargetEscape)
                    {
                        animator.SetBool("movingForward", true);
                        animationInUse = 1;
                        turnToEscapeTarget();
                        moveToEscape();
                    }
                    else
                    {
                        //tracker.logMessage("Monster arrived at Escape Checkpoint");
                        animator.SetBool("movingForward", false);
                        animationInUse = 0;
                        actionStep++;
                        if (actionStep < checkpoints.Length)
                        {
                            targetPosition = checkpoints[actionStep].location;
                            targetEscape = checkpoints[actionStep].escape;
                            lookTarget = checkpoints[actionStep].look;
                        }
                        checkPointActionStep = 0;
                        resetBools();
                        escaping = false;
                        escapeActionStep = 0;
                    }
                    break;
            }
        }
       
        applyGravity();
        // applyTerrainTilt(); TODO fix so rotation isn't weird on other angles
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

    //"Bools" FUCTIONS
    private void updateBools()
    {
        if (targetPosition != null)
        {
            Vector3 targetLoc = targetPosition.position;
            targetLoc.y = transform.position.y;
            if (Vector3.Distance(targetLoc, transform.position) < 4)
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
        if (targetEscape != null)
        {
            Vector3 targetEsc = targetEscape.position;
            targetEsc.y = transform.position.y;
            if (Vector3.Distance(targetEsc, transform.position) < 4)
            {
                atTargetEscape = true;
            }
            else
            {
                atTargetEscape = false;
            }
        }
        

    }

    private void resetBools()
    {
        lookingAtTarget = false;
        atTargetPosition = false;
        lookingAtEscape = false;
        atTargetEscape = false;
        checkPointActionDone = false;
        escaping = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider.gameObject.name);
        if (collider.gameObject.name.Equals("Flashlight_GameObject"))
        {
            escaping = true;
            flashLight.GetComponent<FlashLightControl>().startCooldown();
        }
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

    private void turnToEscapeTarget()
    {
        if (targetEscape != null)
        {
            Vector3 targetDirection = targetEscape.position - transform.position;
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
    private void moveToTarget(bool running)
    {
        if (targetPosition != null)
        {
            Vector3 move = Vector3.Normalize(targetPosition.position - transform.position);
            move.y = 0;

            float speed = walkSpeed;
            if (running) { speed = runSpeed; }

            controller.Move(move * speed * Time.deltaTime);
            transform.position += (move * speed * Time.deltaTime);
        }
    }
    private void moveToEscape()
    {
        if (targetEscape != null)
        {
            Vector3 move = Vector3.Normalize(targetEscape.position - transform.position);
            move.y = 0;

            controller.Move(move * runSpeed * Time.deltaTime);
            transform.position += (move * runSpeed * Time.deltaTime);
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
        if (checkpoints[actionStep].action.Equals("NONE"))
        {
            //tracker.logMessage("Monster at no action Checkpoint");
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "monster at no action checkpoint",
                timeStamp = now
            });
        }
        else if (checkpoints[actionStep].action.Equals("BITE"))
        {
            animationInUse = 2;
            soundtrack = 1;
            ////tracker.logMessage("Monster Performed Bite");
            animator.CrossFade("biteNormal", 0.25f);
            soundOut.Play("Bite");
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "monster performed bite at checkpoint with bite soundtrack",
                timeStamp = now
            });
        }
        else if (checkpoints[actionStep].action.Equals("JUMPBITE"))
        {
            animationInUse = 3;
            soundtrack = 1;
            //tracker.logMessage("Monster Performed Jump Bite");
            animator.CrossFade("jumpBiteNormal", 0.25f);
            soundOut.Play("Bite");
        }
        else if (checkpoints[actionStep].action.Equals("GROWL"))
        {
            animationInUse = 4;
            soundtrack = 2;
            //tracker.logMessage("Monster Performed Growl");
            animator.CrossFade("idleAggressive", 0.25f);
            soundOut.Play("Growl");
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "monster performed idle aggresive at checkpoint and growl soundtrack",
                timeStamp = now
            });
        }
        playCheckPointSound();
    }

    private void monsterFlinch()
    {
        //tracker.logMessage("Monster hit by torch");
        animator.CrossFade("getHitAggressive", 0.25f);
        animationInUse = 5;
        soundOut.Play("Hurt");
        soundtrack = 5;
        dogActions.Add(new DogActions
        {
            id = dogActions.Count,
            interaction = "monster performed hurt from being hit with flashlight and the hurt soundtrack",
            timeStamp = now
        });
    }

    //Audio
    private void playCheckPointSound()
    {
        SoundManager checkPointSound;
        //if (actionStep == 1)
        //{
        //    checkPointSound = checkpoints[8].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("Leaves");
        //    //tracker.logMessage("Leave Rustle sound played at Checkpoint 8");
        //}
        //if (actionStep == 2)
        //{
        //    checkPointSound = checkpoints[13].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("Rockfall");
        //    //tracker.logMessage("Rockfall sound played at Checkpoint 13");
        //}
        if (actionStep == 3)
        {
            soundtrack = 3;
            soundOut.Play("Breath");
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "monster played breath soundtrack",
                timeStamp = now
            });
            //tracker.logMessage("Monster made Breathing Sound");
        }
        if (actionStep == 4)
        {
            //tracker.logMessage("Monster made Snarl Sound");
            soundtrack = 4;
            soundOut.Play("Snarl");
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "monster playyed snarl soundtrack",
                timeStamp = now
            });
        }
        //if (actionStep == 8)
        //{
        //    //tracker.logMessage("Branch Snapping sound played at Checkpoint 2");
        //    checkPointSound = checkpoints[2].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("BranchSnap");
        //}
        //if (actionStep == 10)
        //{
        //    //tracker.logMessage("Monster Breath sound played at Checkpoint 2");
        //    checkPointSound = checkpoints[2].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("MonsterBreath");
        //}
        //if (actionStep == 12)
        //{
        //    //tracker.logMessage("Branch Snapping sound played at Checkpoint 2");
        //    checkPointSound = checkpoints[2].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("BranchSnap");
        //}
        if (actionStep == 13)
        {
            checkPointSound = checkpoints[13].location.GetComponentInParent<SoundManager>();
            checkPointSound.Play("Rockfall");
            soundtrack = 5;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "rockfall soundtrack was playyed below the monster",
                timeStamp = now
            });
            //tracker.logMessage("Rockfall sound played at Checkpoint 13");

        }
        //if (actionStep == 16)
        //{
        //    //tracker.logMessage("Branch Snapping sound played at Checkpoint 2");
        //    checkPointSound = checkpoints[2].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("BranchSnap");
        //}
        if (actionStep == 17)
        {
            //tracker.logMessage("Monster made Growl Sound");
            soundOut.Play("Growl");
            soundtrack = 2;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "monster playyed growl soundtrack",
                timeStamp = now
            });
        }
        //if (actionStep == 21)
        //{
        //    //tracker.logMessage("Branch Snapping sound played at Checkpoint 2");
        //    checkPointSound = checkpoints[2].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("BranchSnap2");
        //}
        //if (actionStep == 22)
        //{
        //    //tracker.logMessage("Monster Breath sound played at Checkpoint 8");
        //    checkPointSound = checkpoints[8].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("MonsterBreath");
        //}
        if (actionStep == 23)
        {
            //tracker.logMessage("Monster made Growl Sound");
            soundOut.Play("Snarl");
            soundtrack = 4;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "monster playyed snarl soundtrack",
                timeStamp = now
            });
        }
        //if (actionStep == 25)
        //{
        //    checkPointSound = checkpoints[8].location.GetComponentInParent<SoundManager>();
        //    checkPointSound.Play("MonsterBreath");
        //    //tracker.logMessage("Monster made Breathing Sound");
        //}
        if (actionStep == 27)
        {
            //tracker.logMessage("Monster made Growl Sound");
            soundOut.Play("Snarl");
            soundtrack = 4;
            dogActions.Add(new DogActions
            {
                id = dogActions.Count,
                interaction = "monster playyed snarl soundtrack",
                timeStamp = now
            });
        }



    }

}
