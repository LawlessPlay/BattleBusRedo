using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject HundredContainer;
    [SerializeField] private GameObject FiftyContainer;
    [SerializeField] private GameObject bigLinePrefab;
    [SerializeField] private GameObject smallLinePrefab;
    [SerializeField] private GameObject PreviewObject;      // Damage preview overlay
    [SerializeField] private GameObject HealPreviewObject;  // Heal preview overlay

    [Header("Values (read-only in Inspector)")]
    [SerializeField] private int maxHealth = 0;
    [SerializeField] private int currentHealth = 0;

    // -------- Setters (push data in; no external deps) --------
    public void SetUIRefs(
        Image healthBarImage,
        GameObject hundredContainer,
        GameObject fiftyContainer,
        GameObject bigTickPrefab,
        GameObject smallTickPrefab,
        GameObject damagePreviewObj,
        GameObject healPreviewObj)
    {
        healthBar         = healthBarImage;
        HundredContainer  = hundredContainer;
        FiftyContainer    = fiftyContainer;
        bigLinePrefab     = bigTickPrefab;
        smallLinePrefab   = smallTickPrefab;
        PreviewObject     = damagePreviewObj;
        HealPreviewObject = healPreviewObj;
    }

    public void SetHealth(int current, int max)
    {
        maxHealth = Mathf.Max(0, max);
        currentHealth = Mathf.Clamp(current, 0, maxHealth);
        UpdateCharacterUI();
        RebuildTickMarks();
    }

    public void SetMaxHealth(int max)
    {
        SetHealth(Mathf.Min(currentHealth, Mathf.Max(0, max)), max);
    }

    public void SetCurrentHealth(int current)
    {
        currentHealth = Mathf.Clamp(current, 0, maxHealth);
        UpdateCharacterUI();
    }

    // -------- UI Updates --------
    public void UpdateCharacterUI()
    {
        if (!healthBar || maxHealth <= 0) return;
        healthBar.fillAmount = maxHealth == 0 ? 0f : (float)currentHealth / maxHealth;
    }

    public void RebuildTickMarks()
    {
        if (maxHealth <= 0) return;
        if (!HundredContainer || !FiftyContainer || !healthBar) return;

        // Clear old children (inline, no helper)
        for (int i = HundredContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(HundredContainer.transform.GetChild(i).gameObject);
        for (int i = FiftyContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(FiftyContainer.transform.GetChild(i).gameObject);

        int bigLineAmount   = Mathf.FloorToInt(maxHealth / 100f);
        int smallLineAmount = Mathf.FloorToInt(maxHealth / 25f);

        var hundredRT = HundredContainer.GetComponent<RectTransform>();
        var fiftyRT   = FiftyContainer.GetComponent<RectTransform>();
        if (!hundredRT || !fiftyRT) return;

        float width = hundredRT.rect.width;

        // Big ticks (every 100)
        for (int i = 1; i <= bigLineAmount; i++)
        {
            if (!bigLinePrefab) break;
            var indicator = Instantiate(bigLinePrefab, HundredContainer.transform);
            var indRT = indicator.GetComponent<RectTransform>();
            float indicatorWidth = indRT ? indRT.rect.width : 0f;
            float percentage = (i * 100f / maxHealth) * 100f; // percent of bar
            float xPos = (width / 100f * percentage) - (indicatorWidth / 2f);
            if (indRT) indRT.anchoredPosition = new Vector2(xPos, 0f);
        }

        // Small ticks (every 25)
        for (int i = 1; i <= smallLineAmount; i++)
        {
            if (!smallLinePrefab) break;
            var indicator = Instantiate(smallLinePrefab, FiftyContainer.transform);
            var indRT = indicator.GetComponent<RectTransform>();
            float indicatorWidth = indRT ? indRT.rect.width : 0f;
            float percentage = (i * 25f / maxHealth) * 100f;
            float xPos = (width / 100f * percentage) - (indicatorWidth / 2f);
            if (indRT) indRT.anchoredPosition = new Vector2(xPos, 0f);
        }
    }

    // -------- Previews --------
    public enum PreviewType { Damage, Heal }

    /// <summary>
    /// Show a preview of incoming damage/heal.
    /// For damage: value is final damage.
    /// For heal:   value is raw heal amount.
    /// </summary>
    public void SetPreview(int value, bool isDamage)
    {
        if (!healthBar || maxHealth <= 0) return;

        var barRT = healthBar.GetComponent<RectTransform>();
        if (!barRT) return;

        float width = barRT.rect.width;
        if (width <= 0f) return;

        float fillAmountPercentage = healthBar.fillAmount * 100f;

        if (isDamage)
        {
            if (!PreviewObject) return;

            float damage = Mathf.Max(0f, value);
            float clamped = Mathf.Min(damage, currentHealth);

            var prevRT = PreviewObject.GetComponent<RectTransform>();
            var img = PreviewObject.GetComponent<Image>();
            if (!prevRT || !img) return;

            // Preserve original position math
            float xPos = (width / 100f * fillAmountPercentage) - width;
            prevRT.anchoredPosition = new Vector2(xPos, 0f);
            img.fillAmount = clamped / maxHealth;
        }
        else
        {
            if (!HealPreviewObject) return;

            float missing = Mathf.Max(0f, maxHealth - currentHealth);
            float clamped = Mathf.Clamp(value, 0f, missing);

            var healRT = HealPreviewObject.GetComponent<RectTransform>();
            var img = HealPreviewObject.GetComponent<Image>();
            if (!healRT || !img) return;

            float xPos = healthBar.fillAmount * width;
            healRT.anchoredPosition = new Vector2(xPos, 0f);
            img.fillAmount = clamped / maxHealth;
        }
    }

    public void HidePreview()
    {
        if (PreviewObject)
        {
            var img = PreviewObject.GetComponent<Image>();
            if (img) img.fillAmount = 0f;
        }

        if (HealPreviewObject)
        {
            var img = HealPreviewObject.GetComponent<Image>();
            if (img) img.fillAmount = 0f;
        }
    }
}
