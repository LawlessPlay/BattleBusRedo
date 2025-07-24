using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TacticsToolkit
{
    [ExecuteInEditMode()]
    public class CharacterTooltip : Tooltip
    {
        public int MaxHealth = 100;
        public int CurrentHealth = 100;
        
        public Image HealthSprite;
        public Sprite AllyBackground;
        public Sprite EnemyBackGround;
        public Image BackGround;
        public TMP_Text HealthText;
        public TMP_Text EffectDescriptionText;

        public void SetContent(Entity character, Vector3 position, Vector2 dimensions)
        {
            SetSprites(character);
            SetHealthBar(character);
            SetEffectDescriptions(character);

            this.title.text = character.name;
            int titleLenght = this.title.text.Length;
            layoutElement.enabled = titleLenght > characterLimit;

            if (!isFixedPosition)
            {
                StartCoroutine(MoveTooltip(position, dimensions));
            }
        }

        private void SetSprites(Entity character)
        {
            this.image.sprite = character.portrait;
            

            if (character.teamID == Entity.TeamType.Player)
            {
                BackGround.sprite = AllyBackground;
            }
            else
            {
                BackGround.sprite = EnemyBackGround;
            }
        }

        private void SetHealthBar(Entity character)
        {
            MaxHealth = character.GetStat(Stats.Health).statValue;
            CurrentHealth = character.GetStat(Stats.CurrentHealth).statValue;

            HealthSprite.fillAmount = (float)CurrentHealth / (float)MaxHealth;
            HealthText.text = CurrentHealth + "/" + MaxHealth;
        }

        private void SetEffectDescriptions(Entity character)
        {
            var statMods = character.GetStatModifiers();

            if (statMods.Count > 0)
            {
                EffectDescriptionText.text = statMods.First().description;

                for (int i = 1; i < statMods.Count; i++)
                {
                    EffectDescriptionText.text += ", " + statMods[i].description;
                }
            }
            else
            {
                EffectDescriptionText.text = "";
            }
        }


        public void ResetContent()
        {
            this.title.text = "";
            this.CurrentHealth = MaxHealth;
        }

    }
}
