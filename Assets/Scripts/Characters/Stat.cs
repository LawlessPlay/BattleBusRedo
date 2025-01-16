using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacticsToolkit
{
    //At attribute for a entity. 
    [Serializable]
    public class Stat
    {
        public Stats statKey;
        public Entity character;

        //BaseStatValue is the unedited value. 
        public int baseStatValue;
        public float baseStatModifier;

        //StatValue is the BaseStatValue that can be changed by effects/damage. 
        public int statValue;
        public bool isModified;
        public List<StatModifier> statMods;

        public Stat(Stats statKey, int statValue, Entity character)
        {
            this.character = character;
            this.statValue = statValue;
            this.statKey = statKey;

            baseStatValue = statValue;
            statMods = new List<StatModifier>();
            isModified = false;
        }

        //Update the value of a stat. 
        public void ChangeStatValue(int newValue)
        {
            statValue = newValue;
            baseStatValue = newValue;
        }

        //Apply a modifier to a stat. Change stat value. 
        public void ApplyStatMods()
        {
            if(statKey != Stats.CurrentHealth && statKey != Stats.CurrentMana) {
                statValue = baseStatValue;
            }

            foreach (var statMod in statMods)
            {
                if (statMod != null)
                {
                    ModifyStat(statMod);
                }
            }
        }

        public void TickStatMods()
        {
            foreach (var statMod in statMods)
            {
                if (statMod != null)
                {

                    statMod.duration--;
                }

            }
            statMods.RemoveAll(x => x.duration <= 0);

            if (statMods.Count == 0)
            {
                isModified = false;
            }
        }

        private void ModifyStat(StatModifier statMod)
        {
            switch (statMod.Operator)
            {
                case Operation.Add:
                    statValue = Mathf.CeilToInt(statValue + statMod.value);
                    break;
                case Operation.Minus:
                    if (statKey == Stats.CurrentHealth)
                    {
                        character.TakeDamage(Mathf.CeilToInt(statMod.value));
                        ChangeStatValue(character.statsContainer.CurrentHealth.statValue);
                    }
                    else
                    {
                        statValue = Mathf.CeilToInt(statValue - statMod.value);
                    }
                    break;
                case Operation.Multiply:
                    statValue = Mathf.CeilToInt(statValue * statMod.value);
                    break;
                case Operation.Divide:
                    statValue = Mathf.CeilToInt(statValue / statMod.value);
                    break;
                case Operation.AddByPercentage:
                    statValue = Mathf.CeilToInt(statValue * (1 + statMod.value / 100));
                    break;
                case Operation.MinusByPercentage:
                    if (statKey == Stats.CurrentHealth)
                    {
                        float percentageDifference = (float)(statMod.value / 100f) * (float)baseStatValue;
                        character.TakeDamage(Mathf.CeilToInt(percentageDifference), true);
                        ChangeStatValue(character.statsContainer.CurrentHealth.statValue);
                    }
                    else
                    {
                        statValue = Mathf.CeilToInt(statValue * (1 - statMod.value / 100));
                    }
                    break;
            }
        }

        public void ApplySingleStatMod(StatModifier statMod)
        {
            ModifyStat(statMod);
        }

        public void UndoStatMod(StatModifier statMod)
        {
            switch (statMod.Operator)
            {
                case Operation.Add:
                    statValue = Mathf.CeilToInt(statValue - statMod.value);
                    break;
                case Operation.Minus:
                    statValue = Mathf.CeilToInt(statValue + statMod.value);
                    break;
                case Operation.Multiply:
                    if (statMod.value != 0)
                    {
                        statValue = Mathf.CeilToInt(statValue / statMod.value);
                    }
                    break;
                case Operation.Divide:
                    if (statMod.value != 0)
                    {
                        statValue = Mathf.CeilToInt(statValue * statMod.value);
                    }
                    break;
                case Operation.AddByPercentage:
                    if (statMod.value != 0)
                    {
                        statValue = Mathf.CeilToInt(statValue / (1 + statMod.value / 100));
                    }
                    break;
                case Operation.MinusByPercentage:
                    if (statMod.value != 0)
                    {
                        statValue = Mathf.CeilToInt(statValue / (1 - statMod.value / 100));
                    }
                    break;
            }

        }
    }
}