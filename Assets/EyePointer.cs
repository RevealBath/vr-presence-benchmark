using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;

public class EyePointer : MonoBehaviour
{
    public float defaultLength = 80f;
    public GameObject eyeEndPoint;
 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        eyeEndPoint.transform.position = calculateEnd();

        GameObject dog = GameObject.Find("Dalmatian");
        if (dog != null)
        {
            
            Vector3 dogPos = dog.transform.position;
            var distance = Vector3.Distance(eyeEndPoint.transform.position, dogPos);
            GameObject eye = GameObject.Find("eye gaze pointer");
            if(eye != null)
            {

                ColourManagment cm = eye.GetComponent<ColourManagment>();
                if (distance < 5)
                {
                    cm.changeColour(Color.blue);
                }
                else if (distance < 10)
                {
                    cm.changeColour(Color.green);

                }
                else
                {
                    cm.changeColour(Color.white);
                }
            }
        }
    }

    private Vector3 calculateEnd()
    {
        RaycastHit hit = createForwardRaycast();
        Vector3 endPosition = defaultEnd(defaultLength);

        if (hit.collider)
        {
            endPosition = hit.point;
            eyeEndPoint.SetActive(true);
        }
        else
        {
            eyeEndPoint.SetActive(false);
        }
        return endPosition;
    }


    private RaycastHit createForwardRaycast()
    {
        var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
        RaycastHit rHit;
        Ray ray = new Ray(eyeTrackingData.GazeRay.Origin, eyeTrackingData.GazeRay.Direction.normalized);
        Physics.Raycast(ray, out rHit, defaultLength);
        return rHit;
    }

    private Vector3 defaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }
}
