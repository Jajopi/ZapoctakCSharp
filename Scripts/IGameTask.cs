using System;
using UnityEngine;
using Networking;

namespace GameTasks
{
    interface IGameTask
    {
        string EncodeAction();
        void PerformReceivedAction(ReceivedDataToken receivedDataToken);
    }

    /*interface IServerGameTask : IGameTask
    {
        void Spawn(GameObject spawnPlace);
        string EncodeSpawn();
    }*/
}
