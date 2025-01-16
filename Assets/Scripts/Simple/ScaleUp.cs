using UnityEngine;

public class ScaleUp : MonoBehaviour
{
    public Vector3 targetScale = Vector3.one; // The target scale
    public float duration = 1f; // Time in seconds to complete the scaling
    public EasingType easingType = EasingType.Linear; // The easing type
    public AnimationCurve customCurve; // Custom curve for scaling
    public bool useAnimationCurve = false; // Whether to use the custom animation curve

    private Vector3 initialScale;
    private float elapsedTime;

    void Awake()
    {
        initialScale = transform.localScale;
        //StartCoroutine(ScaleCoroutine());
    }

    private System.Collections.IEnumerator ScaleCoroutine()
    {
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = useAnimationCurve ? customCurve.Evaluate(t) : ApplyEasing(t);

            transform.localScale = Vector3.LerpUnclamped(initialScale, targetScale, easedT);
            yield return null;
        }

        transform.localScale = targetScale; // Ensure final scale is set
    }

    private float ApplyEasing(float t)
    {
        switch (easingType)
        {
            case EasingType.Linear:
                return t;
            case EasingType.EaseIn:
                return t * t;
            case EasingType.EaseOut:
                return t * (2 - t);
            case EasingType.EaseInOut:
                return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
            default:
                return t;
        }
    }

    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }
}
