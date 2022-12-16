using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourManagment : MonoBehaviour
{
    public Color HighlightColor = Color.red;
    public float AnimationTime = 0.0f;

    private Renderer _renderer;
    private Color _originalColor;
    public Color _targetColor;

    public void changeColour(Color colour)
    {
        _targetColor = colour;
    }
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
        _targetColor = _originalColor;
    }

    // Update is called once per frame
    void Update()
    {
        //This lerp will fade the color of the object
        if (_renderer.material.HasProperty(Shader.PropertyToID("_BaseColor"))) // new rendering pipeline (lightweight, hd, universal...)
        {
            _renderer.material.SetColor("_BaseColor", Color.Lerp(_renderer.material.GetColor("_BaseColor"), _targetColor, Time.deltaTime * (1 / AnimationTime)));
        }
        else // old standard rendering pipline
        {
            _renderer.material.color = Color.Lerp(_renderer.material.color, _targetColor, Time.deltaTime * (1 / AnimationTime));
        }
    }
}
