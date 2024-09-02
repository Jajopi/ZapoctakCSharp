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

        if (gameEvent.Type == GameEvent.EventType.Create)
        {
            actualIndex++;
        }

        eventReceiver.ReceiveEvents(new DataToken(gameEvent.ToString()));
    }

    string EulerRotation(string eulerAngles)
    {
        return GameEvent.EulerRotation(eulerAngles);
    }

    string CreateMessageBroken(BreakableGameTask.TaskType task)
    {
        return $"{BreakableGameTask.TaskTypeToString(task)} in main section broke.";
    }

    void CreateLevel()
    {
        // Ship hull
        SendEvent($"Create;ObjectType:Ship;ObjectPosition:0~-0.2~0;ObjectRotation:0~0~0~0");

        // Player
        SendEvent($"Create;ObjectType:Player;ControllerID:0;ObjectPosition:0~1~5;ObjectRotation:0~0~0~0");

        // Elevators
        SendEvent($"Create;ObjectType:Elevator;ObjectPosition:9~0~0;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetSwitchValues;SwitchValues:0~5~10~5");
        SendEvent($"Create;ObjectType:Switch;ObjectPosition:6~0~0;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");
        
        SendEvent($"Create;ObjectType:Elevator;ObjectPosition:-9~0~0;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetSwitchValues;SwitchValues:0~5~10~5");
        SendEvent($"Create;ObjectType:Switch;ObjectPosition:-6~0~0;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");
        
        SendEvent($"Create;ObjectType:Elevator;ObjectPosition:0~0~-9;ObjectRotation:{EulerRotation("0~90~0")}");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetSwitchValues;SwitchValues:0~5~10~5");
        SendEvent($"Create;ObjectType:Switch;ObjectPosition:0~0~-6;ObjectRotation:0~0~0~0");
        SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");

        // Doors
        for (float height = 0; height < 11; height += 5)
        {
            SendEvent($"Create;ObjectType:Door;ObjectPosition:15~{height}~0;ObjectRotation:{EulerRotation("0~90~0")}");
            SendEvent($"Create;ObjectType:Switch;ObjectPosition:13~{height + 0.2f}~-4;ObjectRotation:0~0~0~0");
            SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");
            SendEvent($"Create;ObjectType:Switch;ObjectPosition:17~{height + 0.2f}~4;ObjectRotation:0~0~0~0");
            SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 2}");

            /*SendEvent($"Create;ObjectType:Door;ObjectPosition:0~{height}~-15;ObjectRotation:0~0~0~0");
            SendEvent($"Create;ObjectType:Switch;ObjectPosition:-4~{height + 0.2f}~-13;ObjectRotation:0~0~0~0");
            SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");
            SendEvent($"Create;ObjectType:Switch;ObjectPosition:4~{height + 0.2f}~-17;ObjectRotation:0~0~0~0");
            SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 2}");
            */

            SendEvent($"Create;ObjectType:Door;ObjectPosition:-15~{height}~0;ObjectRotation:{EulerRotation("0~90~0")}");
            SendEvent($"Create;ObjectType:Switch;ObjectPosition:-13~{height + 0.2f}~4;ObjectRotation:0~0~0~0");
            SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 1}");
            SendEvent($"Create;ObjectType:Switch;ObjectPosition:-17~{height + 0.2f}~-4;ObjectRotation:0~0~0~0");
            SendEvent($"Action;ObjectID:{LastIndex};ActionType:SetTarget;TargetID:{LastIndex - 2}");
        }

        // Ship controller
        SendEvent($"Create;ObjectType:ShipController;ObjectPosition:0~5~10;ObjectRotation:0~0~0~0");

        // First four panels
        SendEvent($"Create;ObjectType:DoorPanel;ObjectPosition:1.5~1~10;ObjectRotation:{EulerRotation("0~90~0")};AdditionalInfo:{CreateMessageBroken(BreakableGameTask.TaskType.DoorPanel)}");
        SendEvent($"Create;ObjectType:OxyGenerator;ObjectPosition:0.5~1~10;ObjectRotation:{EulerRotation("0~90~0")};AdditionalInfo:{CreateMessageBroken(BreakableGameTask.TaskType.OxyGenerator)}");
        SendEvent($"Create;ObjectType:MotorReactor;ObjectPosition:-0.5~1~10;ObjectRotation:{EulerRotation("0~90~0")};AdditionalInfo:{CreateMessageBroken(BreakableGameTask.TaskType.MotorReactor)}");
        SendEvent($"Create;ObjectType:WireBox;ObjectPosition:-1.5~1~10;ObjectRotation:{EulerRotation("0~90~0")};AdditionalInfo:{CreateMessageBroken(BreakableGameTask.TaskType.WireBox)}");
    }

    void Start()
    {
        eventReceiver = GameObject.FindFirstObjectByType<GameEventReceiverServer>();

        CreateLevel();
    }
}
