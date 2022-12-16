using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamController2 : MonoBehaviour
{
    public Transform waterLevel;
    public Transform fish;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (waterLevel != null && fish != null)
        {
            Vector3 position = waterLevel.position;
            position.x = fish.position.x;
            position.z = fish.position.z;

            gameObject.transform.position = position;
        }
    }
}
