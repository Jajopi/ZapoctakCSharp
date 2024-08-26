using System;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using GameEvents;

public class GameEventReceiverServer : MonoBehaviour
{
    [SerializeField]
    ObjectTypeDictionary objectTypes;
    Dictionary<string, GameObject> typeNameToObjectDictionary;

    NetworkServer networkServer;

    List<GameTaskObject> createdObjects = new List<GameTaskObject>();

    void Start()
    {
        networkServer = transform.GetComponent<NetworkServer>();
        typeNameToObjectDictionary = objectTypes.ToDictionary();
    }

    GameTaskObject GetObjectByID(int objectID)
    {
        //if (objectID < 0 || objectID >= createdObjects.Count) return null;
        return createdObjects[objectID];
    }

    void ConnectNewClient(Dictionary<string, string> actionAttributes)
    {
        networkServer.AddClient(new Uri(actionAttributes["ClientAddress"]));
    }

    /*void CreateNewGameTaskObject(Dictionary<string, string> creationAttributes)
    {
        string objectType = creationAttributes["ObjectType"];
        GameObject originalObject = typeNameToObjectDictionary[objectType];

        Vector3 position = GameEvent.ParseEventPosition(creationAttributes["ObjectPosition"]);
        Quaternion rotation = GameEvent.ParseEventRotation(creationAttributes["ObjectRotation"]);

        GameObject newObject = Instantiate(originalObject, position, rotation);
        newObject.GetComponent<GameTaskObject>().SetID(createdObjects.Count);
        createdObjects.Add(newObject.GetComponent<GameTaskObject>());
    }

    void PerformActionOnObject(Dictionary<string, string> actionAttributes)
    {
        GameTaskObject gameTaskObject = GetObjectByID(int.Parse(actionAttributes["ObjectID"]));
        gameTaskObject.PerformAction(actionAttributes);
    }

    void PerformEvent(GameEvent gameEvent)
    {
        if (gameEvent.Type == GameEvent.EventTypes.Connect)
        {
            ConnectNewPlayer(gameEvent.EventAttributes);
        }
        else if (gameEvent.Type == GameEvent.EventTypes.Create)
        {
            CreateNewGameTaskObject(gameEvent.EventAttributes);
        }
        else if (gameEvent.Type == GameEvent.EventTypes.Action)
        {
            PerformActionOnObject(gameEvent.EventAttributes);
        }
    }*/

    public void ReceiveEvents(DataToken receivedDataToken)
    {
        string tokenEvents = receivedDataToken.Value;

        foreach (string eventString in tokenEvents.Split('\n'))
        {
            if (eventString.Length == 0)
            {
                continue;
            }
            GameEvent gameEvent = new GameEvent(eventString);
            //PerformEvent(gameEvent);
            Debug.Log(gameEvent);
        }
    }
}
