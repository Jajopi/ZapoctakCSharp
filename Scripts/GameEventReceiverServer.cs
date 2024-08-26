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

    void ConnectNewClient(GameEvent gameEvent)
    {
        networkServer.AddClient(new Uri(gameEvent.EventAttributes["ClientAddress"]));
    }

    void CreateNewGameTaskObject(GameEvent gameEvent)
    {
        Dictionary<string, string> creationAttributes = gameEvent.EventAttributes;

        string objectType = creationAttributes["ObjectType"];
        GameObject originalObject = typeNameToObjectDictionary[objectType];

        Vector3 position = GameEvent.ParseEventPosition(creationAttributes["ObjectPosition"]);
        Quaternion rotation = GameEvent.ParseEventRotation(creationAttributes["ObjectRotation"]);

        GameObject newObject = Instantiate(originalObject, position, rotation);
        int newObjectID = createdObjects.Count;
        newObject.GetComponent<GameTaskObject>().SetID(newObjectID);
        createdObjects.Add(newObject.GetComponent<GameTaskObject>());

        networkServer.SendEvent(new GameEvent(gameEvent.ToString() + $";ObjectID:{newObjectID}"));
    }

    void PerformActionOnObject(GameEvent gameEvent)
    {
        Dictionary<string, string> actionAttributes = gameEvent.EventAttributes;

        GameTaskObject gameTaskObject = GetObjectByID(int.Parse(actionAttributes["ObjectID"]));
        bool wasActionSuccesful = gameTaskObject.PerformAction(actionAttributes);
        if (wasActionSuccesful)
        {
            networkServer.SendEvent(gameEvent);
        }
    }

    void PerformEvent(GameEvent gameEvent)
    {
        if (gameEvent.Type == GameEvent.EventTypes.Connect)
        {
            ConnectNewClient(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventTypes.Create)
        {
            CreateNewGameTaskObject(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventTypes.Action)
        {
            PerformActionOnObject(gameEvent);
        }

        throw new NotImplementedException("Invalid GameEvent type encountered.");
    }

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
            PerformEvent(gameEvent);
            Debug.Log(gameEvent);
        }
    }
}
