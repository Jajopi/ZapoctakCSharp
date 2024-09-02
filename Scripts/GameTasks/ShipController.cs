using System;
using System.Collections.Generic;
using UnityEngine;
using GameEvents;

using Random = UnityEngine.Random;

public class ShipController : GameTaskObject
{
    bool[,,] floorTunelOccupied = new bool[3, 3, 30];
    Dictionary<BreakableGameTask.TaskType, int> brokenCounts = new();
    Dictionary<BreakableGameTask.TaskType, int> allCounts = new();

    float distancePassed = 0f;
    float shipSpeed = 0f;
    float oxygenLevel = 1f;

    float updateSendingTime = 0f;
    float sendUpdatesEvery_seconds = 0.2f;

    float newTaskCreationTime = 20f;
    float createNewTaskEvery_seconds = 30f;

    void Start()
    {
        SetControllingPlayerID(0);

        foreach (BreakableGameTask.TaskType type in BreakableGameTask.TaskTypes)
        {
            brokenCounts[type] = 0;
            allCounts[type] = 0;
        }
    }

    void Update()
    {
        if (IsControllingPlayer())
        {
            UpdateOxygen();
            UpdateSpeed();
            UpdateDistance();
        }

        updateSendingTime -= Time.deltaTime;
        if (updateSendingTime <= 0)
        {
            SendUpdates();
            updateSendingTime = sendUpdatesEvery_seconds;
        }

        newTaskCreationTime -= Time.deltaTime;
        if (newTaskCreationTime <= 0)
        {
            SendEncodedAction("ActionType:CreateGameTask");
            newTaskCreationTime = createNewTaskEvery_seconds;
        }
    }

    void SendUpdates()
    {
        SendArbitraryEvent(new GameEvent($"Control;Temporary;ActionType:UpdateDistance;NewDistance:{distancePassed}"));
        SendArbitraryEvent(new GameEvent($"Control;Temporary;ActionType:UpdateOxygen;NewOxygen:{oxygenLevel}"));
    }

    void UpdateOxygen()
    {
        const BreakableGameTask.TaskType type = BreakableGameTask.TaskType.OxyGenerator;

        oxygenLevel -= 0.01f * GetPlayerCount() * Time.deltaTime;
        oxygenLevel += 0.01f * (allCounts[type] - brokenCounts[type]) * Time.deltaTime;
        oxygenLevel -= 0.01f * brokenCounts[type] * Time.deltaTime;

        if (oxygenLevel > 1)
        {
            oxygenLevel = 1f;
        }
    }

    void UpdateSpeed()
    {
        const BreakableGameTask.TaskType type = BreakableGameTask.TaskType.MotorReactor;

        float maxSpeed = 1f * allCounts[type];
        
        shipSpeed += 0.1f * (allCounts[type] - brokenCounts[type]) * Time.deltaTime;
        shipSpeed -= 0.1f * brokenCounts[type] * Time.deltaTime;

        if (shipSpeed > maxSpeed)
        {
            shipSpeed = maxSpeed;
        }
        if (shipSpeed < 0)
        {
            shipSpeed = 0;
        }
    }

    void UpdateDistance()
    {
        distancePassed += shipSpeed * Time.deltaTime;
    }

    static string TunelIndexLetter(int tunel)
    {
        if (tunel == 0) return "A";
        if (tunel == 1) return "B";
        if (tunel == 2) return "C";
        return "";
    }

    bool PerformTaskCreation()
    {
        BreakableGameTask.TaskType task = BreakableGameTask.GetRandomType();
        allCounts[task]++;

        int floor = (int)Mathf.Floor(Random.value * 3 % 3);
        int tunel = (int)Mathf.Floor(Random.value * 3 % 3);
        if (task == BreakableGameTask.TaskType.DoorPanel) tunel = 1;
        int slot = (int)Mathf.Floor(Random.value * 30 % 30);

        while (floorTunelOccupied[floor, tunel, slot])
        {
            floor = (int)Mathf.Floor(Random.value * 3 % 3);
            tunel = (int)Mathf.Floor(Random.value * 3 % 3);
            if (task == BreakableGameTask.TaskType.DoorPanel) tunel = 1;
            slot = (int)Mathf.Floor(Random.value * 30 % 30);
        }

        floorTunelOccupied[floor, tunel, slot];

        Debug.Log($"{floor}, {tunel}, {slot}");

        Vector3 position = new Vector3();
        switch (tunel)
        {
            case 0:
                position = new Vector3(slot * 1f + 20f,
                                       floor * 5f + 0.2f + 1f,
                                       3f);
                break;
            case 1:
                position = new Vector3(3f,
                                       floor * 5f + 0.2f + 1f,
                                       -(slot * 1f + 20f));
                break;
            case 2:
                position = new Vector3(-(slot * 1f + 20f),
                                       floor * 5f + 0.2f + 1f,
                                       3f);
                break;
            
        }

        Vector3 eulerAngles = new Vector3(
            0,
            tunel == 1 ? 0 : -90,
            0);

        string taskString = BreakableGameTask.TaskTypeToString(task);
        networkClient.SendEvent(new GameEvent($"Create;ObjectType:{taskString};ObjectPosition:{GameEvent.EncodeVector3(position)};ObjectRotation:{GameEvent.EulerRotation(GameEvent.EncodeVector3(eulerAngles))}"));

        return true;
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        if (!IsControllingPlayer())
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

    public void IncreaseTypeCount(BreakableGameTask.TaskType type)
    {
        allCounts[type]++;
    }

    public bool TryIncreaseBroken(BreakableGameTask.TaskType type)
    {
        if (brokenCounts[type] == allCounts[type])
        {
            return false;
        }

        brokenCounts[type]++;

        return true;
    }

    public bool TryDecreaseBroken(BreakableGameTask.TaskType type)
    {
        if (brokenCounts[type] == 0)
        {
            return false;
        }

        brokenCounts[type]--;

        return true;
    }

    public bool ShouldDoorBeLocked()
    {
        const BreakableGameTask.TaskType type = BreakableGameTask.TaskType.DoorPanel;
        
        return brokenCounts[type] >= (allCounts[type] - brokenCounts[type]);
    }

    public float GetGlobalSpeed()
    {
        return 1f / (1 + 0.2f * brokenCounts[BreakableGameTask.TaskType.WireBox]);
    }

    public float GetGlobalBreakChance()
    {
        return 1f + (0.05f * brokenCounts[BreakableGameTask.TaskType.WireBox]);
    }

    int GetPlayerCount()
    {
        return eventReceiver.GetPlayerCount();
    }
}
