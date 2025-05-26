using System.Collections.Generic;
using UnityEngine;

namespace TacticsToolkit
{
    //The ability object
    [CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 1)]
    public class Ability : ScriptableObject
    {
        [Header("General Stuff")]
        public string Name;

        public string Desc;

        [Header("Ability Stuff")]
        public TextAsset abilityShape;

        public List<ScriptableEffect> effects;

        public GameObject abilityFX;

        public int range;

        public int cooldown;

        public int cost;

        public int value;

        public AbilityTypes abilityType;

        public bool includeOrigin;

        public int requiredLevel;

        public bool requiresTarget;
        
        public TooltipSO tooltip;

        public enum AbilityTypes
        {
            Heal,
            Damage,
            All
        }

        //TODO damage types
    }
}