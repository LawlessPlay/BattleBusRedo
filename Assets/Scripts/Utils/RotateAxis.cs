using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAxis : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public Axis axis = Axis.X;
    public float rotationSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (axis)
        {
            case Axis.X:
                transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
                break;
            case Axis.Y:
                transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
                break;
            case Axis.Z:
                transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
                break;
            default:
                break;
        }
    }
}
