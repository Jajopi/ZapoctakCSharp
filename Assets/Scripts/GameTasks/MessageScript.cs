using System;
using System.Collections.Generic;
using UnityEngine;
using GameEvents;
using TMPro;

public class MessageScript : GameTaskObject
{
    string message;
    Canvas canvas;
    bool isEnabled = false;

    public override bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "Destroy":
                if (ControllingPlayerID == GetPlayerID())
                {
                    return true;
                }
                return PerformTransformSet(actionAttributes);
            default:
                return false;
        }
    }

    new void Awake()
    {
        base.Awake();

        canvas = GameObject.FindFirstObjectByType<MenuController>().GetMessageCanvas();
    }

    void Update()
    {
        if (isEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyUp(KeyCode.E))
            {
                Deactivate();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Deactivate();
                Destroy(gameObject);
            }
        }
    }

    string GetMessageText()
    {
        return message;
    }

    public override void Activate()
    {
        if (isEnabled)
        {
            return;
        }

        canvas.enabled = true;
        canvas.transform.Find("MessageText").GetComponent<TMP_Text>().text = GetMessageText();

        isEnabled = true;
    }

    void Deactivate()
    {
        canvas.enabled = false;

        isEnabled = false;
    }

    public void SetMessage(string newMessage)
    {
        message = newMessage;
    }

    public override void AddInfo(string info)
    {
        SetMessage(info);
    }
}
