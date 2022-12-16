using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System;

public class LightPupilSceneOutController : MonoBehaviour
{

    public Image image;

    public int scene = 0;
    DateTime sceneStart;
    public int sceneDuration;

    bool fadeInComplete = false;
    bool fadeOut = false;
    bool fadeOutComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        sceneStart = DateTime.Now;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (DateTime.Now > sceneStart.AddSeconds(sceneDuration) && !fadeOut)
        {
            fadeOut = true;
            GameObject pupil = GameObject.Find("pupil out");
            pupil.GetComponent<PupilOut>().endScene();
        }
        if (!fadeInComplete)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - 0.005f);
            if (image.color.a <= 0) { fadeInComplete = true; }
        }
        if (fadeOut)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a + 0.005f);
            if (image.color.a >= 1) { fadeOutComplete = true; }
        }

        if (fadeOutComplete)
        {
            SceneManager.LoadScene(scene);
        }
    }
}
