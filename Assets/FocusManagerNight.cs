using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using System.IO;
using System;
using System.Linq;
using ViveSR.anipal.Eye;
using System.Net.Mail;
using System.Net;

public class FocusManagerNight : MonoBehaviour
{
    public bool sceneEnded;

    private DateTime now; //so the data in one runthrough uses the same date time

    private List<Look> looks;
    private List<WorldPosition> worldPos;
    private List<DogPos> dogPosition;
    private List<PupilDilation> pupilDilations;
    private List<BlinkData> blinks;
    private List<GazeDistances> distances;
    private List<fogPosition> fogPositions;

    private string mainFilePath;

    private string PN;

    private EyeData eyeData;
    private float defaultLength = 200f;


    // Start is called before the first frame update
    void Start()
    {
        //get participant number
        //PN = "TODO";
        GameObject PNH = GameObject.Find("PN Holder");
        string PN = PNH.GetComponent<MenuControl>().getPN();
        mainFilePath = "results/PN " + PN + " NightPupilometry.csv";

        eyeData = new EyeData();

        //create lists
        looks = new List<Look>();
        worldPos = new List<WorldPosition>();
        pupilDilations = new List<PupilDilation>();
        blinks = new List<BlinkData>();
        distances = new List<GazeDistances>();
        dogPosition = new List<DogPos>();
        fogPositions = new List<fogPosition>();
    }
    // Update is called once per frame
    void Update()
    {
        now = DateTime.Now;
        if (!sceneEnded)
        {
            //get eye gaze data
            var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

            //eyes closed?
            blinkManagment(eyeTrackingData);

            GameObject fog = GameObject.Find("Initial Fog");
            fogPositions.Add(new fogPosition
            {
                position = fog.transform.position
            });

            //cast gaze ray
            RaycastHit rHit;
            Ray ray = new Ray(eyeTrackingData.GazeRay.Origin, eyeTrackingData.GazeRay.Direction.normalized);
            Physics.Raycast(ray, out rHit, defaultLength);

            getDogPosition();
            pupilDilation();

            if (rHit.collider != null)
            {
                GameObject gameObject = rHit.collider.gameObject;
                if (gameObject != null)
                {
                    looks.Add(new Look
                    {
                        name = gameObject.name
                    });

                }
                float gazeDist = -1; //TODO
                gazeDist = rHit.distance;

                float dogDist = distanceFromDog(rHit); //TODO
                float agencyDist = distanceFromLaser(rHit); //TODO
                distances.Add(new GazeDistances
                {
                    valid = true,
                    dist = gazeDist,
                    dogDist = dogDist,
                    agencyDist = agencyDist,
                });
            }
            else
            {
                //if no collider is hit then add empty data
                distances.Add(new GazeDistances
                {
                    valid = false,
                    dist = -1,
                    agencyDist = -1,
                    dogDist = -1,
                });
                looks.Add(new Look
                {
                    name = "-1"
                });
            }


            gatherPositionData(eyeTrackingData);
        }
    }

