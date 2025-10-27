using UnityEngine;
using UnityEngine.Events;

namespace TacticsToolkit
{
    public class GameEventBoolListener : GameEventListener<bool>
    {
        [SerializeField] private GameEventBool eventGameObject = null;
        [SerializeField] private UnityEvent<bool> response = null;

        public override GameEvent<bool> Event => eventGameObject;
        public override UnityEvent<bool> Response => response;
    }
}
