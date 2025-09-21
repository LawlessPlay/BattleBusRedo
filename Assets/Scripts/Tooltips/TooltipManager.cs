using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TacticsToolkit
{
    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager instance;

        public Tooltip spellTooltip;
        public CharacterTooltip targetTooltip;
        public TileTooltip tileTooltip;
        public float fadeOnTime = 1f;
        public float waitTime = 1f;

        public void Awake()
        {
            instance = this;
        }

        public void ShowSpellTooltip(Sprite image, string title, string description, Vector3 position, Vector2 dimensions)
        {
            spellTooltip.SetContent(image, title, description, position, dimensions);
            Show(spellTooltip.gameObject, -waitTime);
        }

        public void ShowTargetTooltip(Entity character, Vector3 position, Vector2 dimensions)
        {
            targetTooltip.SetContent(character, position, dimensions);
            Show(targetTooltip.gameObject);
        }

        public void ShowTileTooptip(OverlayTile tile)
        {
            tileTooltip.SetContent(tile);
            Show(tileTooltip.gameObject);
        }

        private Coroutine hideRoutine;
        private Coroutine showRoutine;

// Optional: a generation counter to invalidate stale Show coroutines
private int showGen = 0;

public void Show(GameObject tooltip, float delay = 0f)
{
    var canvasGroup = tooltip.GetComponent<CanvasGroup>();
    if (!canvasGroup) return;

    // Cancel any previous pending show (for other tiles or older hovers)
    if (showRoutine != null)
    {
        StopCoroutine(showRoutine);
        showRoutine = null;
    }

    // New generation token for this Show call
    int myGen = ++showGen;
    showRoutine = StartCoroutine(DelayedShowAfterWait(canvasGroup, myGen, delay));
}

private IEnumerator DelayedShowAfterWait(CanvasGroup canvasGroup, int myGen, float delay = 0f)
{
    var waitToUse = waitTime + delay;
    // Wait first; do not interrupt any ongoing hide yet
    if (waitToUse > 0f)
        yield return new WaitForSecondsRealtime(waitTime);

    // If a newer Show started while we were waiting, abort
    if (myGen != showGen)
        yield break;

    // Now that we're actually going to show, stop any ongoing hide
    if (hideRoutine != null)
    {
        StopCoroutine(hideRoutine);
        hideRoutine = null;
    }

    // Fade from current alpha -> 1
    yield return FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f);

    showRoutine = null;
}

public void Hide(GameObject tooltip)
{
    var canvasGroup = tooltip.GetComponent<CanvasGroup>();
    if (!canvasGroup) return;

    // Cancel any pending show immediately (moving off or to a new tile)
    if (showRoutine != null)
    {
        StopCoroutine(showRoutine);
        showRoutine = null;
        // Optionally bump gen to invalidate any stray coroutines
        // ++showGen;
    }

    // If already fully hidden, skip
    if (canvasGroup.alpha <= 0f) return;

    // Stop previous hide and start a fresh one from current alpha -> 0
    if (hideRoutine != null)
    {
        StopCoroutine(hideRoutine);
        hideRoutine = null;
    }

    hideRoutine = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f));
}

