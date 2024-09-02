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

    Vector3 positionFixed = new Vector3(1, -0.25f, 0);
    Vector3 positionBroken = new Vector3(1, 0.25f, 0);
    Transform movingPart;

    protected void Start()
    {
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
        /*movingPart.localPosition = Vector3.MoveTowards(movingPart.localPosition,
                                                       targetPosition,
                                                       movementSpeed * Time.deltaTime);*/
        movingPart.localPosition = targetPosition;
    }

    protected bool PerformBreak()
    {
        if (IsBroken)
        {
            return false;
        }

        IsBroken = true;

        return controller.TryIncreaseBroken(type);
    }

    protected bool PerformFix()
    {
        if (!IsBroken)
        {
            return false;
        }

        IsBroken = false;

        return controller.TryDecreaseBroken(type);
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

    public override void Activate()
    {
        if (IsBroken)
        {
            SendEncodedAction("ActionType:Fix");
        }
    }
}
