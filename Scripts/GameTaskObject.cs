using System;
using System.Collections.Generic;
using UnityEngine;
using GameEvents;

public class GameTaskObject : MonoBehaviour
{
    public int ObjectID { get;  private set; }

    public bool PerformAction(Dictionary<string, string> actionAttributes)
    {
        foreach (string key in actionAttributes.Keys)
        {
            Debug.Log(key + ": " + actionAttributes[key]);
        }
        return true;
    }

    public void SetID(int newID)
    {
        ObjectID = newID;
    }

    public string EncodeTransform()
    {
        return $"Action;ObjectID:{ObjectID};ActionType:SetTransform;Position:{GameEvent.EncodePosition(transform.position)};Rotation:{GameEvent.EncodeRotation(transform.rotation)}";
    }
}
