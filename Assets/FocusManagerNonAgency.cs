using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using System.IO;
using System;
using System.Linq;
using ViveSR.anipal.Eye;

public class FocusManagerNonAgency : MonoBehaviour
{
    public bool sceneEnded;

    //tobii variables
    //public DateTime LastMessageLogTobii;
    //public string pathTobii;
    //public string currentFocusTobii;
    //public List<EnvObject> lookStatsTobii;
    //public int totalChangesTobii = 0;
    //public float totalDistanceTobii = 0;
    //public int distanceAddedTobii = 0;

    private DateTime LastMessageLog;

    private DateTime LastFiveDogLog;
    private DateTime LastTenDogLog;
    private DateTime LastOneLaserLog;
    private DateTime LastFiveLaserLog;

    private TimeSpan timeWithinTenDog;
    private TimeSpan timeWithinFiveDog;
    private TimeSpan timeWithinOneLaser;
    private TimeSpan timeWithinFiveLaser;

    private bool withinFiveDog;
    private bool withinTenDog;
    private bool withinFiveLaser;
    private bool withinOneLaser;

    private string path;
    private string currentFocus;
    private List<EnvObject> lookStats;
    private int totalChanges = 0;

    private float leftEyeDiameter;
    private float rightEyeDiameter;
    private int leftEyeAdded;
    private int rightEyeAdded;

    private float totalDistance = 0;
    private float totalDistanceDog = 0;
    private float totalDistanceLaser = 0;

    private int distanceAdded = 0;
    private int distanceAddedDog = 0;
    private int distanceAddedLaser = 0;


    private float leftPupilBase;
    private float rightPupilBase;

    private EyeData eyeData;
    private float defaultLength = 200f;


    // Start is called before the first frame update
    void Start()
    {
        //string PN = "TODO";
        GameObject PNH = GameObject.Find("PN Holder");
        string PN = PNH.GetComponent<MenuControl>().getPN();

        //string PN = "ToDo";
        path = DateTime.Now.ToString("yyyy_MM_dd hh_mm ") + "PN" + PN + " EyeGaze.txt";
        LastFiveDogLog = DateTime.Now;
        LastTenDogLog = DateTime.Now;
        LastOneLaserLog = DateTime.Now;
        LastFiveLaserLog = DateTime.Now;
        LastMessageLog = DateTime.Now;
        eyeData = new EyeData();
        lookStats = new List<EnvObject>();

    }
    // Update is called once per frame
    void Update()
    {

        if (sceneEnded)
        {
            //nothing
        }
        else
        {
            //dont do anything if blinking
            var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
            if (!eyeTrackingData.IsLeftEyeBlinking && !eyeTrackingData.IsRightEyeBlinking)
            {
                pupilDilation();
                if (eyeTrackingData.GazeRay.IsValid)
                {
                    RaycastHit rHit;
                    Ray ray = new Ray(eyeTrackingData.GazeRay.Origin, eyeTrackingData.GazeRay.Direction.normalized);
                    Physics.Raycast(ray, out rHit, defaultLength);

                    //use tobii gaze ray but find the rest myself
                    if (rHit.collider != null)
                    {
                        objectTracking(rHit);
                        distanceTracking(rHit);
                        distanceFromDog(rHit);
                        //distanceFromLaser(rHit); don't use as there is no laser
                    }
                }
            }
        }
    }



