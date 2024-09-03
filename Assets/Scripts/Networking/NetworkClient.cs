using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using GameEvents;
using Networking;

public class NetworkClient : MonoBehaviour
{
    INetworkReceiver networkReceiver;
    INetworkSender networkSender;

    GameEventReceiverClient eventReceiver;

    int clientID = -1;

    void Awake()
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
        Uri targetAddress = GetTargetAddress();
        networkSender.SetTarget(targetAddress);

        GameEvent connectionEvent = new GameEvent($"Connect;ClientAddress:{receivingAddress}");
        networkSender.SendData(new DataToken(connectionEvent.ToString()));
    }

    Uri GetTargetAddress()
    {
        if (PlayerPrefs.HasKey("ServerAddress"))
        {
            return NetworkHelperFunctions.GetUriFromAddress(PlayerPrefs.GetString("ServerAddress"));
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

    public virtual void SendEvent(GameEvent gameEvent)
    {
        networkSender.SendData(new DataToken(gameEvent.ToString()));
    }

    public virtual int GetClientID()
    {
        return clientID;
    }

    public void SetClientID(int newClientID)
    {
        clientID = newClientID;
    }
}
