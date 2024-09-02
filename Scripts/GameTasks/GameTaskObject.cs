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

    protected void Awake()
    {
        networkClient = GameObject.FindFirstObjectByType<NetworkClient>();
        eventReceiver = GameObject.FindFirstObjectByType<GameEventReceiver>();
        SetControllingPlayerID(-1);
    }

    protected bool PerformTransformSet(Dictionary<string, string> actionAttributes)
    {
        transform.position = GameEvent.ParseVector3(actionAttributes["Position"]);
        transform.rotation = GameEvent.ParseQuaternion(actionAttributes["Rotation"]);
        
        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        if (rigidbody is not null && actionAttributes.ContainsKey("Velocity"))
        {
            rigidbody.linearVelocity = GameEvent.ParseVector3(actionAttributes["Velocity"]);
        }
        return true;
    }

    public virtual bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        switch (actionAttributes["ActionType"])
        {
            case "SetTransform":
                if (ControllingPlayerID == GetPlayerID())
                {
                    return true;
                }
                return PerformTransformSet(actionAttributes);
            default:
                return false;
        }
    }

    public virtual void AddInfo(string info)
    {
        return;
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

    public bool IsControllingPlayer()
    {
        return ControllingPlayerID == GetPlayerID();
    }

    protected string EncodeTransform()
    {
        string velocityString = "";
        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        if (rigidbody is not null)
        {
            string velocityEncoded = GameEvent.EncodeVector3(rigidbody.linearVelocity);
            velocityString = "Velocity:" + velocityEncoded;
        }

        return $"ActionType:SetTransform;Position:{GameEvent.EncodeVector3(transform.position)};Rotation:{GameEvent.EncodeQuaternion(transform.rotation)};{velocityString};";
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
        SendArbitraryEvent(new GameEvent(EncodeCommonInformation(encoding, temporary)));
    }

    public void SendArbitraryEvent(GameEvent gameEvent)
    {
        networkClient.SendEvent(gameEvent);
    }

    public void SendTransformUpdate()
    {
        SendEncodedAction(EncodeTransform(), true);
    }

    public virtual void Activate()
    {
        return;
    }

    public virtual void ActivateOnTouch()
    {
        return;
    }

    protected GameTaskObject GetObjectByID(int objectID)
    {
        return eventReceiver.GetObjectByID(objectID);
    }
}