    private void gatherPositionData(TobiiXR_EyeTrackingData eyeData)
    {
        Vector3 empty = new Vector3
        {
            x = -1,
            y = -1,
            z = -1,
        };

        GameObject camera = GameObject.Find("Camera");
        GameObject laser = GameObject.Find("FlashlightPointer");
        GameObject flashlight = GameObject.Find("flashlight");
        var temp = new WorldPosition
        {
            id = worldPos.Count,
            timeStamp = now,
        };

        if (camera != null)
        {
            temp.position = camera.transform.position;
            temp.direction = camera.transform.forward;
        }
        else
        {
            temp.position = empty;
            temp.direction = empty;
        }
        if (flashlight != null)
        {
            temp.LasModPos = flashlight.transform.position;
            temp.LasModDir = flashlight.transform.forward;
        }
        else
        {
            temp.LasModPos = empty;
            temp.LasModDir = empty;
        }
        if (laser != null)
        {
            temp.LaserPos = laser.transform.position;
        }
        else
        {
            temp.LaserPos = empty;
        }


        //get eye data if valid
        if (eyeData.GazeRay.IsValid)
        {
            temp.GazeOrigin = eyeData.GazeRay.Origin;
            temp.GazeDirection = eyeData.GazeRay.Direction;
        }
        else
        {
            temp.GazeDirection = new Vector3
            {
                x = -1,
                y = -1,
                z = -1
            };
            temp.GazeOrigin = new Vector3
            {
                x = -1,
                y = -1,
                z = -1
            };
        }
        worldPos.Add(temp);
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

    private float distanceFromDog(RaycastHit ray)
    {
        float distance = -1;
        Vector3 impact = ray.point;
        GameObject dog = GameObject.Find("Monster Dog");
        if (dog != null)
        {
            Vector3 dogPos = dog.transform.position;
            distance = Vector3.Distance(impact, dogPos);
        }
        return distance;
    }

    private void getDogPosition()
    {
        GameObject dog = GameObject.Find("Monster Dog");
       
        if (dog != null)
        {
            CreatureControllerV2 creatureCon = dog.GetComponent<CreatureControllerV2>();
            dogPosition.Add(new DogPos
            {
                dogPos = dog.transform.position,
                dogDirection = dog.transform.forward,
                animation = creatureCon.animationInUse,
                walking = creatureCon.walking,
                soundtrack = creatureCon.soundtrack,
            });
        } else
        {
            dogPosition.Add(new DogPos
            {
                dogPos = new Vector3
                {
                    x = -1,
                    y = -1,
                    z = -1,
                },
                dogDirection = new Vector3
                {
                    x = -1,
                    y = -1,
                    z = -1
                }
            });
        }
    }
    private float distanceFromLaser(RaycastHit ray)
    {
        float distance = -1;
        Vector3 impact = ray.point;
        GameObject laser = GameObject.Find("FlashlightPointer");
        if(laser != null)
        {
            Vector3 laserPos = laser.transform.position;
            distance = Vector3.Distance(impact, laserPos);
        }else
        {
            distance = -1;
        }

        return distance;
    }

    private void pupilDilation()
    {
        SRanipal_Eye_API.GetEyeData(ref eyeData);
        PupilDilation pup = new PupilDilation();

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


    public void endScene()
    {
        if (!sceneEnded)
        {
            sceneEnded = true;
            writeMain();
        }

    }

    public void writeMain()
    {
        StreamWriter writer = new StreamWriter(mainFilePath);
        writer.WriteLine("id,timestamp,pos.x pos.y pos.z,dir.x dir.y dir.z,las.x las.y las.z," +
            "lasModPos.x lasModPos.y lasModPos.z,lasModDir.x lasModDir.y lasModDir.z," +
            "gazePos.x gazePos.y gazePos.z,gazeDir.x gazeDir.y gazeDir.z,looking at object," +
            "fogPos.x fogPos.y fogPos.z," +
            "pupil dilation left,pupil dilation right,eye closed left, eye closed right," +
            "gaze ray hit collider,distance looking from self,distance looking from dog," +
            "distance from laserdot,dogPos.x dogPos.y dogPos.z,dogDir.x dogDir.y dogDir.z," +
            "animation,walking,audio,soundtrack");

        for (int i = 0; i < dogPosition.Count; i++)
        {
            writer.WriteLine(worldPos[i].id + "," + worldPos[i].timeStamp.ToString("dd/MM/yyyy hh:mm:ss.fff") +
                "," + worldPos[i].position.x + "|" + worldPos[i].position.y + "|" + worldPos[i].position.z +
                "," + worldPos[i].direction.x + "|" + worldPos[i].direction.y + "|" + worldPos[i].direction.z +
                "," + worldPos[i].LaserPos.x + "|" + worldPos[i].LaserPos.y + "|" + worldPos[i].LaserPos.z +
                "," + worldPos[i].LasModPos.x + "|" + worldPos[i].LasModPos.y + "|" + worldPos[i].LasModPos.z +
                "," + worldPos[i].LasModDir.x + "|" + worldPos[i].LasModDir.y + "|" + worldPos[i].LasModDir.z +
                "," + worldPos[i].GazeOrigin.x + "|" + worldPos[i].GazeOrigin.y + "|" + worldPos[i].GazeOrigin.z +
                "," + worldPos[i].GazeDirection.x + "|" + worldPos[i].GazeDirection.y + "|" + worldPos[i].GazeDirection.z +
                "," + looks[i].name + "," + fogPositions[i].position.x + "|" + fogPositions[i].position.y + "|" + fogPositions[i].position.z +
                "," + pupilDilations[i].leftDilation + "," + pupilDilations[i].rightDilation +
                "," + blinks[i].left + "," + blinks[i].right + "," + distances[i].valid + "," + distances[i].dist + "," + distances[i].dogDist +
                "," + distances[i].agencyDist + "," + dogPosition[i].dogPos.x + "|" + dogPosition[i].dogPos.y +
                "|" + dogPosition[i].dogPos.z + "," + dogPosition[i].dogDirection.x + "|" + dogPosition[i].dogDirection.y +
                "|" + dogPosition[i].dogDirection.z + "," + dogPosition[i].animation + "," + dogPosition[i].walking +
                "," + dogPosition[i].soundtrack);
        }
        writer.WriteLine(worldPos.Count + "," + looks.Count + "," + pupilDilations.Count + "," + blinks.Count + "," + distances.Count + "," + dogPosition.Count);
        writer.Flush();
        writer.Close();
    }

}

public class fogPosition 
{
    public Vector3 position;
}
