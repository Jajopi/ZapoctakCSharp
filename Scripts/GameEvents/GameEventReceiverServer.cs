using System;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using GameEvents;

public class GameEventReceiverServer : GameEventReceiver
{
    NetworkServer networkServer;
    MenuController menuController;


    List<GameTaskObject> createdObjects = new List<GameTaskObject>();

    void Awake()
    {
        networkServer = transform.GetComponent<NetworkServer>();
        menuController = GameObject.FindFirstObjectByType<MenuController>();
        typeNameToObjectDictionary = GameObject.FindFirstObjectByType<ObjectTypeDictionaryHolder>().GetDictionary();
    }

    void Start()
    {
        menuController.UpdateServerAddress(networkServer.GetReceivingAddress().ToString());
    }

    public override GameTaskObject GetObjectByID(int objectID)
    {
        return createdObjects[objectID];
    }

    void CreateNewPlayer(int controllerID)
    {
        CreateNewGameTaskObject(new GameEvent($"Create;ObjectType:Player;ControllerID:{controllerID};ObjectPosition:0~1~5;ObjectRotation:0~0~0~0"));
    }

    void ConnectNewClient(GameEvent gameEvent)
    {
        int newClientID = networkServer.AddClient(new Uri(gameEvent.EventAttributes["ClientAddress"]));

        CreateNewPlayer(newClientID);
    }

    void CreateNewGameTaskObject(GameEvent gameEvent)
    {
        Dictionary<string, string> creationAttributes = gameEvent.EventAttributes;

        string objectType = creationAttributes["ObjectType"];
        GameObject originalObject = typeNameToObjectDictionary[objectType];

        Vector3 position = GameEvent.ParseVector3(creationAttributes["ObjectPosition"]);
        Quaternion rotation = GameEvent.ParseQuaternion(creationAttributes["ObjectRotation"]);

        GameObject newObject = Instantiate(originalObject, position, rotation);
        int newObjectID = createdObjects.Count;
        newObject.GetComponent<GameTaskObject>().SetID(newObjectID);
        if (creationAttributes.ContainsKey("ControllerID"))
        {
            int controllerID = int.Parse(creationAttributes["ControllerID"]);
            newObject.GetComponent<GameTaskObject>().SetControllingPlayerID(controllerID);
        }

        createdObjects.Add(newObject.GetComponent<GameTaskObject>());

        /*if (creationAttributes.ContainsKey("ActionType"))
        {
            PerformActionOnObject(new GameEvent(gameEvent.ToString() + $";ObjectID:{newObjectID}"));
        }*/

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

    void PerformControlAction(GameEvent gameEvent)
    {
        Dictionary<string, string> actionAttributes = gameEvent.EventAttributes;

        if (actionAttributes["ActionType"] == "UpdateScore")
        {
            string newScore = actionAttributes["NewScore"];
            menuController.UpdateScore(newScore);
        }

        networkServer.SendEvent(gameEvent);
    }

    protected override void PerformEvent(GameEvent gameEvent)
    {
        if (gameEvent.Type == GameEvent.EventType.Connect)
        {
            ConnectNewClient(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventType.Create)
        {
            CreateNewGameTaskObject(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventType.Action)
        {
            PerformActionOnObject(gameEvent);
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
