using UnityEngine;
using System.Collections.Generic;
using GameEvents;

public class DoorScript : GameTaskObject
{
    bool isOpen = false;
    float openTime;
    float openTimeReset = 10f;

    Vector3 leftWingPositionActivated = new Vector3(5f, 2.5f, 0);
    Vector3 leftWingPositionDeactivated = new Vector3(2.5f, 2.5f, 0);
    Vector3 rightWingPositionActivated = new Vector3(-5f, 2.5f, 0);
    Vector3 rightWingPositionDeactivated = new Vector3(-2.5f, 2.5f, 0);
    Transform leftWing;
    Transform rightWing;

    float maxSpeed = 0.5f;
    public float ActualSpeed { get { return maxSpeed * GetGlobalSpeed(); } }

    int lockCount = 0;
    bool isReallyOpen { get { return lockCount == 0 && isOpen; } }

    void Start()
    {
        leftWing = transform.Find("LeftWing");
        rightWing = transform.Find("RightWing");
    }

    void UpdatePositions()
    {
        Vector3 leftTargetPosition = isReallyOpen ? leftWingPositionActivated : leftWingPositionDeactivated;
        Vector3 rightTargetPosition = isReallyOpen ? rightWingPositionActivated : rightWingPositionDeactivated;

        leftWing.localPosition = Vector3.MoveTowards(leftWing.localPosition,
                                                     leftTargetPosition,
                                                     ActualSpeed * Time.deltaTime);

        rightWing.localPosition = Vector3.MoveTowards(rightWing.localPosition,
                                                      rightTargetPosition,
                                                      ActualSpeed * Time.deltaTime);
    }

    void Update()
    {
        openTime -= Time.deltaTime;
        if (openTime <= 0)
        {
            openTime = 0;
            isOpen = false;
        }

        UpdatePositions();
    }

    float GetGlobalSpeed()
    {
        return 1;
    }

    bool PerformLock(bool unlock)
    {
        if (unlock)
        {
            lockCount--;
            if (lockCount < 0)
            {
                lockCount = 0;
            }
        }
        else
        {
            lockCount++;
        }

        return true;
    }

    public bool PerformSwitchToggle(bool newOpen)
    {
        isOpen = newOpen;

        if (newOpen)
        {
            openTime = openTimeReset;
        }

        return true;
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "CycleSwitch":
                return PerformSwitchToggle(!isOpen);
            case "AddOrRemoveLock":
                return PerformLock(actionAttributes["LockChange"] == "Add");
            default:
                return base.PerformAction(actionAttributes);
        }
    }
}
