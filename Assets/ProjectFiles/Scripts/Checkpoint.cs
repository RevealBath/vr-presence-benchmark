using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Checkpoint
{
    public Transform location;
    public Transform look;
    public Transform escape;

    public string action = "NONE";
    public float delay = 0;
    public bool run = false;


}
