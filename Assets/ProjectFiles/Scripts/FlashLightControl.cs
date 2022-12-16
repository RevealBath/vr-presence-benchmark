using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class FlashLightControl : MonoBehaviour
{

    public SteamVR_Action_Boolean flashLightOn;
    public SteamVR_Input_Sources handType;
    public Light[] lights;
    private SoundManager soundOut;

    public float cooldown = 3f;
    private float currentCD;
    private bool onCD = false;

    public CapsuleCollider lightCollider;

    // Start is called before the first frame update
    void Start()
    {
        lights = GetComponentsInChildren<Light>();
        soundOut = GetComponent<SoundManager>();
        flashLightOn.AddOnStateUpListener(toggleLight, handType);
       
    }

    // Update is called once per frame
    void Update()
    {

        if (onCD)
        {
            currentCD -= Time.deltaTime;
            if(currentCD <= 0) { onCD = false; lightOn();}
        }

    }

    public void startCooldown()
    {
        Debug.Log("Light CD Start");
        lightOff();
        currentCD = cooldown;
        onCD = true;
    }


    public void toggleLight(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("Toggle");
        if (!onCD)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = !lights[i].enabled;
            }
            soundOut.Play("toggleLight");
        }
    }

    public void lightOn()
    {
        for (int i = 0; i < lights.Length; i++)
        {   
            lights[i].enabled = true;
            soundOut.Play("lightOn");
        }
    }
    public void lightOff()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].enabled = false;
            soundOut.Play("lightOff");
        }
    }

}
