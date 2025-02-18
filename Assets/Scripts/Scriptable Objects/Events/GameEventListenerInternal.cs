// ----------------------------------------------------------------------------
// Based on Ryan Hipple's (Schell Games) talk
// @ Unite 2017 - Game Architecture with Scriptable Objects
// https://github.com/roboryantron/Unite2017 (MIT)
// ----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;

namespace TacticsToolkit
{
    public class GameEventListenerInternal
    {
        private GameEvent gameEvent;
        private UnityEvent response;

        public void RegisterEvent(GameEvent gameEvent, UnityEvent response)
        {
            if (gameEvent != null)
            {
                this.gameEvent = gameEvent;
                this.response = response;

                gameEvent.RegisterListener(this);
            }
        }

        public void UnregisterEvent()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }

        public void OnEventRaised()
        {
            if (response != null)
            {
                response.Invoke();
            }
        }
    }

    public class GameEventListenerInternal<T>
    {
        private GameEvent<T> gameEvent;
        private UnityEvent<T> response;

        public void RegisterEvent(GameEvent<T> gameEvent, UnityEvent<T> response)
        {
            if (gameEvent != null)
            {
                this.gameEvent = gameEvent;
                this.response = response;

                gameEvent.RegisterListener(this);
            }
        }

        public void UnregisterEvent()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }

        public void OnEventRaised(T param)
        {
            if (response != null)
            {
                response.Invoke(param);
            }
        }
    }
}