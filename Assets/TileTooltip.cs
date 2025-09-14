using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileTooltip : MonoBehaviour,ITooltip
{
    public TMP_Text height;
    public TMP_Text tileName;
    public TMP_Text description;
    public Image image;
    public TMP_Text cost;

    public void SetContent(OverlayTile tile)
    {
        height.text = Mathf.RoundToInt(tile.transform.position.y).ToString();
        tileName.text = tile.tileData.tooltip.tooltipName;
        image.sprite = tile.tileData.tooltip.image;
        description.text = tile.tileData.tooltip.tooltipDescription;
        cost.text = tile.GetMoveCost().ToString();
    }

    public void SetContent(Ability ability)
    {
        throw new System.NotImplementedException();
    }

    public void SetContent(Entity entity)
    {
        throw new System.NotImplementedException();
    }
}

interface ITooltip
{
    void SetContent(OverlayTile tile);
    void SetContent(Ability ability);
    void SetContent(Entity entity);
    
}
