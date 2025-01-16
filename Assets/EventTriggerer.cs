using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class EventTriggerer : MonoBehaviour
{
    public GameEvent GameEvent;

    public void Trigger()
    {
        GameEvent.Raise();
    }
}