    private void objectTracking(RaycastHit ray)
    {
        GameObject gameObject = ray.collider.gameObject;
        if (gameObject != null)
        {


            if (gameObject.name != currentFocus)
            {
                //deal with previous object

                //calculate the duration the user was looking at the object for
                DateTime now = DateTime.Now;
                TimeSpan duration = now - LastMessageLog;

                var obj = lookStats.FirstOrDefault(o => o.name == currentFocus);
                if (obj != null)
                {
                    obj.lookedAt += 1;
                    obj.duration += duration;
                }
                else
                {
                    lookStats.Add(new EnvObject
                    {
                        duration = duration,
                        lookedAt = 1,
                        name = currentFocus
                    });
                }
                logMessage("Duration: " + duration.ToString(@"mm\:ss\:ff"));

                //deal with current object

                currentFocus = gameObject.name;
                totalChanges += 1;
                LastMessageLog = now;


                logMessage("[" + now.ToString("ddd MMM MM H:mm:ss yyyy") + "] :: focus: " + gameObject.name);


            }
        }
        else //we haven't hit a collider
        {
            if (currentFocus != "nothing")
            {
                DateTime now = DateTime.Now;
                TimeSpan duration = now - LastMessageLog;

                var obj = lookStats.FirstOrDefault(o => o.name == currentFocus);
                if (obj != null)
                {
                    obj.lookedAt += 1;
                    obj.duration += duration;
                }
                else
                {
                    lookStats.Add(new EnvObject
                    {
                        duration = duration,
                        lookedAt = 1,
                        name = currentFocus
                    });
                }
                logMessage("Duration: " + duration.ToString(@"mm\:ss\:ff"));

                //deal with current object
                currentFocus = "nothing";
                totalChanges += 1;
                LastMessageLog = now;

                logMessage("[" + now.ToString("ddd MMM MM H:mm:ss yyyy") + "] :: focus: " + "nothing");

            }
            else
            {
                //still not looking at a collider
            }

        }
    }

    private void distanceTracking(RaycastHit ray)
    {
        if (ray.distance <= 200)
        {
            totalDistance += ray.distance;
            distanceAdded += 1;
        }
    }

