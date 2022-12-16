using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour
{
    string participantNumber = "";

    public TMP_InputField textBoxText;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void buttonPressed()
    {
        participantNumber = textBoxText.text;
        Debug.Log("PN: " + participantNumber);
        SceneManager.LoadScene(1);
    }

    public string getPN()
    {
        return participantNumber;
    }
}
