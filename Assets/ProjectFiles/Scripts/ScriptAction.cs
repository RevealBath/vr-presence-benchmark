using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScriptAction
{
    public string actionName;
    public int[] parameters;
    public bool completed = false;
}

public class MoveAction : ScriptAction
{ 
    public MoveAction(int x, int y, bool running)
    {
        actionName = "Move";
        parameters = new int[3];
        parameters[0] = x;
        parameters[1] = y;      
    }


    public int getXCoord(){ return parameters[0]; }
    public int getZCoord() { return parameters[1]; } //z coord

    public void setXCoord(int x) { parameters[0] = x; }
    public void setZCoord(int z) { parameters[1] = z; }

    public void setRunning(bool running)
    {
        if (running) { parameters[2] = 1; }
        else { parameters[2] = 0; }
    }
    public bool getRunning()
    {
        if(parameters[2] > 0) { return true; }
        else { return false; } //default to walking
    }
}
public class LookAction : ScriptAction
{
    private string targetType; // Player | Checkpoint

    public LookAction(string type, int target)
    {
        actionName = "Look";
        targetType = type;
        parameters[0] = target;
    }

    public void setTargetType(string type) { targetType = type; }
    public string getTargetType() { return targetType; }

    public void setTarget(int target){ parameters[0] = target; }
    public int getTarget() { return parameters[0]; }

}
