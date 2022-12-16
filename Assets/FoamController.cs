using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamController : MonoBehaviour
{

    public Transform fish;

    private MeshRenderer mRender;


    // Start is called before the first frame update
    void Start()
    {
        mRender = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fish != null)
        {
            Vector3 fishPosition = fish.position;
            Quaternion fishRotation = fish.rotation;

            Vector3 newRotation = new Vector3(0, fish.transform.eulerAngles.y - 90, 90);

            fishPosition.y = transform.position.y;

            transform.position = fishPosition;
            transform.eulerAngles = newRotation;


        }

        mRender.material.mainTextureOffset = new Vector2(mRender.material.mainTextureOffset.x, mRender.material.mainTextureOffset.y - 0.01f);


    }
}
