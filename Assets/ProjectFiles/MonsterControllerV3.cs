using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterControllerV3 : MonoBehaviour
{
    //Animator
    private Animator animator;
    //Character Controller
    private CharacterController controller;

    //"Bools" for tracking TODO at actionSteps
    private bool lookingAtTarget = false;
    private bool atTargetPosition = false;
    private bool checkPointActionDone = false;
    private bool lookingAtEscape = false;
    private bool atTargetEscape = false;
    private bool escaping = false;

    //After arriving at a checkpoint look at corresponding look target
    public Transform[] moveCheckpoints;
    public Transform[] lookCheckpoints;
    public Transform[] escapeCheckpoints; //Move to if warded off
    //Once both moveCheckpoint and lookCheckpoint is achieved wait delay then proceed to next step
    public float[] checkPointDelay;
    public string[] checkPointAction; //NONE, BITE, JUMPBITE, GROWL

    private int actionStep = 0; //index in checkpoint arrays
    private int checkPointActionStep = 0;
    private int escapeActionStep = 0;

    private float currentDelay = 0;

    //Speeds
    public float turnSpeed = 5f;
    public float walkSpeed = 5f;
    public float runSpeed = 5f;
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

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        //UPDATE BOOLS
        if (!escaping)
        {
            if (!lookingAtTarget)
            {

            }
            else
            {

            }

            if (!atTargetPosition)
            {

            }
            else
            {

            }

            if (atTargetPosition && lookingAtTarget)
            {

            }

        }
        else
        {

        }




    }

    //"Bools" Functions
    private void updateBools()
    {
        //Check if close to targetPosition
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

        //Check if looking at target
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

        //Check if close to escape point

        //Check if looking at escape point


    }







}
