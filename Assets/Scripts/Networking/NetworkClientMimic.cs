using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using GameEvents;
using Networking;

public class NetworkClientMimic : NetworkClient
{
    GameEventReceiverServer mimicEventReceiver;

    void Awake()
    {
        mimicEventReceiver = GameObject.FindFirstObjectByType<GameEventReceiverServer>();
    }

    public override int GetClientID()
    {
        return 0;
    }

    void Update()
    {
        return;
    }

    public override void SendEvent(GameEvent gameEvent)
    {
        mimicEventReceiver.ReceiveEvents(new DataToken(gameEvent.ToString()));
    }
}
