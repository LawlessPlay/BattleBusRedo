#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileCardFactory : MonoBehaviour
{
    [MenuItem("GameObject/UI/Create TileCard Prefab", false, 10)]
    public static void CreateTileCard()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (!canvas)
        {
            var goCanvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = goCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = goCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        GameObject MakeGO(string name, Transform parent, params System.Type[] comps)
        {
            var go = new GameObject(name, comps);
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            return go;
        }

        // Root
        var card = MakeGO("TileCard",
            canvas.transform,
            typeof(Image),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter),
            typeof(TileCardView));
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.sizeDelta = new Vector2(340, 0); // width fixed, height auto
        var bg = card.GetComponent<Image>();
        bg.type = Image.Type.Sliced; // assign your 9-slice later

        var vlg = card.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.spacing = 4;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        var csf = card.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // TopRow
        var topRow = MakeGO("TopRow", card.transform, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        var topLE = topRow.GetComponent<LayoutElement>();
        topLE.minHeight = 32;
        var topHLG = topRow.GetComponent<HorizontalLayoutGroup>();
        topHLG.spacing = 6;
        topHLG.childAlignment = TextAnchor.MiddleLeft;
        topHLG.childControlHeight = true;
        topHLG.childControlWidth = false;
        topHLG.childForceExpandWidth = false;
        topHLG.childForceExpandHeight = false;

        // Top children
        var tileIcon = MakeGO("TileIcon", topRow.transform, typeof(Image), typeof(LayoutElement));
        tileIcon.GetComponent<LayoutElement>().preferredWidth = 18;
        tileIcon.GetComponent<LayoutElement>().preferredHeight = 18;

        var nameTextGO = MakeGO("NameText", topRow.transform, typeof(TextMeshProUGUI), typeof(LayoutElement));
        var nameTMP = nameTextGO.GetComponent<TextMeshProUGUI>();
        nameTMP.text = "Grass";
        nameTMP.enableWordWrapping = false;
        nameTMP.overflowMode = TextOverflowModes.Ellipsis;
        var nameLE = nameTextGO.GetComponent<LayoutElement>();
        nameLE.flexibleWidth = 1;

        var arrowIcon = MakeGO("ArrowIcon", topRow.transform, typeof(Image), typeof(LayoutElement));
        arrowIcon.GetComponent<LayoutElement>().preferredWidth = 14;
        arrowIcon.GetComponent<LayoutElement>().preferredHeight = 14;

        var tierTextGO = MakeGO("TierText", topRow.transform, typeof(TextMeshProUGUI));
        tierTextGO.GetComponent<TextMeshProUGUI>().text = "2";

        // BottomRow
        var bottomRow = MakeGO("BottomRow", card.transform, typeof(HorizontalLayoutGroup));
        var bHLG = bottomRow.GetComponent<HorizontalLayoutGroup>();
        bHLG.spacing = 6;
        bHLG.childControlWidth = true;
        bHLG.childControlHeight = true;
        bHLG.childForceExpandWidth = false;
        bHLG.childForceExpandHeight = true;
        bHLG.childAlignment = TextAnchor.UpperLeft;

        // MoveCostCol
        var moveCol = MakeGO("MoveCostCol", bottomRow.transform, typeof(VerticalLayoutGroup), typeof(LayoutElement));
        moveCol.GetComponent<LayoutElement>().preferredWidth = 56;
        var moveVLG = moveCol.GetComponent<VerticalLayoutGroup>();
        moveVLG.spacing = 2;
        moveVLG.childAlignment = TextAnchor.MiddleCenter;
        moveVLG.childControlHeight = true;
        moveVLG.childControlWidth = true;

        var runnerIcon = MakeGO("RunnerIcon", moveCol.transform, typeof(Image), typeof(LayoutElement));
        runnerIcon.GetComponent<LayoutElement>().preferredWidth = 16;
        runnerIcon.GetComponent<LayoutElement>().preferredHeight = 16;

        var moveTextGO = MakeGO("MoveCostText", moveCol.transform, typeof(TextMeshProUGUI));
        moveTextGO.GetComponent<TextMeshProUGUI>().text = "1";

        // Divider
        var divider = MakeGO("Divider", bottomRow.transform, typeof(Image), typeof(LayoutElement));
        divider.GetComponent<LayoutElement>().preferredWidth = 2;
        divider.GetComponent<Image>().type = Image.Type.Simple;

        // DescCol + text
        var descCol = MakeGO("DescCol", bottomRow.transform, typeof(RectTransform), typeof(LayoutElement));
        descCol.GetComponent<LayoutElement>().flexibleWidth = 1;

        var descTextGO = MakeGO("DescriptionText", descCol.transform, typeof(TextMeshProUGUI));
        var descTMP = descTextGO.GetComponent<TextMeshProUGUI>();
        descTMP.enableWordWrapping = true;
        descTMP.text = "Feels nice between your toes";

        // Wire TileCardView refs
        var view = card.GetComponent<TileCardView>();
        view.tileIcon = tileIcon.GetComponent<Image>();
        view.nameText = nameTMP;
        view.arrowIcon = arrowIcon.GetComponent<Image>();
        view.tierText = tierTextGO.GetComponent<TextMeshProUGUI>();
        view.runnerIcon = runnerIcon.GetComponent<Image>();
        view.moveCostText = moveTextGO.GetComponent<TextMeshProUGUI>();
        view.descriptionText = descTMP;
        view.rootRect = card.GetComponent<RectTransform>();

        // Select and (optionally) save prefab
        Selection.activeGameObject = card;

        var dir = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        var path = $"{dir}/TileCard.prefab";
        PrefabUtility.SaveAsPrefabAsset(card, path, out bool success);
        Debug.Log(success ? $"Saved prefab at {path}" : "Failed to save prefab.");
    }
}
#endif
