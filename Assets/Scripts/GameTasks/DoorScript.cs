using UnityEngine;
using System.Collections.Generic;
using GameEvents;

public class DoorScript : GameTaskObject
{
    bool isOpen = false;
    float openTime;
    public float openTimeReset = 60f;

    Vector3 leftWingPositionActivated = new Vector3(5f, 2.5f, 0);
    Vector3 leftWingPositionDeactivated = new Vector3(2.5f, 2.5f, 0);
    Vector3 rightWingPositionActivated = new Vector3(-5f, 2.5f, 0);
    Vector3 rightWingPositionDeactivated = new Vector3(-2.5f, 2.5f, 0);
    Transform leftWing;
    Transform rightWing;

    float maxSpeed = 0.5f;
    public float ActualSpeed { get { return maxSpeed * GetGlobalSpeed(); } }

    ShipController controller;

    new void Awake()
    {
        base.Awake();

        SetControllingPlayerID(0);
    }

    void Start()
    {
        controller = GameObject.FindFirstObjectByType<ShipController>();

        leftWing = transform.Find("LeftWing");
        rightWing = transform.Find("RightWing");
    }

    void UpdatePositions()
    {
        Vector3 leftTargetPosition = isOpen ? leftWingPositionActivated : leftWingPositionDeactivated;
        Vector3 rightTargetPosition = isOpen ? rightWingPositionActivated : rightWingPositionDeactivated;

        leftWing.localPosition = Vector3.MoveTowards(leftWing.localPosition,
                                                     leftTargetPosition,
                                                     ActualSpeed * Time.deltaTime);

        rightWing.localPosition = Vector3.MoveTowards(rightWing.localPosition,
                                                      rightTargetPosition,
                                                      ActualSpeed * Time.deltaTime);
    }

    void TryCloseAfterTime()
    {
        if (!IsControllingPlayer())
        {
            return;
        }

        if (openTime > 0)
        {
            openTime -= Time.deltaTime;
            if (openTime <= 0 && isOpen)
            {
                openTime = 0;
                PerformSwitchToggle(false);
            }
        }
    }

    void Update()
    {
        TryCloseAfterTime();

        UpdatePositions();
    }

    float GetGlobalSpeed()
    {
        return controller.GetGlobalSpeed();
    }

    bool ShouldBeLocked()
    {
        return controller.ShouldDoorBeLocked();
    }

    public bool PerformSwitchToggle(bool newOpen)
    {
        if (!IsControllingPlayer())
        {
            return false;
        }

        if (newOpen && !isOpen && !ShouldBeLocked())
        {
            SendEncodedAction("ActionType:Open");
        }
        else if (!newOpen && isOpen)
        {
            SendEncodedAction("ActionType:Close");
        }

        return true;
    }

    bool PerformClose()
    {
        if (isOpen)
        {
            isOpen = false;
            return true;
        }
        return false;
    }

    bool PerformOpen()
    {
        if (!isOpen)
        {
            isOpen = true;
            openTime = openTimeReset;
            return true;
        }
        return false;
    }

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "Close":
                return PerformClose();
            case "Open":
                return PerformOpen();
            case "CycleSwitch":
                return PerformSwitchToggle(!isOpen);
            default:
                return base.PerformAction(actionAttributes);
        }
    }
}
