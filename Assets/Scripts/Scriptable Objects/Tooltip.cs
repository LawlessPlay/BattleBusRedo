using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

//A tile type to be attached to the overlay tiles. Currently only using effect but there's lots of potential usages here. 
    [CreateAssetMenu(fileName = "TooltipSO", menuName = "ScriptableObjects/TooltipSO")]
    public class TooltipSO : ScriptableObject
    {
        public Sprite image;
        public string tooltipName;
        [TextArea(3, 10)]
        public string tooltipDescription;
    }