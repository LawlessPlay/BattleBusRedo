using UnityEngine;

public class GrassInWind : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swaySpeed = 1f; // Speed of the swaying motion
    public float swayAmount = 15f; // Maximum sway angle in degrees

    [Header("Wind Settings")]
    public float windVarianceSpeed = 0.5f; // Speed of wind strength variation
    public float windVarianceAmount = 5f; // Maximum variance in sway amount

    private float baseAngle;
    private float randomOffset;

    void Start()
    {
        // Store the initial rotation angle of the blade of grass
        baseAngle = transform.localEulerAngles.z;

        // Add a random offset to the sine wave for variation
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // Calculate the dynamic sway amount with wind variance
        float dynamicSwayAmount = swayAmount + Mathf.Sin(Time.time * windVarianceSpeed) * windVarianceAmount;

        // Use a sine wave to calculate the swaying angle
        float swayAngle = Mathf.Sin(Time.time * swaySpeed + randomOffset) * dynamicSwayAmount;

        // Apply the swaying angle to the blade of grass
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            baseAngle + swayAngle
        );
    }
}
