using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class ScenarioTracker : MonoBehaviour
{

    private float worldTime = 0f;
    public float endPoint = 10f;
    public bool night;
    bool fadeInComplete = false;
    bool scenarioEnded = false;

    public Image image;

    public string path = "Timeline.txt";
    public string PN = "";

    // Start is called before the first frame update
    void Start()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        PN = "TODO";
        //GameObject PNH = GameObject.Find("PN Holder");

        //PN = PNH.GetComponent<MenuControl>().getPN();

        path = System.DateTime.Now.ToString("yyyy_MM_dd hh_mm ") + "PN" + PN + " TimeLine.txt";
        Debug.Log("PATH: " + path);
    }

    // Update is called once per frame
    void Update()
    {
        worldTime += Time.deltaTime;

        if (!fadeInComplete)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - 0.005f);
            if (image.color.a <= 0) { fadeInComplete = true; } //logMessage("Fade In Complete");
        }

        if (worldTime >= endPoint)
        {
            if (!night)
            {
                GameObject camera = GameObject.Find("[CameraRig]");
                FocusManager fm = camera.GetComponent<FocusManager>();
                fm.endScene();
            } else
            {
                GameObject camera = GameObject.Find("[CameraRig]");
                FocusManagerNight fm = camera.GetComponent<FocusManagerNight>();
                fm.endScene();
            }

            if (!night)
            {
                GameObject dog = GameObject.Find("Dalmatian");
                DalmationControllerV2 dc = dog.GetComponent<DalmationControllerV2>();
                dc.endScene();
            } else
            {
                GameObject dog = GameObject.Find("Monster Dog");
                CreatureControllerV2 dc = dog.GetComponent<CreatureControllerV2>();
                dc.endScene();
            }
            
            endScenario();
        }

        if (scenarioEnded)
        {
            //logMessage("Scenario Ended, Transition to End Scene");
            SceneManager.LoadScene(4);
        }

    }

    private void endScenario()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a + 0.005f);
        if (image.color.a >= 1) { scenarioEnded = true; }
    }


    public void logMessage(string message)
    {
        StreamWriter writer = new StreamWriter(path, true);
        //Fri Feb 21 hh:mm:ss yyyy
        String date = System.DateTime.Now.ToString("ddd MMM MM H:mm:ss yyyy");
        writer.WriteLine("[" + date + "] :: " + message);
        writer.Close();
    }
}