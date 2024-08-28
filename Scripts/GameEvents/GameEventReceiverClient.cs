using System;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using GameEvents;

public class GameEventReceiverClient : GameEventReceiver
{
    NetworkClient networkClient;

    Dictionary<int, GameTaskObject> createdObjects = new Dictionary<int, GameTaskObject>();

    void Awake()
    {
        networkClient = transform.GetComponent<NetworkClient>();
        typeNameToObjectDictionary = GameObject.FindFirstObjectByType<ObjectTypeDictionaryHolder>().GetDictionary();
    }

    public override GameTaskObject GetObjectByID(int objectID)
    {
        return createdObjects[objectID];
    }

    void HandleConnection(GameEvent gameEvent)
    {
        networkClient.SetClientID(int.Parse(gameEvent.EventAttributes["ClientID"]));
        
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
        if (creationAttributes.ContainsKey("ControllerID"))
        {
            int controllerID = int.Parse(creationAttributes["ControllerID"]);
            newObject.GetComponent<GameTaskObject>().SetControllingPlayerID(controllerID);
        }

        createdObjects.Add(objectID, newObject.GetComponent<GameTaskObject>());

        if (creationAttributes.ContainsKey("ActionType"))
        {
            PerformActionOnObject(gameEvent);
        }
    }

    void PerformActionOnObject(GameEvent gameEvent)
    {
        Dictionary<string, string> actionAttributes = gameEvent.EventAttributes;

        GameTaskObject gameTaskObject = GetObjectByID(int.Parse(actionAttributes["ObjectID"]));
        gameTaskObject.PerformAction(actionAttributes);
    }

    protected override void PerformEvent(GameEvent gameEvent)
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
        else
        {
            throw new NotImplementedException("Invalid GameEvent type encountered.");
        }
    }
}
