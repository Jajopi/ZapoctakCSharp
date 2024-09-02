using System;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using GameEvents;
using System.Globalization;

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

    void DestroyPlayer(GameEvent gameEvent)
    {
        Dictionary<string, string> attributes = gameEvent.EventAttributes;

        GameObject.Destroy(GetObjectByID(int.Parse(attributes["ObjectID"])).gameObject);
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
                menuController.ExitGame(true);
            }
        }
    }

    protected override void PerformEvent(GameEvent gameEvent)
    {
        if (gameEvent.Type == GameEvent.EventType.Connect)
        {
            HandleConnection(gameEvent);
        }
        else if (gameEvent.Type == GameEvent.EventType.Disconnect)
        {
            DestroyPlayer(gameEvent);
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
