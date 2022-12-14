using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    public float defaultLength = 80f;
    public GameObject endPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        endPoint.transform.position = calculateEnd();
    }

    private Vector3 calculateEnd()
    {
        RaycastHit hit = createForwardRaycast();
        Vector3 endPosition = defaultEnd(defaultLength);

        if (hit.collider)
        {
            endPosition = hit.point;
            endPoint.SetActive(true);
        }
        else
        {
            endPoint.SetActive(false);
        }
        return endPosition;
    }


    private RaycastHit createForwardRaycast()
    {
        RaycastHit rHit;
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out rHit, defaultLength);
        return rHit;

    }

    private Vector3 defaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }
}
