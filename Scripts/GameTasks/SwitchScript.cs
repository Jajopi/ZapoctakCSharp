using UnityEngine;
using System.Collections.Generic;
using GameEvents;

public class SwitchScript : GameTaskObject
{
    int switchPosition;
    GameTaskObject targetObject;
    List<float> positionValues = new List<float>();

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    bool PerformSwitchValuesSet(List<float> values)
    {
        positionValues = values;
        return true;
    }

    bool PerformTargetSet(int targetID)
    {
        targetObject = eventReceiver.GetObjectByID(targetID);
        return true;
    }

    public bool PerformSwitchToggle(int newPosition)
    {
        if (newPosition < 0 || newPosition >= positionValues.Count)
        {
            return false;
        }

        switchPosition = newPosition;
        
        //targetObject

        return true;
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "SetTarget":
                return PerformTargetSet(int.Parse(actionAttributes["TargetID"]));
            case "ToggleSwitch":
                return PerformSwitchToggle(int.Parse(actionAttributes["SwitchPosition"]));
            case "CycleSwitch":
                return PerformSwitchToggle(switchPosition + 1);
            case "SetSwitchValues":
                return PerformSwitchValuesSet(GameEvent.ParseListOfFloats(actionAttributes["SwitchValues"]));
            default:
                return base.PerformAction(actionAttributes);
        }
    }

    public override void Activate()
    {
        SendEncodedAction("CycleSwitch");
    }
}
