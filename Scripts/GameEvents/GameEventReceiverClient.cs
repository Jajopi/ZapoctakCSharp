using System;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using GameEvents;

public class GameEventReceiverClient : GameEventReceiver
{
    NetworkClient networkClient;
    MenuController menuController;

    Dictionary<int, GameTaskObject> createdObjects = new Dictionary<int, GameTaskObject>();

    void Awake()
    {
        networkClient = transform.GetComponent<NetworkClient>();
        menuController = GameObject.FindFirstObjectByType<MenuController>();
        typeNameToObjectDictionary = GameObject.FindFirstObjectByType<ObjectTypeDictionaryHolder>().GetDictionary();
    }

    public override GameTaskObject GetObjectByID(int objectID)
    {
        return createdObjects[objectID];
    }

    void DisplayServerAddress()
    {
        menuController.UpdateServerAddress(PlayerPrefs.GetString("ServerAddress"));
    }

    void HandleConnection(GameEvent gameEvent)
    {
        networkClient.SetClientID(int.Parse(gameEvent.EventAttributes["ClientID"]));
        
        GameObject.FindFirstObjectByType<ClientGameUI>().DestroyWaitingObject();

        DisplayServerAddress();
    }

    void CreateNewGameTaskObject(GameEvent gameEvent)
    {
        Dictionary<string, string> creationAttributes = gameEvent.EventAttributes;

        string objectType = creationAttributes["ObjectType"];
        GameObject originalObject = typeNameToObjectDictionary[objectType];

        Vector3 position = GameEvent.ParseVector3(creationAttributes["ObjectPosition"]);
        Quaternion rotation = GameEvent.ParseQuaternion(creationAttributes["ObjectRotation"]);
        GameObject newObject = Instantiate(originalObject, position, rotation);

        int objectID = int.Parse(creationAttributes["ObjectID"]);
        newObject.GetComponent<GameTaskObject>().SetID(objectID);
        if (creationAttributes.ContainsKey("ControllerID"))
        {
            int controllerID = int.Parse(creationAttributes["ControllerID"]);
            newObject.GetComponent<GameTaskObject>().SetControllingPlayerID(controllerID);
        }

        createdObjects.Add(objectID, newObject.GetComponent<GameTaskObject>());

        /*if (creationAttributes.ContainsKey("ActionType"))
        {
            PerformActionOnObject(gameEvent);
        }*/
    }

    void PerformAction(GameEvent gameEvent)
    {
        Dictionary<string, string> actionAttributes = gameEvent.EventAttributes;

        GameTaskObject gameTaskObject = GetObjectByID(int.Parse(actionAttributes["ObjectID"]));
        gameTaskObject.PerformAction(actionAttributes);
    }

    void PerformControlAction(GameEvent gameEvent)
    {
        Dictionary<string, string> actionAttributes = gameEvent.EventAttributes;

        if (actionAttributes["ActionType"] == "UpdateScore")
        {
            string newScore = actionAttributes["NewScore"];
            menuController.UpdateScore(newScore);
        }
    }

    protected override void PerformEvent(GameEvent gameEvent)
    {
        if (gameEvent.Type == GameEvent.EventType.Connect)
        {
            HandleConnection(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventType.Create)
        {
            CreateNewGameTaskObject(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventType.Action)
        {
            PerformAction(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventType.Control)
        {
            PerformControlAction(gameEvent);
        }
        else
        {
            throw new NotImplementedException("Invalid GameEvent type encountered.");
        }
    }
}
