using UnityEngine;
using GameEvents;
using Networking;
using TMPro;

public class ServerGameInitializer : MonoBehaviour
{
    GameEventReceiverServer eventReceiver;

    int actualIndex = 0;
    int LastIndex { get { return actualIndex - 1; } }

    void SendEvent(string encoding)
    {
        GameEvent gameEvent = new GameEvent(encoding);

        if (gameEvent.Type == GameEvent.EventTypes.Create)
        {
            actualIndex++;
        }

        eventReceiver.ReceiveEvents(new DataToken(gameEvent.ToString()));
    }

    void DisplayServerAddress()
    {
        NetworkServer networkServer = GameObject.FindFirstObjectByType<NetworkServer>();
        TMP_Text text = GameObject.FindFirstObjectByType<TMP_Text>();
        text.text = networkServer.GetReceivingAddress().ToString();
    }

    string EncodeRotation(string eulerAngles)
    {
        return GameEvent.EncodeQuaternion(Quaternion.Euler(GameEvent.ParseVector3(eulerAngles)));
    }

    void CreateLevel()
    {
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:60~0~0;ObjectRotation:{EncodeRotation("0~0~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:0~0~-60;ObjectRotation:{EncodeRotation("0~90~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:-60~0~0;ObjectRotation:{EncodeRotation("0~180~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:0~0~60;ObjectRotation:{EncodeRotation("0~270~0")}");

        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:60~5~0;ObjectRotation:{EncodeRotation("0~0~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:0~5~-60;ObjectRotation:{EncodeRotation("0~90~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:-60~5~0;ObjectRotation:{EncodeRotation("0~180~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:0~5~60;ObjectRotation:{EncodeRotation("0~270~0")}");

        /*SendEvent($"Create;ObjectType:Tunel;ObjectPosition:43~0~43;ObjectRotation:{EncodeRotation("0~315~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:-43~0~43;ObjectRotation:{EncodeRotation("0~225~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:-43~0~-43;ObjectRotation:{EncodeRotation("0~135~0")}");
        SendEvent($"Create;ObjectType:Tunel;ObjectPosition:43~0~-43;ObjectRotation:{EncodeRotation("0~45~0")}");*/

        SendEvent($"Create;ObjectType:Plane;ObjectPosition:0~0~0;ObjectRotation:{EncodeRotation("0~0~0")}");

        SendEvent($"Create;ObjectType:Player;ControllerID:0;ObjectPosition:0~2~0;ObjectRotation:0~0~0~0");

        SendEvent($"Create;ObjectType:Elevator;ObjectPosition:9~0~0;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetSwitchValues;SwitchValues:0~5~10~15");
        SendEvent($"Create;ObjectType:ElevatorSwitch;ObjectPosition:5~0~0;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");

        SendEvent($"Create;ObjectType:Elevator;ObjectPosition:-9~0~0;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetSwitchValues;SwitchValues:0~5~10~15");
        SendEvent($"Create;ObjectType:ElevatorSwitch;ObjectPosition:-5~0~0;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");

        SendEvent($"Create;ObjectType:Elevator;ObjectPosition:0~0~9;ObjectRotation:{EncodeRotation("0~90~0")}");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetSwitchValues;SwitchValues:0~5~10~15");
        SendEvent($"Create;ObjectType:ElevatorSwitch;ObjectPosition:0~0~5;ObjectRotation:{EncodeRotation("0~90~0")}");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");

        SendEvent($"Create;ObjectType:Elevator;ObjectPosition:0~0~-9;ObjectRotation:{EncodeRotation("0~90~0")}");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetSwitchValues;SwitchValues:0~5~10~15");
        SendEvent($"Create;ObjectType:ElevatorSwitch;ObjectPosition:0~0~-5;ObjectRotation:{EncodeRotation("0~90~0")}");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");
    }

    void Start()
    {
        eventReceiver = GameObject.FindFirstObjectByType<GameEventReceiverServer>();

        CreateLevel();

        DisplayServerAddress();
    }

    void Update()
    {

    }
}
