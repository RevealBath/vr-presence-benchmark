using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightsOff : MonoBehaviour
{

    public GameObject lamp;

    private float time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        lamp.GetComponentInChildren<Light>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > 1.5f)
        {
            time = 0f;

            lamp.GetComponentInChildren<Light>().enabled = !lamp.GetComponentInChildren<Light>().enabled;
        }
    }
}
