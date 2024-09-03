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

    public GameObject messagePrefab;

    new void Awake()
    {
        base.Awake();

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
        oxygenLevel += 0.03f * (allCounts[type] - brokenCounts[type]) * Time.deltaTime;
        oxygenLevel -= 0.02f * brokenCounts[type] * Time.deltaTime;

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

    public static string TunelIndexLetter(int tunel)
    {
        if (tunel == 0) return "A";
        if (tunel == 1) return "B";
        if (tunel == 2) return "C";
        return "???";
    }

    string CreateMessageBroken(int floor, int tunel, int slot, BreakableGameTask.TaskType task)
    {
        return $"{BreakableGameTask.TaskTypeToString(task)} in section {TunelIndexLetter(tunel)}, floor {floor}, position {slot} broke.";
    }

    string CreateMessageNew(int floor, int tunel, int slot, BreakableGameTask.TaskType task)
    {
        return $"New {BreakableGameTask.TaskTypeToString(task)} spawned in section {TunelIndexLetter(tunel)}, floor {floor}, position {slot}.";
    }

    bool PerformTaskCreation()
    {
        BreakableGameTask.TaskType task = BreakableGameTask.GetRandomType();
        allCounts[task]++;

        int floor = (int)Mathf.Floor(Random.value * 3 % 3);
        if (GetPlayerCount() == 1 && floor > 1) floor = (int)Mathf.Floor(Random.value * 2 % 2);
        int tunel = (int)Mathf.Floor(Random.value * 3 % 3);
        if (task == BreakableGameTask.TaskType.DoorPanel) tunel = 1;
        int slot = (int)Mathf.Floor(Random.value * 30 % 30);

        while (floorTunelOccupied[floor, tunel, slot])
        {
            floor = (int)Mathf.Floor(Random.value * 3 % 3);
            if (GetPlayerCount() == 1 && floor > 1) floor = (int)Mathf.Floor(Random.value * 2 % 2);
            tunel = (int)Mathf.Floor(Random.value * 3 % 3);
            if (task == BreakableGameTask.TaskType.DoorPanel) tunel = 1;
            slot = (int)Mathf.Floor(Random.value * 30 % 30);
        }

        floorTunelOccupied[floor, tunel, slot] = true;

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
            tunel == 1 ? 180 : 90,
            0);

        string taskString = BreakableGameTask.TaskTypeToString(task);
        List<int> additionalInfo = new List<int> { floor, tunel, slot };
        networkClient.SendEvent(new GameEvent(
            $"Create;ObjectType:{taskString};ObjectPosition:{GameEvent.EncodeVector3(position)};ObjectRotation:{GameEvent.EulerRotation(GameEvent.EncodeVector3(eulerAngles))};AdditionalInfo:{CreateMessageBroken(floor, tunel, slot, task)}"
        ));

        EjectMessage(CreateMessageNew(floor, tunel, slot, task));

        return true;
    }

    public void EjectMessage(string message)
    {
        if (!IsControllingPlayer())
        {
            return;
        }

        Vector3 position = new Vector3(0, 14, 0);
        Quaternion rotation = Random.rotation;

        networkClient.SendEvent(new GameEvent(
            $"Create;ObjectType:Message;ObjectPosition:{GameEvent.EncodeVector3(position)};ObjectRotation:{GameEvent.EncodeQuaternion(rotation)};AdditionalInfo:{message}"
        ));
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

    /*public override void Activate()
    {
        SendEncodedAction("ActionType:CreateGameTask");
    }*/

    public void IncreaseTypeCount(BreakableGameTask.TaskType type)
    {
        allCounts[type]++;
    }

    public void IncreaseBroken(BreakableGameTask.TaskType type)
    {
        if (brokenCounts[type] == allCounts[type])
        {
            return;
        }

        brokenCounts[type]++;
    }

    public void DecreaseBroken(BreakableGameTask.TaskType type)
    {
        if (brokenCounts[type] == 0)
        {
            return;
        }

        brokenCounts[type]--;
    }

    public bool ShouldDoorBeLocked()
    {
        const BreakableGameTask.TaskType type = BreakableGameTask.TaskType.DoorPanel;
        
        return brokenCounts[type] >= (allCounts[type] - brokenCounts[type]);
    }

    public float GetGlobalSpeed()
    {
        const BreakableGameTask.TaskType type = BreakableGameTask.TaskType.WireBox;

        return (1f + allCounts[type] - brokenCounts[type]) / (1f + brokenCounts[type]);
    }

    public float GetGlobalBreakChance()
    {
        const BreakableGameTask.TaskType type = BreakableGameTask.TaskType.WireBox;

        return 1f + 0.05f * brokenCounts[type] - 0.05f * (allCounts[type] - brokenCounts[type]);
    }

    int GetPlayerCount()
    {
        return eventReceiver.GetPlayerCount();
    }
}
