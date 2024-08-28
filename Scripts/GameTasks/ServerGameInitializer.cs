using UnityEngine;
using GameEvents;
using Networking;
using TMPro;

public class ServerGameInitializer : MonoBehaviour
{
    GameEventReceiverServer eventReceiver;

    void SendEvent(string encoding)
    {
        eventReceiver.ReceiveEvents(new DataToken(new GameEvent(encoding).ToString()));
    }

    void Start()
    {
        eventReceiver = GameObject.FindFirstObjectByType<GameEventReceiverServer>();

        SendEvent("Create;ObjectType:Plane;ObjectPosition:0~0~0;ObjectRotation:0~0~0~0");
        SendEvent("Create;ObjectType:Player;ControllerID:0;ObjectPosition:0~10~0;ObjectRotation:0~0~0~0");
        SendEvent("Create;ObjectType:Elevator;ObjectPosition:0~0~10;ObjectRotation:0~0~0~0");
        SendEvent("Create;ObjectType:ElevatorSwitch;ObjectPosition:0~0~5;ObjectRotation:0~0~0~0");
        int elevatorID = GameObject.FindFirstObjectByType<ElevatorScript>().gameObject.GetComponent<GameTaskObject>().ObjectID;
        int switchID = GameObject.FindFirstObjectByType<SwitchScript>().gameObject.GetComponent<GameTaskObject>().ObjectID;
        SendEvent($"Action;ObjectID:{switchID};ActionType:SetTarget;TargetID:{elevatorID};");

        NetworkServer networkServer = GameObject.FindFirstObjectByType<NetworkServer>();
        TMP_Text text = GameObject.FindFirstObjectByType<TMP_Text>();
        text.text = networkServer.GetReceivingAddress().ToString();
    }

    void Update()
    {

    }
}
