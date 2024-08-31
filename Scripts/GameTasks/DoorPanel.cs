using UnityEngine;
using System.Collections.Generic;
using GameEvents;

public class DoorPanel : GameTaskObject
{
    GameTaskObject targetObject;

    bool broken = false;

    Vector3 positionFixed = new Vector3(1, -0.25f, 0);
    Vector3 positionBroken = new Vector3(1, 0.25f, 0);
    Transform movingPart;

    void Start()
    {
        movingPart = transform.Find("MovingPart");

        PerformBreak();
    }

    void TryMoveComponent()
    {
        Vector3 targetPosition = broken ? positionBroken : positionFixed;
        /*movingPart.localPosition = Vector3.MoveTowards(movingPart.localPosition,
                                                       targetPosition,
                                                       movementSpeed * Time.deltaTime);*/
        movingPart.localPosition = targetPosition;
    }

    void Update()
    {
        TryMoveComponent();
    }

    bool PerformTargetSet(int objectID)
    {
        targetObject = GetObjectByID(objectID);
        return true;
    }

    bool PerformBreak()
    {
        if (broken)
        {
            return false;
        }

        broken = true;

        return targetObject.PerformAction(new GameEvent("Action;ActionType:AddOrRemoveLock;LockChange:Add").EventAttributes);
    }

    bool PerformFix()
    {
        if (!broken)
        {
            return false;
        }

        broken = false;

        return targetObject.PerformAction(new GameEvent("Action;ActionType:AddOrRemoveLock;LockChange:Remove").EventAttributes);
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "SetTarget":
                return PerformTargetSet(int.Parse(actionAttributes["TargetID"]));
            case "TryBreak":
                return PerformBreak();
            case "TryFix":
                return PerformFix();
            default:
                return base.PerformAction(actionAttributes);
        }
    }

    public override void Activate()
    {
        SendEncodedAction("ActionType:TryFix");
    }
}
