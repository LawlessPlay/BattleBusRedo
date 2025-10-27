using System;
using UnityEngine;

namespace TacticsToolkit
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameEventBool", menuName = "GameEvents/GameEventBool", order = 3)]
    public class GameEventBool : GameEvent<bool>
    {
        public bool value;
    }
}
