using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableSound : MonoBehaviour
{
    SoundManager soundOut;

    // Start is called before the first frame update
    void Start()
    {
        soundOut = GetComponent<SoundManager>();
        soundOut.Play("Pond");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