    private void distanceFromDog(RaycastHit ray)
    {
        Vector3 impact = ray.point;
        GameObject dog = GameObject.Find("Dalmatian");
        if (dog != null)
        {
            Vector3 dogPos = dog.transform.position;
            var distance = Vector3.Distance(impact, dogPos);
            if (distance <= 200)
            {
                totalDistanceDog += distance;
                distanceAddedDog += 1;
            }
            if (distance < 10)
            {
                if (withinTenDog)
                {
                    //nothing
                }
                else
                {
                    withinTenDog = true;
                    LastTenDogLog = DateTime.Now;
                }
            }
            else
            {
                if (withinTenDog)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan duration = now - LastTenDogLog;
                    timeWithinTenDog += duration;

                    withinTenDog = false;
                    LastTenDogLog = now;
                }
                else
                {
                    //nothing
                }
            }
            if (distance < 5)
            {
                if (withinFiveDog)
                {
                    //nothing
                }
                else
                {
                    LastFiveDogLog = DateTime.Now;
                    withinFiveDog = true;
                }
            }
            else
            {
                if (withinFiveDog)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan duration = now - LastFiveDogLog;
                    timeWithinFiveDog += duration;

                    withinFiveDog = false;
                    LastFiveDogLog = now;
                }
                else
                {
                    //nothing
                }
            }
        }
    }

    private void distanceFromLaser(RaycastHit ray)
    {
        Vector3 impact = ray.point;
        GameObject laser = GameObject.Find("LaserPointerDot");
        if (laser != null)
        {
            Vector3 laserPos = laser.transform.position;
            var distance = Vector3.Distance(impact, laserPos);
            if (distance < 200)
            {
                totalDistanceLaser += distance;
                distanceAddedLaser += 1;
            }

            if (distance < 1)
            {
                if (withinOneLaser)
                {
                    //nothing
                }
                else
                {
                    LastOneLaserLog = DateTime.Now;
                    withinOneLaser = true;
                }
            }
            else
            {
                if (withinOneLaser)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan duration = now - LastOneLaserLog;
                    timeWithinOneLaser += duration;

                    withinOneLaser = false;
                    LastOneLaserLog = now;
                }
                else
                {
                    //nothing
                }
            }

            if (distance < 5)
            {
                if (withinFiveLaser)
                {
                    //nothing
                }
                else
                {
                    LastFiveLaserLog = DateTime.Now;
                    withinFiveLaser = true;
                }
            }
            else
            {
                if (withinFiveLaser)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan duration = now - LastFiveLaserLog;
                    timeWithinFiveLaser += duration;

                    withinFiveLaser = false;
                    LastFiveLaserLog = now;
                }
                else
                {
                    //nothing
                }
            }
        }
    }
    public void endScene()
    {
        if (!sceneEnded)
        {
            DateTime now = DateTime.Now;
            //get final object duration
            TimeSpan duration = now - LastMessageLog;

            var obj = lookStats.FirstOrDefault(o => o.name == currentFocus);
            if (obj != null)
            {
                obj.lookedAt += 1;
                obj.duration += duration;
            }
            else
            {
                lookStats.Add(new EnvObject
                {
                    duration = duration,
                    lookedAt = 1,
                    name = currentFocus
                });
            }

            //get final within distance durations if necessary

            if (withinFiveDog)
            {
                TimeSpan fiveDogduration = now - LastFiveDogLog;
                timeWithinFiveDog += fiveDogduration;
            }
            if (withinTenDog)
            {
                TimeSpan tenDogduration = now - LastTenDogLog;
                timeWithinTenDog += tenDogduration;

            }
            if (withinOneLaser)
            {
                TimeSpan oneLaserduration = now - LastOneLaserLog;
                timeWithinOneLaser += oneLaserduration;
            }
            if (withinFiveLaser)
            {
                TimeSpan fiveLaserduration = now - LastFiveLaserLog;
                timeWithinFiveLaser += fiveLaserduration;
            }

            sceneEnded = true;
            TimeSpan totalTime = new TimeSpan();
            lookStats.ForEach(o =>
            {
                totalTime += o.duration;
                logMessage(o.name);
                logMessage("total duration: " + o.duration.ToString());
                logMessage("total looks: " + o.lookedAt);
                logMessage("");
            });
            logMessage("");

            logMessage("Total time captured: " + totalTime);
            logMessage("Total changes in focus: " + totalChanges);
            logMessage("");

            logMessage("Base left dilation: " + leftPupilBase);
            var leftPup = leftEyeDiameter / leftEyeAdded;
            logMessage("Average left pupil dilation" + leftPup);
            logMessage("difference left pupil: " + (leftPup - leftPupilBase));
            logMessage("");

            logMessage("Base right dilation: " + rightPupilBase);
            var rightPup = rightEyeDiameter / rightEyeAdded;
            logMessage("Average right pupil dilation" + rightPup);
            logMessage("difference right pupil: " + (rightPup - rightPupilBase));

            logMessage("");
            logMessage("Average distance looked from self" + totalDistance / distanceAdded);
            logMessage("");
            logMessage("Time looking within 5 units of dog: " + timeWithinFiveDog.ToString());
            logMessage("Time looking within 10 units of dog: " + timeWithinTenDog.ToString());
            logMessage("Average distance looked from dog: " + totalDistanceDog / distanceAddedDog);
            logMessage("");
            logMessage("Time looking within 1 units of laser: " + timeWithinOneLaser.ToString());
            logMessage("Time looking within 5 units of laser: " + timeWithinFiveLaser.ToString());
            logMessage("Average distance looked from laser: " + totalDistanceLaser / distanceAddedLaser);
        }

    }

    private void pupilDilation()
    {
        SRanipal_Eye_API.GetEyeData(ref eyeData);
        if (eyeData.verbose_data.left.pupil_diameter_mm != -1.0)
        {
            leftEyeDiameter += eyeData.verbose_data.left.pupil_diameter_mm;
            leftEyeAdded += 1;
        }

        if (eyeData.verbose_data.right.pupil_diameter_mm != -1.0)
        {
            rightEyeDiameter += eyeData.verbose_data.right.pupil_diameter_mm;
            rightEyeAdded += 1;
        }
    }


    public void logMessage(string message)
    {
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(message);
        writer.Close();
    }
}
