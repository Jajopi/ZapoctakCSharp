using UnityEngine;
using System.Collections.Generic;
using GameEvents;

public class SwitchScript : GameTaskObject
{
    GameTaskObject targetObject;
    float activationCooldown;
    float activationCooldownTime = 2f;

    bool activated = false;

    Vector3 positionActivated = new Vector3(0, 0.2f, 0);
    Vector3 positionDeactivated = new Vector3(0, 1f, 0);
    Transform movingPart;

    new void Awake()
    {
        base.Awake();

        movingPart = transform.Find("MovingPart");
    }

    void TryMoveComponent()
    {
        Vector3 targetPosition = activated ? positionActivated : positionDeactivated;
        movingPart.localPosition = targetPosition;
    }

    void Update()
    {
        activationCooldown -= Time.deltaTime;
        if (activationCooldown <= 0)
        {
            activationCooldown = 0;
            activated = false;
        }

        TryMoveComponent();
    }

    bool PerformTargetSet(int objectID)
    {
        targetObject = GetObjectByID(objectID);
        return true;
    }

    bool PerformSwitchCycle()
    {
        activated = true;
        activationCooldown = activationCooldownTime;

        return targetObject.PerformAction(new GameEvent("Action;ActionType:CycleSwitch").EventAttributes);
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "SetTarget":
                return PerformTargetSet(int.Parse(actionAttributes["TargetID"]));
            case "CycleSwitch":
                return PerformSwitchCycle();
            default:
                return base.PerformAction(actionAttributes);
        }
    }

    public override void Activate()
    {
        if (activationCooldown <= 0)
        {
            SendEncodedAction("ActionType:CycleSwitch");
        }
    }

    public override void ActivateOnTouch()
    {
        Activate();
    }
}
