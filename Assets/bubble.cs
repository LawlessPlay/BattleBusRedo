using UnityEngine;
using TMPro;
using Unity.VisualScripting;

[RequireComponent(typeof(RectTransform))]
public class BubbleSizer : MonoBehaviour
{
    public TextMeshProUGUI text;                // assign your TMP child
    public RectTransform background;            // usually this object's RectTransform
    public Vector2 padding = new Vector2(24,12);// x = left+right, y = top+bottom
    public float maxHeight = 120f;
    public float maxWidth  = Mathf.Infinity;    // set to e.g. 400 to enable wrapping cap

    // change-tracking to avoid needless work
    string _lastText;
    float  _lastFontSize;
    Vector2 _lastScale;

    void Reset()
    {
        background = GetComponent<RectTransform>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        Recalculate();
    }

    void OnRectTransformDimensionsChange()
    {
        // parent/layout changes that could affect text wrapping
        Recalculate();
    }

    void LateUpdate()
    {
        if (!text) return;

        // Re-run when TMP flags changes or when obvious inputs change
        if (text.havePropertiesChanged ||
            _lastText != text.text ||
            Mathf.Abs(_lastFontSize - text.fontSize) > 0.001f ||
            _lastScale != new Vector2(text.rectTransform.lossyScale.x, text.rectTransform.lossyScale.y))
        {
            Recalculate();
        }
    }

    void Recalculate()
    {
        if (!text || !background) return;

        float widthLimit  = float.IsInfinity(maxWidth) ? 10000f : Mathf.Max(1f, maxWidth - padding.x);

        // Make TMP lay out as if constrained to widthLimit
        var tRect = text.rectTransform;
        tRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthLimit);
        text.ForceMeshUpdate();

        // Longest rendered line
        float contentW = 0f;
        var ti = text.textInfo;
        for (int i = 0; i < ti.lineCount; i++)
        {
            var line = ti.lineInfo[i];
            float lineW = line.lineExtents.max.x - line.lineExtents.min.x; // actual rendered width
            if (lineW <= 0) lineW = line.maxAdvance; // fallback for older TMP
            contentW = Mathf.Max(contentW, lineW);
        }

        // Height after wrapping at widthLimit
        float contentH = text.preferredHeight;

        float w = Mathf.Min(contentW + padding.x, maxWidth);
        float h = Mathf.Min(contentH + padding.y, maxHeight);
        
        background.localPosition = new Vector2(w/2, h/2);

        background.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        background.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   h);
    }

}
