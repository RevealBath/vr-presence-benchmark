using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tobii.XR;
using UnityEngine;
using ViveSR.anipal.Eye;

public class PupilBase : MonoBehaviour
{
    public GameObject fog;
    public float worldTime;
    private bool fogArrived;
    public bool night;
    
    private EyeData eyeData;
    private DateTime now;
    private bool sceneEnded;
    private string baseFile;

    private string PN;
    private List<BlinkData> blinks;
    private List<PupilDilation> pupilDilations;

    // Start is called before the first frame update
    void Start()
    {
        eyeData = new EyeData();
        pupilDilations = new List<PupilDilation>();
        blinks = new List<BlinkData>();
        fogArrived = false;
        //PN = "TODO";

        GameObject PNH = GameObject.Find("PN Holder");
        string PN = PNH.GetComponent<MenuControl>().getPN();
        if (night)
        {
            baseFile = "results/PN " + PN + " NightBaseData.csv";
            fog.SetActive(false);
        } else
        {
            baseFile = "results/PN " + PN + " DayBaseData.csv";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (night)
        {
            worldTime += Time.deltaTime;
            if (worldTime > 15f && !fogArrived)
            {
                fog.SetActive(true);
                fogArrived = true;
            }  
        }
        if (!sceneEnded)
        {
            now = DateTime.Now;

            var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
            blinkManagment(eyeTrackingData);

            SRanipal_Eye_API.GetEyeData(ref eyeData);
            PupilDilation pup = new PupilDilation
            {
                id = pupilDilations.Count,
                timeStamp = now,
            };

            if (eyeData.verbose_data.left.pupil_diameter_mm > 0)
            {
                pup.leftDilation = eyeData.verbose_data.left.pupil_diameter_mm;
            }
            else
            {
                pup.leftDilation = -1; //to easily show the invalid data 
            }

            if (eyeData.verbose_data.right.pupil_diameter_mm > 0)
            {
                pup.rightDilation = eyeData.verbose_data.right.pupil_diameter_mm;
            }
            else
            {
                pup.rightDilation = -1; //to easily show the invalid data 
            }
            pupilDilations.Add(pup);
        }
    }

    private void blinkManagment(TobiiXR_EyeTrackingData data)
    {
        BlinkData temp = new BlinkData();
        if (data.IsLeftEyeBlinking)
        {
            temp.left = true;
        }
        else
        {
            temp.left = false;
        }
        if (data.IsRightEyeBlinking)
        {
            temp.right = true;
        }
        else
        {
            temp.right = false;
        }
        blinks.Add(temp);
    }

    public void endScene()
    {
        if (!sceneEnded)
        {
            writeOutput();
            
            sceneEnded = true;
        }
    }

    public void writeOutput()
    {
        StreamWriter writer = new StreamWriter(baseFile);

        writer.WriteLine("id,timestamp,left dilation,right dilation,left closed, right closed");

        for (int i = 0; i < pupilDilations.Count; i++)
        {
            writer.WriteLine(pupilDilations[i].id + "," + pupilDilations[i].timeStamp.ToString("MM/dd/yyyy hh:mm:ss.fff") + "," +
                pupilDilations[i].leftDilation + "," + pupilDilations[i].rightDilation + "," + blinks[i].left + "," + blinks[i].right);
        }
        writer.WriteLine("Final, " + pupilDilations.Count);
        writer.Flush();
        writer.Close();
    }

}
