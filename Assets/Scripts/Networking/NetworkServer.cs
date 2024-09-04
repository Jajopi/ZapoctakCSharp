using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using GameEvents;
using Networking;

public class NetworkServer : MonoBehaviour
{
    INetworkReceiver networkReceiver;
    INetworkSender networkSender;

    GameEventReceiverServer eventReceiver;

    List<Uri> clients = new List<Uri>();
    StringBuilder sentDataLog = new StringBuilder();

    void Awake()
    {
        eventReceiver = transform.GetComponent<GameEventReceiverServer>();

        StartServer();
    }

    void StartServer()
    {
        int freePort = NetworkHelperFunctions.GetRandomFreePort();
        Uri receivingAddress = NetworkHelperFunctions.GetReceivingAddress(freePort);

        networkReceiver = new SimpleReceiver(receivingAddress);
        networkReceiver.StartReceiving();

        networkSender = new SimpleSender();
    }

    void Update()
    {
        while (networkReceiver.IsDataReady())
        {
            DataToken data = networkReceiver.GetNextReceivedData();
            eventReceiver.ReceiveEvents(data);
        }
    }

    public int AddClient(Uri clientAddress)
    {
        clients.Add(clientAddress);

        int newClientID = clients.Count;
        networkSender.SendData(new DataToken($"Connect;ClientID:{newClientID}"), clientAddress);

        string data = sentDataLog.ToString();
        if (data.Length > 0)
        {
            networkSender.SendData(new DataToken(data), clientAddress);
        }
        return newClientID;
    }

    void SaveEventIntoLog(GameEvent gameEvent)
    {
        sentDataLog.AppendLine(gameEvent.ToString());
    }

    void SendDataToAllClients(DataToken data)
    {
        foreach (Uri client in clients)
        {
            if (client is null)
            {
                continue;
            }

            networkSender.SendData(data, client);
        }
    }

    public void SendEvent(GameEvent gameEvent)
    {
        if (!gameEvent.IsTemporary)
        {
            SaveEventIntoLog(gameEvent);
        }

        SendDataToAllClients(new DataToken(gameEvent.ToString()));
    }

    public Uri GetReceivingAddress()
    {
        return networkReceiver.GetReceivingAddress();
    }

    public void RemoveClient(int clientID)
    {
        if (clientID == 0)
        {
            return;
        }

        clients[clientID - 1] = null;
    }

    void OnDestroy()
    {
        if (networkReceiver != null)
        {
            networkReceiver.EndReceiving();
        }
    }
}
