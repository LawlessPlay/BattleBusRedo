using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(HorizontalLayoutGroup))]
public class AutoScaleToFitHorizontal : MonoBehaviour
{
    [Range(0.25f, 1f)]
    public float minScale = 0.6f;   // don’t shrink below this
    public bool includeInactive = false;

    RectTransform _rt;
    HorizontalLayoutGroup _hg;

    void OnEnable()
    {
        _rt = GetComponent<RectTransform>();
        _hg = GetComponent<HorizontalLayoutGroup>();
        UpdateScale();
    }

    void OnRectTransformDimensionsChange() => UpdateScale();
    void OnTransformChildrenChanged() => UpdateScale();

#if UNITY_EDITOR
    void Update()  // keeps it responsive in editor; remove if you prefer manual refresh
    {
        if (!Application.isPlaying) UpdateScale();
    }
#endif

    public void UpdateScale()
    {
        if (_rt == null || _hg == null) return;

        // Available width inside the padding
        float available = Mathf.Max(0f, _rt.rect.width - _hg.padding.left - _hg.padding.right);

        // Count & sum preferred widths of children
        var children = GetComponentsInChildren<RectTransform>(includeInactive)
            .Where(c => c != _rt && c.parent == _rt); // direct children only

        int activeCount = 0;
        float totalPreferred = 0f;

        foreach (var child in children)
        {
            if (!includeInactive && !child.gameObject.activeInHierarchy) continue;

            float w = LayoutUtility.GetPreferredSize(child, 0); // 0 = horizontal
            // fallback to min size if preferred is 0 (some controls report 0)
            if (w <= 0f) w = Mathf.Max(LayoutUtility.GetMinSize(child, 0), child.rect.width);
            totalPreferred += w;
            activeCount++;
        }

        if (activeCount > 1)
            totalPreferred += _hg.spacing * (activeCount - 1);

        totalPreferred += _hg.padding.left + _hg.padding.right;

        // No children? reset
        if (activeCount == 0 || totalPreferred <= 0f)
        {
            SetScale(1f);
            return;
        }

        // Compute target scale (only shrink; never grow past 1)
        float scale = available > 0f ? Mathf.Min(1f, available / totalPreferred) : minScale;
        scale = Mathf.Clamp(scale, minScale, 1f);

        SetScale(scale);
    }

    void SetScale(float s)
    {
        var current = _rt.localScale;
        if (!Mathf.Approximately(current.x, s) || !Mathf.Approximately(current.y, s))
            _rt.localScale = new Vector3(s, s, 1f);
    }
}
