using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderDisplayTwo : MonoBehaviour
{
    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    public RectTransform container;
    public GameObject iconPrefab;
    public Orientation orientation = Orientation.Vertical;
    public Orientation insertOrientation = Orientation.Vertical;
    public float marginX = 0f, marginY = 0f;
    public float spacing = 10f;

    public List<TurnOrderObject> currentOrder = new List<TurnOrderObject>();
    private List<RectTransform> iconRects = new List<RectTransform>();
    private List<Image> iconImages = new List<Image>();


    public void SetTurnOrderList(List<TurnOrderObject> order)
    {
        StopCoroutine("MoveToPosition");
        // Remove old icons
        foreach (RectTransform rect in iconRects)
        {
            Destroy(rect.gameObject);
        }

        iconRects.Clear();
        iconImages.Clear();
        currentOrder.Clear();

        currentOrder.AddRange(order);
        if (currentOrder.Count == 0) return;

        // Optionally determine icon size from the prefab (for positioning)
        RectTransform prefabRect = iconPrefab.GetComponent<RectTransform>();
        float iconWidth = prefabRect.sizeDelta.x;
        float iconHeight = prefabRect.sizeDelta.y;

        // Instantiate icons for each character in order
        for (int i = 0; i < currentOrder.Count; i++)
        {
            Entity character = currentOrder[i].character;
            GameObject iconGO = Instantiate(iconPrefab);
            iconGO.transform.SetParent(container, false); // parent to container:contentReference[oaicite:4]{index=4}
            RectTransform rt = iconGO.GetComponent<RectTransform>();
            Image img = iconGO.GetComponent<Image>();
            if (img != null && character.portrait != null)
            {
                img.sprite = character.portrait;
            }

            // Calculate anchored position based on orientation
            Vector2 pos = Vector2.zero;
            if (orientation == Orientation.Vertical)
            {
                pos.x = marginX;
                pos.y = -marginY - i * (iconHeight + spacing);
            }
            else
            {
                // Horizontal
                pos.x = marginX + i * (iconWidth + spacing);
                pos.y = -marginY;
            }

            rt.anchoredPosition = pos; // position the icon:contentReference[oaicite:5]{index=5}
            iconRects.Add(rt);
            iconImages.Add(img);
        }
    }

    public float moveSpeed = 500f; // animation speed in UI units per second

    public void PopOffFirst(System.Action onComplete = null)
    {
        if (currentOrder.Count == 0) return;
        // Get the first icon
        RectTransform firstRect = iconRects[0];
        // Determine off-screen target (above the first icon for vertical, to the left for horizontal)
        Vector2 offTarget = firstRect.anchoredPosition;
        if (insertOrientation == Orientation.Vertical)
        {
            offTarget.y += 20f; // slide 200px up (adjust as needed or calculate from icon size)
        }
        else
        {
            offTarget.x -= 20f; // slide 200px left
        }

        // Start coroutine to animate removal
        StartCoroutine(PopOffCoroutine(firstRect, offTarget, onComplete));
        
        // Remove the first character data from the list (we'll remove the UI element in the coroutine)
        currentOrder.RemoveAt(0);
    }

    private IEnumerator PopOffCoroutine(RectTransform firstRect, Vector2 offTarget, System.Action onComplete)
    {
        // 1. Animate first icon off-screen
        Vector2 startPos = firstRect.anchoredPosition;
        float distance = Vector2.Distance(startPos, offTarget);
        float duration = distance / moveSpeed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            
            if (firstRect == null || firstRect.Equals(null)) yield break;
            
            firstRect.anchoredPosition = Vector2.Lerp(startPos, offTarget, t);
            yield return null;
        }

        firstRect.anchoredPosition = offTarget;
        // Destroy the first icon object
        Destroy(firstRect.gameObject);
        iconRects.RemoveAt(0);
        iconImages.RemoveAt(0);
        
        // 2. Shift remaining icons to their new positions
        // Recalculate target positions for each remaining icon (index i becomes i-1)
        for (int i = 0; i < iconRects.Count; i++)
        {
            RectTransform rt = iconRects[i];
            // Compute the new anchored position for this index
            Vector2 targetPos;
            if (orientation == Orientation.Vertical)
            {
                targetPos = new Vector2(marginX, -marginY - i * (rt.rect.height + spacing));
            }
            else
            {
                targetPos = new Vector2(marginX + i * (rt.rect.width + spacing), -marginY);
            }

            // Animate each icon to its new position
            StartCoroutine(MoveToPosition(rt, rt.anchoredPosition, targetPos));
        }

        // Wait until the longest shift movement is done (so that we only call onComplete after all moves)
        // Here we estimate duration of one slot shift based on moveSpeed:
        float shiftDistance;
        if (iconRects.Count > 0)
        {
            shiftDistance = (orientation == Orientation.Vertical)
                ? (iconRects[0].rect.height + spacing)
                : (iconRects[0].rect.width + spacing);
        }
        else
        {
            shiftDistance = 0;
        }

        yield return new WaitForSeconds(shiftDistance / moveSpeed);
        
        // 3. Invoke callback if provided (to signal animation complete)
        onComplete?.Invoke();
    }

