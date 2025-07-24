using System.Collections.Generic;
using UnityEngine;


namespace TacticsToolkit
{
    [CreateAssetMenu(fileName = "CharacterDescription", menuName = "ScriptableObjects/CharacterDescription", order = 1)]
    public class CharacterDescription : ScriptableObject
    {
        public string Name;
        public Sprite Icon;
    }
}
