using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public float updateInterval = 0.5f; // Interval at which the FPS is updated
    private TMP_Text fpsText;
    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeLeft; // Time left for current interval
    private Color defaultColor;
    private void Start()
    {
        fpsText = GetComponent<TMP_Text>();
        defaultColor = fpsText.color;
        if (!fpsText)
        {
            Debug.LogError("No TextMeshPro Text component found on the same GameObject as the FPSCounter script!");
            enabled = false; // Disable the script if no TextMeshPro Text component is found
            return;
        }
        timeLeft = updateInterval;
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // When interval is reached, update the FPS text
        if (timeLeft <= 0.0)
        {
            float fps = accum / frames;
            fpsText.text = $"{fps:F2}";

            if (fps < 30)
            {
                fpsText.color = Color.yellow;
            }
            else if (fps < 10)
            {
                fpsText.color = Color.red;
            }
            else
            {
                fpsText.color = defaultColor ;
            }

            timeLeft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }
}
