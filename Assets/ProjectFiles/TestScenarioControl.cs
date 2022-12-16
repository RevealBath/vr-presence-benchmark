using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScenarioControl : MonoBehaviour
{
    public MonsterController monster;
    public float worldTime = 0;

    private bool[] taskComplete = new bool[20];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            taskComplete[i] = false;
        }
        monster.setMoveTarget(1, false);
    }

    // Update is called once per frame
    void Update()
    {
        worldTime += Time.deltaTime;

        //script world event, add bool to each function so it isn't repeated when done
        if (worldTime > 10f)
        {
            if (!taskComplete[0])
            {
                monster.setMoveTarget(2, false);
                taskComplete[0] = true;
            }
            
        }
        if (worldTime > 25f)
        {
            if (!taskComplete[1])
            {
                monster.setMoveTarget(3, true);
                taskComplete[1] = true;
            }
        }

    }
}
