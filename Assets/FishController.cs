using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishController : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;
    private SoundManager soundOut;

    public Transform laserPointerEnd;
    public ScenarioTracker tracker;

    public FishCheckpoint[] fishCheckpoints;

    public GameObject particleEffect;

    private int checkpointIndex = 0;

    bool atTargetPosition = false;
    bool lookingAtTarget = false;

    int laserActive = 0;
    Vector3 laserPoint;
    bool laserActivated = false;

    private Transform targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        soundOut = GetComponent<SoundManager>();
        controller = GetComponent<CharacterController>();
        targetPosition = fishCheckpoints[checkpointIndex].location;
    }

    // Update is called once per frame
    void Update()
    {
        updateBools();
        if (laserActive > 0)
        {
            laserActive -= 1;
            targetPosition.position = laserPoint;

        }
        else
        {
            if (laserActivated)
            {
                tracker.logMessage("Stopped Following Laser");
                targetPosition = fishCheckpoints[checkpointIndex].location;
                tracker.logMessage(fishCheckpoints[checkpointIndex].message);
                laserActivated = false;
            }
        }

        if (!lookingAtTarget)
        {
            //Turn to target
            turnToTarget();
        }
        else
        {
            //Move to target
            if (!atTargetPosition)
            {
                moveToTarget();
            }
            else
            {
                //increment move
                checkpointIndex++;
                if (checkpointIndex >= fishCheckpoints.Length)
                {
                    checkpointIndex = 0;
                }
                targetPosition = fishCheckpoints[checkpointIndex].location;
                tracker.logMessage(fishCheckpoints[checkpointIndex].message);
                resetBools();
            }
        }
        updateParticleEffect();
    }

    //Movement
    private void moveToTarget()
    {
        if (targetPosition != null)
        {
            Vector3 move = Vector3.Normalize(targetPosition.position - transform.position);
            move.y = 0;

            float speed = 3f;

            transform.position += transform.forward * Time.deltaTime * speed;
        }
    }

    private void turnToTarget()
    {
        float speed = 1f;
        if (animator.GetBool("Turn") == false)
        {
            animator.SetBool("Turn", true);
            soundOut.Play("FishTurn");
        }
        transform.Rotate(new Vector3(0, 1, 0), 2f);
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    //BOOLS
    private void resetBools()
    {
        atTargetPosition = false;
        lookingAtTarget = false;
    }

    private void updateBools()
    {
        if (targetPosition != null)
        {
            Vector3 targetLoc = targetPosition.position;
            targetLoc.y = transform.position.y;
            if (Vector3.Distance(targetLoc, transform.position) < 1)
            {
                atTargetPosition = true;
            }
            else
            {
                atTargetPosition = false;
            }
        }
        if (targetPosition != null)
        {
            Vector3 target = targetPosition.position;
            target.y = transform.position.y;
            Vector3 targetDirection = target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            float angle = Quaternion.Angle(targetRotation, transform.rotation);
            if (angle < 2)
            {
                lookingAtTarget = true;
                animator.SetBool("Turn",false);
            }
            else
            {
                lookingAtTarget = false;
            }
        }
    }

    public void updateLaserPointerDot(Vector3 dot)
    {
        Vector3 newDot = dot;
        Vector3 oldDot = laserPoint;
        newDot.y = transform.position.y;
        oldDot.y = transform.position.y;
        if (Vector3.Distance(newDot, oldDot) > 1 && Vector3.Distance(newDot, transform.position) > 1)
        {
            tracker.logMessage("Moving to New Laser Pointer Location");
            laserPoint = dot;
            laserActivated = true;
        }
        laserActive = 10;
    }

    private void updateParticleEffect()
    {
        if (transform.position.y > 59.7f)
        {//Slowly lower back into water
            float speed = 1f;

            transform.position += -transform.up * Time.deltaTime * speed;
        }

        if (transform.position.y >= 59.6f)
        {
            if (!particleEffect.activeInHierarchy)
            {
                soundOut.Play("WaterBreak");
            }
            particleEffect.SetActive(true);
        }
        else
        {
            particleEffect.SetActive(false);
        }
    }


}
