using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionFlipper : MonoBehaviour
{
    public Vector3 defaultPostion;
    public Vector3 defaultRotation;
    public Vector3 flippedPostion;
    public Vector3 flippedRotation;


    // Start is called before the first frame update
    void Start()
    {
        defaultPostion = transform.localPosition;
        defaultRotation = transform.eulerAngles;

        flippedPostion = new Vector3(Mathf.Abs(defaultPostion.x), defaultPostion.y, defaultPostion.z);
        flippedRotation = new Vector3(-Mathf.Abs(defaultRotation.x), defaultRotation.y + 180, defaultRotation.z);
    }

    public void SetSide(bool isRight)
    {
        if (isRight)
        {
            transform.localPosition = defaultPostion;
            transform.localEulerAngles = defaultRotation;
        } else
        {
            transform.localPosition = flippedPostion;
            transform.localEulerAngles = flippedRotation;
        }
    }
}
