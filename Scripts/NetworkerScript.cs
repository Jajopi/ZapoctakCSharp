using UnityEngine;
using Networking;
using System;

public class NetworkerScript : MonoBehaviour
{
    float elapsedTime;
    int counter;

    SimpleReceiver networkReceiver;
    SimpleSender networkSender;

    void Start()
    {
        elapsedTime = 3;
        counter = 1;

        networkReceiver = new SimpleReceiver("http://localhost:8001/", true);
        networkReceiver.StartReceiving();

        networkSender = new SimpleSender();
        networkSender.SetTarget(new Uri("http://localhost:8001/"));
        
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 2)
        {
            elapsedTime -= 2;

            if (counter < 10)
            {
                networkSender.SendData(counter.ToString());
                counter++;
                networkSender.SendData(counter.ToString());
                counter++;
                networkSender.SendData(counter.ToString());
                counter++;
            }
        }

        if (networkReceiver.IsDataReady())
        {
            ReceivedDataToken data = networkReceiver.GetNextReceivedData();
            Debug.Log(data);
        }
    }

    void OnApplicationQuit()
    {
        if (networkReceiver != null)
        {
            Debug.Log("Quitting.");
            networkReceiver.EndReceiving();
        }
    }
}
