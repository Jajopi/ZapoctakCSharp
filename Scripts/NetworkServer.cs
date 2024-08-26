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

    void Start()
    {
        eventReceiver = transform.GetComponent<GameEventReceiverServer>();

        StartServer();
    }

    void StartServer()
    {
        int freePort = NetworkHelperFunctions.GetRandomFreePort();
        Uri receivingAddress = NetworkHelperFunctions.GetReceivingAddress(freePort);

        networkReceiver = new SimpleReceiver(receivingAddress, false);
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

    void OnApplicationQuit()
    {
        if (networkReceiver != null)
        {
            networkReceiver.EndReceiving();
        }
    }

    public void AddClient(Uri clientAddress)
    {
        clients.Add(clientAddress);

        string data = sentDataLog.ToString();
        if (data.Length > 0)
        {
            networkSender.SendData(new DataToken(data), clientAddress);
        }
    }

    void SaveEventIntoLog(GameEvent gameEvent)
    {
        sentDataLog.AppendLine(gameEvent.ToString());
    }

    void SendData(DataToken data)
    {
        foreach (Uri client in clients)
        {
            networkSender.SendData(data, client);
        }
    }

    public void SendEvent(GameEvent gameEvent)
    {
        if (!gameEvent.IsTemporary)
        {
            SaveEventIntoLog(gameEvent);
        }

        SendData(new DataToken(gameEvent.ToString()));
    }
}
