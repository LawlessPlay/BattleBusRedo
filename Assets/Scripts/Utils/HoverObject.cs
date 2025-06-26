using UnityEngine;

public class HoverObject : MonoBehaviour
{
    [Header("Hover Settings")]
    public Vector3 direction = Vector3.up; // Direction of hover
    public float speed = 1f;               // How fast it oscillates
    public float amplitude = 0.5f;         // Distance of oscillation
    public AnimationCurve hoverCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 initialPosition;
    private float timeOffset;

    void Start()
    {
        initialPosition = transform.localPosition;
        timeOffset = Random.Range(0f, 100f); // Prevent identical timing across objects
    }

    void Update()
    {
        float time = (Time.time + timeOffset) * speed;
        float curveValue = hoverCurve.Evaluate(time % 1f); // Ensure t stays within [0,1]
        transform.localPosition = initialPosition + direction.normalized * (curveValue * amplitude);
    }
}