// Utility coroutine to move an icon from startPos to endPos smoothly
    private IEnumerator MoveToPosition(RectTransform rt, Vector2 startPos, Vector2 endPos)
    {
        if (rt != null)
        {
            float distance = Vector2.Distance(startPos, endPos);
            float duration = (moveSpeed > 0) ? distance / moveSpeed : 0.1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                if (rt == null || rt.Equals(null)) yield break;
                
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            if (rt == null || rt.Equals(null)) yield break;
            
            rt.anchoredPosition = endPos;
        }
        yield return null;
    }

    public void InsertAt(int index, TurnOrderObject newChar)
    {
        // Clamp index to valid range [0, currentOrder.Count]
        if (index < 0) index = 0;
        if (index > currentOrder.Count) index = currentOrder.Count;

        // 1. Update the data and UI lists
        currentOrder.Insert(index, newChar);
        GameObject iconGO = Instantiate(iconPrefab);
        iconGO.transform.SetParent(container, false);
        RectTransform newRect = iconGO.GetComponent<RectTransform>();
        Image newImg = iconGO.GetComponent<Image>();
        if (newImg != null && newChar.character.portrait != null)
        {
            newImg.sprite = newChar.character.portrait;
        }

        // Insert the RectTransform and Image into our lists at the correct index
        iconRects.Insert(index, newRect);
        iconImages.Insert(index, newImg);

        // Determine slot size (assuming uniform size icons)
        float iconWidth = newRect.rect.width;
        float iconHeight = newRect.rect.height;

        // 2. Set new icon's starting position (just outside the gap)
        Vector2 startPos = Vector2.zero;
        
        if (insertOrientation == Orientation.Vertical)
        {
            // Start slightly above where the new slot will be
            startPos.x = marginX;
            startPos.y = -marginY - (index * (iconHeight + spacing)) + 50f;
            // (Here +50f moves it up 50 px from its target, adjust as needed)
        }
        else
        {
            // Start slightly to the left of where the new slot will be
            startPos.x = marginX- (index * (iconWidth + spacing))- 50f;
            startPos.y = -marginY ;
        }

        newRect.anchoredPosition = startPos;

        // 3. Shift existing icons at and after the insertion index to their new positions
        for (int i = index + 1; i < iconRects.Count; i++)
        {
            RectTransform rt = iconRects[i];
            Vector2 target;
            if (orientation == Orientation.Vertical)
            {
                target = new Vector2(marginX, -marginY - i * (iconHeight + spacing));
            }
            else
            {
                target = new Vector2(marginX + i * (iconWidth + spacing), -marginY);
            }

            StartCoroutine(MoveToPosition(rt, rt.anchoredPosition, target));
        }

        // 4. Animate the new icon into its proper slot position
        Vector2 newTarget;
        if (orientation == Orientation.Vertical)
        {
            newTarget = new Vector2(marginX, -marginY - index * (iconHeight + spacing));
        }
        else
        {
            newTarget = new Vector2(marginX + index * (iconWidth + spacing), -marginY);
        }

        StartCoroutine(MoveToPosition(newRect, startPos, newTarget));
    }

    private List<TurnOrderObject> originalOrderBackup = new List<TurnOrderObject>();
    public bool isPreviewMode = false;
    private List<Entity> previewTargetChars;

    public float previewOffset = 10f; // how far to offset previewed icons
    public float previewAlpha = 0.7f; // transparency for previewed icons (1 = fully opaque)
    public Color previewTintColor = Color.yellow; // tint color for preview (e.g., yellow highlight)

    public void StartPreview(List<TurnOrderObject> newOrder, List<Entity> affectedChars)
    {
        if (isPreviewMode)
        {
            CancelPreview(); // revert any existing preview first
        }

        // Backup the current order data
        originalOrderBackup = new List<TurnOrderObject>(currentOrder);
        // Display the new order list (instantly, no animation here)
        SetTurnOrderList(newOrder);
        
        previewTargetChars = affectedChars;
        foreach (var affectedChar in affectedChars)
        {
        isPreviewMode = true;
        // Apply preview styling to all instances of the affected character
        for (int i = 0; i < currentOrder.Count; i++)
        {
            if (currentOrder[i].character == affectedChar)
            {
                // Offset the icon's position 
                if (orientation == Orientation.Vertical)
                {
                    iconRects[i].anchoredPosition += new Vector2(previewOffset, 0);
                }
                else
                {
                    iconRects[i].anchoredPosition += new Vector2(0, -previewOffset);
                }

                // Adjust transparency and tint
                if (iconImages[i] != null)
                {
                    // For example, tint the image to yellow and reduce opacity
                    Color baseColor = iconImages[i].color;
                    iconImages[i].color = new Color(previewTintColor.r, previewTintColor.g, previewTintColor.b,
                        previewAlpha);
                }

                // Enable outline component if available for extra highlight
                Outline outline = iconRects[i].GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                    outline.effectColor = previewTintColor;
                }
            }
            
        }
        }
    }

    public void ConfirmPreview()
    {
        if (!isPreviewMode) return;
        // Remove the offset and restore normal appearance for previously affected icons
        for (int i = 0; i < currentOrder.Count; i++)
        {
            if (previewTargetChars.Contains(currentOrder[i].character))
            {
                if (orientation == Orientation.Vertical)
                {
                    iconRects[i].anchoredPosition -= new Vector2(previewOffset, 0);
                }
                else
                {
                    iconRects[i].anchoredPosition -= new Vector2(0, -previewOffset);
                }

                if (iconImages[i] != null)
                {
                    // Assuming original color was full opacity white (no tint)
                    iconImages[i].color = new Color(1f, 1f, 1f, 1f);
                }

                Outline outline = iconRects[i].GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = false;
                }
            }
        }

        // The currentOrder remains as the new order now
        originalOrderBackup.Clear();
        isPreviewMode = false;
        previewTargetChars = null;
    }

    public void CancelPreview()
    {
        if (!isPreviewMode) return;
        // Restore the original turn order
        SetTurnOrderList(originalOrderBackup);
        originalOrderBackup.Clear();
        isPreviewMode = false;
        previewTargetChars = null;
    }

    public void RemoveAt(int index, System.Action onComplete = null)
    {
        if (index < 0 || index >= currentOrder.Count)
        {
            Debug.LogWarning("RemoveAt: Invalid index.");
            return;
        }

        RectTransform iconToRemove = iconRects[index];

        // Animate off-screen + fade out
        Vector2 offTarget = iconToRemove.anchoredPosition;
        if (insertOrientation == Orientation.Vertical)
            offTarget.y += 200f;
        else
            offTarget.x -= 200f;

        StartCoroutine(RemoveAtCoroutine(index, iconToRemove, offTarget, onComplete));
    }

    private IEnumerator RemoveAtCoroutine(int index, RectTransform iconToRemove, Vector2 offTarget,
        System.Action onComplete)
    {
        Image img = iconImages[index];
        Color startColor = img.color;
        Color fadeColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        Vector2 startPos = iconToRemove.anchoredPosition;
        float duration = Vector2.Distance(startPos, offTarget) / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            iconToRemove.anchoredPosition = Vector2.Lerp(startPos, offTarget, t);
            if (img) img.color = Color.Lerp(startColor, fadeColor, t);
            yield return null;
        }

        Destroy(iconToRemove.gameObject);
        currentOrder.RemoveAt(index);
        iconRects.RemoveAt(index);
        iconImages.RemoveAt(index);

        // Reposition remaining icons
        float iconSize = (orientation == Orientation.Vertical) ? iconToRemove.rect.height : iconToRemove.rect.width;

        for (int i = index; i < iconRects.Count; i++)
        {
            Vector2 newTarget;
            if (orientation == Orientation.Vertical)
                newTarget = new Vector2(marginX, -marginY - i * (iconSize + spacing));
            else
                newTarget = new Vector2(marginX + i * (iconSize + spacing), -marginY);

            StartCoroutine(MoveToPosition(iconRects[i], iconRects[i].anchoredPosition, newTarget));
        }

        yield return new WaitForSeconds(iconSize / moveSpeed);
        onComplete?.Invoke();
    }
}