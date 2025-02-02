using System;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using GameEvents;
using System.Globalization;


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

        playerCount = 1;
    }

    void Start()
    {
        menuController.UpdateServerAddress(networkServer.GetReceivingAddress().ToString());
    }

    public override GameTaskObject GetObjectByID(int objectID)
    {
        return createdObjects[objectID];
    }

    void CreateNewPlayer(int controllerID, string playerName)
    {
        playerCount++;
        CreateNewGameTaskObject(new GameEvent($"Create;ObjectType:Player;ControllerID:{controllerID};ObjectPosition:0~1~5;ObjectRotation:0~0~0~0;AdditionalInfo:{playerName}"));
    }

    void ConnectNewClient(GameEvent gameEvent)
    {
        int newClientID = networkServer.AddClient(new Uri(gameEvent.EventAttributes["ClientAddress"]));
        string playerName = gameEvent.EventAttributes["PlayerName"];

        CreateNewPlayer(newClientID, playerName);
    }

    void DisconnectClient(GameEvent gameEvent)
    {
        Dictionary<string, string> attributes = gameEvent.EventAttributes;

        int objectID = int.Parse(attributes["ObjectID"]);
        GameObject.Destroy(GetObjectByID(objectID).gameObject);
        networkServer.RemoveClient(int.Parse(attributes["ClientID"]));

        playerCount--;

        networkServer.SendEvent(gameEvent);
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

        if (creationAttributes.ContainsKey("AdditionalInfo"))
        {
            newObject.GetComponent<GameTaskObject>().AddInfo(creationAttributes["AdditionalInfo"]);
        }

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

        if (actionAttributes["ActionType"] == "UpdateDistance")
        {
            float distance = float.Parse(GameEvent.StandardizeFloats(
                    actionAttributes["NewDistance"]),
                    CultureInfo.InvariantCulture);
            menuController.UpdateDistance(distance);

            PlayerPrefs.SetFloat("Distance", distance);
            if (distance > PlayerPrefs.GetFloat("BestDistance"))
            {
                PlayerPrefs.SetFloat("BestDistance", distance);
            }
        }

        if (actionAttributes["ActionType"] == "UpdateOxygen")
        {
            float oxygen = float.Parse(GameEvent.StandardizeFloats(
                    actionAttributes["NewOxygen"]),
                    CultureInfo.InvariantCulture);
            menuController.UpdateOxygen(oxygen);

            if (oxygen <= 0)
            {
                networkServer.SendEvent(gameEvent);
                menuController.ExitGame(true);
                return;
            }
        }

        networkServer.SendEvent(gameEvent);
    }

    protected override void PerformEvent(GameEvent gameEvent)
    {
        if (gameEvent.Type == GameEvent.EventType.Connect)
        {
            ConnectNewClient(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventType.Disconnect)
        {
            DisconnectClient(gameEvent);
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
