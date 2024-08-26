using System;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using GameEvents;

public class GameEventReceiverClient : MonoBehaviour
{
    [SerializeField]
    ObjectTypeDictionary objectTypes;
    Dictionary<string, GameObject> typeNameToObjectDictionary;

    NetworkClient networkClient;

    Dictionary<int, GameTaskObject> createdObjects = new Dictionary<int, GameTaskObject>();

    void Start()
    {
        networkClient = transform.GetComponent<NetworkClient>();
        typeNameToObjectDictionary = objectTypes.ToDictionary();
    }

    GameTaskObject GetObjectByID(int objectID)
    {
        return createdObjects[objectID];
    }

    void HandleConnection(GameEvent gameEvent)
    {
        Debug.Log("Connected");
        GameObject.FindFirstObjectByType<ClientGameUI>().DestroyWaitingObject();
    }

    void CreateNewGameTaskObject(GameEvent gameEvent)
    {
        Dictionary<string, string> creationAttributes = gameEvent.EventAttributes;

        string objectType = creationAttributes["ObjectType"];
        GameObject originalObject = typeNameToObjectDictionary[objectType];

        Vector3 position = GameEvent.ParseEventPosition(creationAttributes["ObjectPosition"]);
        Quaternion rotation = GameEvent.ParseEventRotation(creationAttributes["ObjectRotation"]);
        GameObject newObject = Instantiate(originalObject, position, rotation);

        int objectID = int.Parse(creationAttributes["ObjectID"]);
        newObject.GetComponent<GameTaskObject>().SetID(objectID);

        createdObjects.Add(objectID, newObject.GetComponent<GameTaskObject>());
    }

    void PerformActionOnObject(GameEvent gameEvent)
    {
        Dictionary<string, string> actionAttributes = gameEvent.EventAttributes;

        GameTaskObject gameTaskObject = GetObjectByID(int.Parse(actionAttributes["ObjectID"]));
        gameTaskObject.PerformAction(actionAttributes);
    }

    void PerformEvent(GameEvent gameEvent)
    {
        if (gameEvent.Type == GameEvent.EventTypes.Connect)
        {
            HandleConnection(gameEvent);
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
            GameEvent gameEvent = new GameEvent(eventString);
            PerformEvent(gameEvent);
        }
    }
}
