using System;
using System.Collections.Generic;
using UnityEngine;
using GameEvents;

using Random = UnityEngine.Random;

public class BreakableGameTask : GameTaskObject
{
    public enum TaskType
    {
        DoorPanel, OxyGenerator, MotorReactor, WireBox, Invalid
    }

    public static string TaskTypeToString(TaskType type)
    {
        switch (type)
        {
            case TaskType.DoorPanel: return "DoorPanel";
            case TaskType.OxyGenerator: return "OxyGenerator";
            case TaskType.MotorReactor: return "MotorReactor";
            case TaskType.WireBox: return "WireBox";
            default: return "Invalid";
        }
    }

    public static TaskType[] TaskTypes = { TaskType.DoorPanel, TaskType.OxyGenerator, TaskType.MotorReactor, TaskType.WireBox };
    public static TaskType GetRandomType()
    {
        return TaskTypes[(int)Mathf.Floor(Random.value * TaskTypes.Length) % TaskTypes.Length];
    }


    public float inverseLocalBreakChance = 0f;
    public TaskType type = TaskType.Invalid;

    protected float GlobalBreakChance { get { return controller.GetGlobalBreakChance(); } }
    protected ShipController controller;

    public bool IsBroken { get; private set; }
    string messageBroken;

    Vector3 positionFixed = new Vector3(1, 0.25f, 0);
    Vector3 positionBroken = new Vector3(1, -0.25f, 0);
    Transform movingPart;

    new protected void Awake()
    {
        base.Awake();

        controller = GameObject.FindFirstObjectByType<ShipController>();
        SetControllingPlayerID(0);

        movingPart = transform.Find("MovingPart");

        controller.IncreaseTypeCount(type);
    }

    protected void Update()
    {
        if (IsControllingPlayer())
        {
            TryBreak();
        }
        TryMoveComponent();
    }

    protected void TryBreak()
    {
        if (IsBroken)
        {
            return;
        }
        if (Random.value < GlobalBreakChance * Time.deltaTime / inverseLocalBreakChance)
        {
            SendEncodedAction("ActionType:Break");
        }
    }

    void TryMoveComponent()
    {
        Vector3 targetPosition = IsBroken ? positionBroken : positionFixed;
        movingPart.localPosition = targetPosition;
    }

    protected bool PerformBreak()
    {
        if (IsBroken)
        {
            return false;
        }

        IsBroken = true;

        if (IsControllingPlayer())
        {
            controller.EjectMessage(messageBroken);
            controller.IncreaseBroken(type);
        }
        return true;
    }

    protected bool PerformFix()
    {
        if (!IsBroken)
        {
            return false;
        }

        IsBroken = false;

        if (IsControllingPlayer())
        {
            controller.DecreaseBroken(type);
        }
        return true;
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "Break":
                return PerformBreak();
            case "Fix":
                return PerformFix();
            default:
                return base.PerformAction(actionAttributes);
        }
    }

    public override void ActivateOnHold()
    {
        if (IsBroken)
        {
            SendEncodedAction("ActionType:Fix");
        }
    }

    public override void AddInfo(string info)
    {
        messageBroken = info;
    }
}
