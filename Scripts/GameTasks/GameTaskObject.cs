using System;
using System.Collections.Generic;
using UnityEngine;
using GameEvents;

public class GameTaskObject : MonoBehaviour
{
    public int ObjectID { get;  private set; }
    public int ControllingPlayerID { get; private set; }

    protected NetworkClient networkClient;
    protected GameEventReceiver eventReceiver;

    void Awake()
    {
        networkClient = GameObject.FindFirstObjectByType<NetworkClient>();
        eventReceiver = GameObject.FindFirstObjectByType<GameEventReceiver>();
        SetControllingPlayerID(-1);
    }

    protected bool PerformTransformSet(Dictionary<string, string> actionAttributes)
    {
        transform.position = GameEvent.ParseEventPosition(actionAttributes["Position"]);
        transform.rotation = GameEvent.ParseEventRotation(actionAttributes["Rotation"]);
        return true;
    }

    public virtual bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "SetTransform":
                return PerformTransformSet(actionAttributes);
            default:
                return false;
        }
    }

    public void SetID(int newID)
    {
        ObjectID = newID;
    }

    protected void DisableEnableAllComponentsOfType<Type>(bool enable = false) where Type : Behaviour
    {
        if (gameObject.GetComponent<Type>() != null)
        {
            gameObject.GetComponent<Type>().enabled = enable;
        }
        foreach (Type objectOfType in gameObject.GetComponentsInChildren<Type>())
        {
            objectOfType.enabled = enable;
        }
    }

    public void SetControllingPlayerID(int newID)
    {
        ControllingPlayerID = newID;
        bool enable = (ControllingPlayerID == networkClient.GetClientID());

        if (newID == -1)
        {
            enable = true;
        }
        
        DisableEnableAllComponentsOfType<PlayerMovement>(enable);
        DisableEnableAllComponentsOfType<Camera>(enable);
        DisableEnableAllComponentsOfType<AudioListener>(enable);
    }

    public int GetPlayerID()
    {
        return networkClient.GetClientID();
    }

    protected string EncodeTransform()
    {
        return $"ActionType:SetTransform;Position:{GameEvent.EncodePosition(transform.position)};Rotation:{GameEvent.EncodeRotation(transform.rotation)}";
    }

    protected string EncodeCommonInformation(string encoding, bool temporary)
    {
        string tempString = "";
        if (temporary)
        {
            tempString = "Temporary";
        }

        return $"Action;{tempString};ObjectID:{ObjectID};{encoding}";
    }

    protected void SendEncodedAction(string encoding, bool temporary = false)
    {
        networkClient.SendEvent(new GameEvent(EncodeCommonInformation(encoding, temporary)));
    }

    public void SendTransformUpdate()
    {
        SendEncodedAction(EncodeTransform(), true);
    }

    public virtual void Activate()
    {
        return;
    }

    protected GameTaskObject GetObjectByID(int objectID)
    {
        return eventReceiver.GetObjectByID(objectID);
    }
}
