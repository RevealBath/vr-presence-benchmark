using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogControl : MonoBehaviour
{
    bool started = true;
    bool atTargetLocation = false;

    public float moveSpeed = 0.05f;
    public Vector3 targetLocation;

    // Update is called once per frame
    void Update()
    {
        if (started && !atTargetLocation)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetLocation, moveSpeed);
        }

    }
}
