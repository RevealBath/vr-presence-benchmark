using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public Image image;

    public int scene = 0;

    bool fadeInComplete = false;
    bool fadeOut = false;
    bool fadeOutComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && fadeInComplete)
        {
            fadeOut = true;
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
