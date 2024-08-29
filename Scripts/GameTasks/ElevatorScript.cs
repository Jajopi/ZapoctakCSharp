using UnityEngine;
using System.Collections.Generic;
using GameEvents;

public class ElevatorScript : GameTaskObject
{
    int switchPosition;
    List<float> positionValues = new List<float>();

    public float maxSpeed = 2f;
    public float ActualSpeed { get { return maxSpeed * GetGlobalSpeed(); } }

    void Update()
    {
        if (positionValues.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                                                     new Vector3(transform.position.x,
                                                                 positionValues[switchPosition],
                                                                 transform.position.z),
                                                     ActualSpeed * Time.deltaTime);
        }
    }

    float GetGlobalSpeed()
    {
        return 1;
    }

    bool PerformSwitchValuesSet(List<float> values)
    {
        positionValues = values;
        return true;
    }

    public bool PerformSwitchToggle(int newPosition)
    {
        if (newPosition < 0 || newPosition >= positionValues.Count)
        {
            return false;
        }

        switchPosition = newPosition;

        return true;
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "CycleSwitch":
                return PerformSwitchToggle((switchPosition + 1) % positionValues.Count);
            case "SetSwitchValues":
                return PerformSwitchValuesSet(GameEvent.ParseListOfFloats(actionAttributes["SwitchValues"]));
            default:
                return base.PerformAction(actionAttributes);
        }
    }
}
