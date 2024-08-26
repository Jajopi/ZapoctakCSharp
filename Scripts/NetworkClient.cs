using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using GameEvents;
using Networking;

//string dataString = "Create;ObjectType:Player;ObjectPosition:0~10~0;ObjectRotation:0~0~0~0";

public class NetworkClient : MonoBehaviour
{
    INetworkReceiver networkReceiver;
    INetworkSender networkSender;

    GameEventReceiverClient eventReceiver;

    void Start()
    {
        eventReceiver = transform.GetComponent<GameEventReceiverClient>();

        StartClient();
    }

    void StartClient()
    {
        int freePort = NetworkHelperFunctions.GetRandomFreePort();
        Uri receivingAddress = NetworkHelperFunctions.GetReceivingAddress(freePort);

        networkReceiver = new SimpleReceiver(receivingAddress, false);
        networkReceiver.StartReceiving();

        networkSender = new SimpleSender();
        string targetAddress = GetTargetAddress();
        networkSender.SetTarget(new Uri(targetAddress));

        GameEvent connectionEvent = new GameEvent($"Connect;ClientAddress:{receivingAddress}");
        networkSender.SendData(new DataToken(connectionEvent.ToString()));
    }

    string GetTargetAddress()
    {
        if (PlayerPrefs.HasKey("ServerAddress"))
        {
            return PlayerPrefs.GetString("ServerAddress");
        }
        throw new InvalidOperationException("ServerAddress is missing from PlayerPrefs");
    }

    void Update()
    {
        while (networkReceiver.IsDataReady())
        {
            DataToken data = networkReceiver.GetNextReceivedData();
            eventReceiver.ReceiveEvents(data);
        }
    }

    void OnApplicationQuit()
    {
        if (networkReceiver != null)
        {
            networkReceiver.EndReceiving();
        }
    }

    public void SendEvent(GameEvent gameEvent)
    {
        networkSender.SendData(new DataToken(gameEvent.ToString()));
    }
}
