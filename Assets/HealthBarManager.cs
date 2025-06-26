using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    public Image healthBar;
    public GameObject HundredContainer;
    public GameObject FiftyContainer;
    public GameObject bigLinePrefab;
    public GameObject smallLinePrefab;
    public GameObject PreviewObject;
    public GameObject HealPreviewObject;
    
    [SerializeField]
    private int maxHealth = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        maxHealth = GetComponent<Entity>().GetStat(Stats.Health).baseStatValue;
        var bigLineAmount = Mathf.FloorToInt(maxHealth / 100f);
        var smallLineAmount = Mathf.FloorToInt(maxHealth / 25f);

        var width = HundredContainer.GetComponent<RectTransform>().rect.width;
        
        for (int i = 1; i <= bigLineAmount; i++)
        {
            var indicator = Instantiate(bigLinePrefab, HundredContainer.transform);
            var indicatorWidth = indicator.GetComponent<RectTransform>().rect.width;
            var percentage = (i * 100f / maxHealth) * 100f;
            var xPos = (width / 100f * percentage) - (indicatorWidth/2);
            
            indicator.GetComponent<RectTransform>().anchoredPosition = new Vector3(xPos, 0, 0);
        }
        
        for (int i = 1; i <= smallLineAmount; i++)
        {
            var indicator = Instantiate(smallLinePrefab, FiftyContainer.transform);
            var indicatorWidth = indicator.GetComponent<RectTransform>().rect.width;
            var percentage = (i * 25f / maxHealth) * 100f;
            var xPos = (width / 100f * percentage) - (indicatorWidth/2);
            
            indicator.GetComponent<RectTransform>().anchoredPosition = new Vector3(xPos, 0, 0);
        }
    }
    
    //Updates the characters healthbar. 
    public void UpdateCharacterUI()
    {
        healthBar.fillAmount = (float) GetComponent<Entity>().GetStat(Stats.CurrentHealth).statValue / (float)GetComponent<Entity>().GetStat(Stats.Health).statValue;
    }

    public enum PreviewType { Damage, Heal }

    public void SetPreview(int value, bool isDamage)
    {
        var entity = GetComponent<Entity>();
        if (!entity) return;

        float currentHealth = entity.GetStat(Stats.CurrentHealth).statValue;
        float maxHealth = entity.GetStat(Stats.Health).statValue;

        float width = healthBar.GetComponent<RectTransform>().rect.width;
        float fillAmountPercentage = healthBar.fillAmount * 100f;
        float position = (width / 100f * fillAmountPercentage) - width;

        // Default clamped amount to preview
        float previewAmount = 0f;

        if (isDamage)
        {
            if (!PreviewObject) return;

            float damage = entity.CalculateDamage(value);
            previewAmount = Mathf.Min(damage, currentHealth); // clamp to current health

            PreviewObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(position, 0, 0);
            PreviewObject.GetComponent<Image>().fillAmount = previewAmount / maxHealth;
        }
        else
        {
            if (!HealPreviewObject) return;

            float missingHealth = maxHealth - currentHealth;
            float healing = Mathf.Min(value, missingHealth); // clamp to missing health
            previewAmount = healing;
            HealPreviewObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(healthBar.fillAmount * width, 0, 0);
            HealPreviewObject.GetComponent<Image>().fillAmount = previewAmount / maxHealth;
        }
    }


    public void HidePreview()
    {
        PreviewObject.GetComponent<Image>().fillAmount  = 0f;
        
        if (!HealPreviewObject) return;
            HealPreviewObject.GetComponent<Image>().fillAmount  = 0f;
    }
}
