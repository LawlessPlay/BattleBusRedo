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

        [SerializeField]
        private Tooltip spellTooltip;
        [SerializeField]
        private CharacterTooltip targetTooltip;
        public TileTooltip tileTooltip;
        public float fadeOnTime = 1f;
        public float waitTime = 1f;

        public void Awake()
        {
            instance = this;
        }

        public void ShowSpellTooltip(Sprite image, string title, string description, Vector3 position, Vector2 dimensions) => instance.StartCoroutine(instance.ShowSpellTooltipOverTime(image, title, description, position, dimensions, spellTooltip));

        public void ShowTargetTooltip(Entity character, Vector3 position, Vector2 dimensions) => instance.StartCoroutine(instance.ShowTargetTooltipOverTime(character, position, dimensions, targetTooltip));

        public void ShowTileTooptip(OverlayTile tile)
        {
            tileTooltip.SetContent(tile);
            StartCoroutine(ShowTooltipOverTime(tileTooltip.gameObject));
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
                //instance.targetTooltip.GetComponent<CanvasGroup>().alpha = 0;
            }
        }
    }
}
