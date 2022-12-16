using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampControl : MonoBehaviour
{

    Light lampLight;
    AudioSource lampAudio;
    bool lightOff = false;
    bool turnedOff = false;

    // Start is called before the first frame update
    void Start()
    {
        lampLight = GetComponentInChildren<Light>();
        lampAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lightOff && !turnedOff)
        {
            lampAudio.Play();
            lampLight.enabled = false;
            turnedOff = true;
            Debug.Log("["+ Time.time +"] Light Off :" + this.name);
        }

    }

    public void shutOffLight()
    {
        lightOff = true;
    }



}
