using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TacticsToolkit
{
    public class ResetRotation : MonoBehaviour
    {
        void LateUpdate()
        {
            // Get the camera's rotation
            Quaternion cameraRotation = Camera.main.transform.rotation;

            // Set the object's rotation to match the corrected camera rotation
            transform.rotation = Quaternion.Euler(0, cameraRotation.eulerAngles.y, 0);

        }
    }
}
