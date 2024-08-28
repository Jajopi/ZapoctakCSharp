using System;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using GameEvents;

public class GameEventReceiver : MonoBehaviour
{
    protected Dictionary<string, GameObject> typeNameToObjectDictionary;

    public virtual GameTaskObject GetObjectByID(int objectID)
    {
        throw new NotImplementedException();
    }

    protected virtual void PerformEvent(GameEvent gameEvent)
    {
        throw new NotImplementedException();
    }

    public void ReceiveEvents(DataToken receivedDataToken)
    {
        string tokenEvents = receivedDataToken.Value;

        foreach (string eventString in tokenEvents.Split('\n'))
        {
            if (eventString.Trim().Length == 0)
            {
                continue;
            }
            GameEvent gameEvent = new GameEvent(eventString);
            PerformEvent(gameEvent);
        }
    }
}
