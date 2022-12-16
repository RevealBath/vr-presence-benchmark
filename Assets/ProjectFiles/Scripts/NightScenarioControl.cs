using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightScenarioControl : MonoBehaviour
{

    public GameObject[] parkLamps;
    public GameObject[] streetLights;
    public GameObject monster;

    public float worldTime = 0;

    bool streetLightsDisabled = false;
    bool lightsDisabled = false;
    bool monsterAppeared = false;

    IEnumerator LampOffCoroutine()
    {
        int lampIndex = 0;
        while (lampIndex < parkLamps.Length)
        {
            parkLamps[lampIndex].GetComponent<LampControl>().shutOffLight();
            lampIndex++;
            yield return new WaitForSeconds(1.0f);
        }
        if (lampIndex >= parkLamps.Length)
        {
            StopCoroutine(LampOffCoroutine());
        }

    }
    IEnumerator StreetOffCoroutine()
    {
        int streetIndex = 0;
        while (streetIndex < streetLights.Length)
        {
            streetLights[streetIndex].GetComponent<LampControl>().shutOffLight();
            streetIndex++;
            yield return new WaitForSeconds(1.0f);
        }
        if (streetIndex >= streetLights.Length)
        {
            StopCoroutine(LampOffCoroutine());
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        monster.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        worldTime += Time.deltaTime;
        if (worldTime > 5f && !streetLightsDisabled)
        {
            StartCoroutine(StreetOffCoroutine());
            streetLightsDisabled = true;
        }
        if (worldTime > 15f && !monsterAppeared) // Monster appears on path in distance (only eyes should really be visible)
        {
            monster.SetActive(true);
            monsterAppeared = true;
        }

        if (worldTime > 15f && !lightsDisabled)
        {
            StartCoroutine(LampOffCoroutine());
            lightsDisabled = true;
        }
    }
}
