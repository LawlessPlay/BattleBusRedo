using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileCardView : MonoBehaviour
{
    [Header("Top")]
    public Image tileIcon;
    public TextMeshProUGUI nameText;
    public Image arrowIcon;
    public TextMeshProUGUI tierText;

    [Header("Bottom")]
    public Image runnerIcon;
    public TextMeshProUGUI moveCostText;
    public TextMeshProUGUI descriptionText;

    [Header("Optional")]
    public RectTransform rootRect; // assign TileCard rect

    public void SetData(Sprite icon, string tileName, int tier, Sprite runner, int moveCost, string desc)
    {
        if (tileIcon) tileIcon.sprite = icon;
        if (nameText) nameText.text = tileName;              // one line, ellipsis
        if (tierText) tierText.text = tier.ToString();
        if (runnerIcon) runnerIcon.sprite = runner;
        if (moveCostText) moveCostText.text = moveCost.ToString();
        if (descriptionText) descriptionText.text = desc;    // drives height

        if (rootRect) LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
    }
}