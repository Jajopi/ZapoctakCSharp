using System;
using System.Collections.Generic;
using UnityEngine;
using GameEvents;

using Random = UnityEngine.Random;

public class ShipController : GameTaskObject
{
    public List<string> SpawnableObjectTypes;

    List<GameTaskObject>[,] FloorTunelObjects = new List<GameTaskObject>[3,3];

    void Start()
    {
        SetControllingPlayerID(0);

        for (int floor = 0; floor < 3; floor++)
        {
            for (int tunel = 0; tunel < 3; tunel++)
            {
                FloorTunelObjects[floor,tunel] = new List<GameTaskObject>();
            }
        }
    }

    bool PerformTaskCreation()
    {
        if (SpawnableObjectTypes.Count == 0)
        {
            return false;
        }

        int index = (int)Math.Floor(Random.value * SpawnableObjectTypes.Count) % SpawnableObjectTypes.Count;
        string type = SpawnableObjectTypes[index];
        Vector3 position = new Vector3(1, 1, 1);
        Vector3 eulerAngles = new Vector3();

        networkClient.SendEvent(new GameEvent($"Create;ObjectType:{type};ObjectPosition:{GameEvent.EncodeVector3(position)};ObjectRotation:{GameEvent.EulerRotation(GameEvent.EncodeVector3(eulerAngles))}"));

        //networkClient.SendEvent(new GameEvent($"Action;ObjectID:{objectID};ActionType:TryBreak");

        return true;
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        if (ControllingPlayerID != GetPlayerID())
        {
            return base.PerformAction(actionAttributes);
        }

        switch (actionAttributes["ActionType"])
        {
            case "CreateGameTask":
                return PerformTaskCreation();
            default:
                return base.PerformAction(actionAttributes);
        }
    }

    public override void Activate()
    {
        SendEncodedAction("ActionType:CreateGameTask");
    }
}
