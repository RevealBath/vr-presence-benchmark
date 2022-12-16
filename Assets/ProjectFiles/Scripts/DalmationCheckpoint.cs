using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DalmationCheckpoint
{
    public Transform location;
    public Transform look;

    public string action = "NONE";
    public float delay = 0;
    public bool run = false;


}