private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
{
    if (Mathf.Approximately(startAlpha, endAlpha))
    {
        canvasGroup.alpha = endAlpha;
        yield break;
    }

    if (fadeOnTime <= 0f)
    {
        canvasGroup.alpha = endAlpha;
        yield break;
    }

    float elapsed = 0f;
    while (elapsed < fadeOnTime)
    {
        float t = Mathf.Clamp01(elapsed / fadeOnTime);
        canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
        elapsed += Time.unscaledDeltaTime;
        yield return null;
    }
    canvasGroup.alpha = endAlpha;
}


        
        private IEnumerator ShowTooltipOverTime(GameObject tooltip)
        {
            if (instance)
            {
                yield return new WaitForSeconds(waitTime);

                tooltip.gameObject.SetActive(true);

                yield return new WaitForEndOfFrame();
                // Get all the child components with Image or Text components
                var children = tooltip.GetComponentsInChildren<Component>()
                    .Where(c => c is Image || c is Text)
                    .Select(c => (c as Graphic))
                    .ToList();

                // Initialize alpha values for all the child components
                var startAlphas = children.Select(c => c.color.a).ToList();
                var endAlphas = Enumerable.Repeat(1f, children.Count).ToList();

                // Initialize start time and elapsed time
                float startTime = Time.time;
                float elapsedTime = 0f;

                // Fade in all the child components over the specified duration
                while (elapsedTime < fadeOnTime)
                {
                    float t = elapsedTime / fadeOnTime;
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];
                        var startAlpha = startAlphas[i];
                        var endAlpha = endAlphas[i];

                        // Interpolate the alpha value for this child component
                        var alpha = Mathf.Lerp(startAlpha, endAlpha, t);

                        // Set the alpha value for this child component
                        var color = child.color;
                        color.a = alpha;
                        child.color = color;
                    }
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Set final alpha values for all the child components
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    var endAlpha = endAlphas[i];

                    var color = child.color;
                    color.a = 1;
                    child.color = color;
                }
            }
        }
        
        private IEnumerator ShowTargetTooltipOverTime(Entity character, Vector3 position, Vector2 dimensions, CharacterTooltip tooltip)
        {
            if (instance && !tooltip.isActiveAndEnabled)
            {
                // Optional initial delay before showing
                if (waitTime > 0f)
                    yield return new WaitForSecondsRealtime(waitTime);

                // Ensure CanvasGroup exists on the tooltip root
                var cg = tooltip.GetComponent<CanvasGroup>();
                if (cg == null) cg = tooltip.gameObject.AddComponent<CanvasGroup>();

                // Prepare
                tooltip.gameObject.SetActive(true);
                tooltip.SetContent(character, position, dimensions);

                // Wait a frame so layout/content settle
                yield return new WaitForEndOfFrame();

                // Prime for fade-in
                float startAlpha = 0f;
                float endAlpha   = 1f;
                float elapsed    = 0f;

                // Temporarily disable interaction while fading in (optional)
                bool prevInteractable   = cg.interactable;
                bool prevBlocksRaycasts = cg.blocksRaycasts;
                cg.alpha = startAlpha;
                cg.interactable = false;
                cg.blocksRaycasts = false;

                // Fade
                if (fadeOnTime <= 0f)
                {
                    cg.alpha = endAlpha;
                }
                else
                {
                    while (elapsed < fadeOnTime)
                    {
                        float t = Mathf.Clamp01(elapsed / fadeOnTime);
                        cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                        elapsed += Time.unscaledDeltaTime; // unscaled for UI
                        yield return null;
                    }
                    cg.alpha = endAlpha;
                }

                // Restore interaction
                cg.interactable = prevInteractable;
                cg.blocksRaycasts = prevBlocksRaycasts;
            }
        }


        

        private IEnumerator ShowSpellTooltipOverTime(Sprite image, string title, string description, Vector3 position, Vector2 dimensions, Tooltip tooltip)
        {
            if (instance && !tooltip.isActiveAndEnabled)
            {
                yield return new WaitForSeconds(waitTime);

                tooltip.gameObject.SetActive(true);
                tooltip.SetContent(image, title, description, position, dimensions);

                yield return new WaitForEndOfFrame();
                // Get all the child components with Image or Text components
                var children = tooltip.GetComponentsInChildren<Component>()
                    .Where(c => c is Image || c is Text)
                    .Select(c => (c as Graphic))
                    .ToList();

                // Initialize alpha values for all the child components
                var startAlphas = children.Select(c => c.color.a).ToList();
                var endAlphas = Enumerable.Repeat(1f, children.Count).ToList();

                // Initialize start time and elapsed time
                float startTime = Time.time;
                float elapsedTime = 0f;

                // Fade in all the child components over the specified duration
                while (elapsedTime < fadeOnTime)
                {
                    float t = elapsedTime / fadeOnTime;
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];
                        var startAlpha = startAlphas[i];
                        var endAlpha = endAlphas[i];

                        // Interpolate the alpha value for this child component
                        var alpha = Mathf.Lerp(startAlpha, endAlpha, t);

                        // Set the alpha value for this child component
                        var color = child.color;
                        color.a = alpha;
                        child.color = color;
                    }
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Set final alpha values for all the child components
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    var endAlpha = endAlphas[i];

                    var color = child.color;
                    color.a = 1;
                    child.color = color;
                }
            }
        }
        public static void Hide()
        {
            if (instance)
            {
                instance.StopAllCoroutines();
                instance.spellTooltip.ResetContent();
                instance.spellTooltip.gameObject.SetActive(false);
            }
        }
    }
}
