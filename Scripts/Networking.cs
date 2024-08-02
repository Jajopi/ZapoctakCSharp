using UnityEngine;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Text;
using System.IO;
using System.Collections.Generic;

using LockFreeAsyncDataStorage;

namespace Networking
{
    public struct ReceivedDataToken
    {
        public string Value;

        public override string ToString()
        {
            return Value;
        }
    }


    interface INetworkSender
    {
        void SetTarget(Uri targetAddress);
        void SendData(object data);
        void SendData(object data, Uri targetAddress);
    }

    interface INetworkReceiver
    {
        void StartReceiving();
        void EndReceiving();

        bool IsReceiving();
        virtual Uri GetReceivingAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                throw new IOException("Network unavailable.");
            }
            
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress address in ipEntry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return new Uri(address.ToString());
                }
            }
            throw new IOException("Address not found.");

            //throw new NotImplementedException("Global IP address search not supported.");
        }

        bool IsDataReady();
        ReceivedDataToken GetNextReceivedData();
    }


    public class SimpleSender : INetworkSender
    {
        Uri target = null;
        HttpClient targetClient = new HttpClient();

        public void SetTarget(Uri targetAddress)
        {
            target = targetAddress;
        }

        public void SendData(object data, Uri targetAddress)
        {
            Debug.Log($"Sending {data} to {targetAddress}.");
            targetClient.PostAsync(targetAddress, new StringContent(data.ToString()));
        }

        public void SendData(object data)
        {
            if (target is null)
            {
                throw new InvalidOperationException("Target address was not specified.");
            }

            SendData(data, target);
        }
    }


    public class SimpleReceiver : INetworkReceiver
    {
        Thread listenerThread;
        HttpListener listener;
        string prefix;

        IDataStorage<ReceivedDataToken> storage;
        IDataStoragePusher<ReceivedDataToken> storagePusher;
        IDataStoragePopper<ReceivedDataToken> storagePopper;

        public SimpleReceiver(string address, bool isStorageRewritable=false)
        {
            prefix = address;

            if (isStorageRewritable)
            {
                storage = new RewritableDataStorage<ReceivedDataToken>();
            }
            else
            {
                storage = new ConstantCapacityDataStorage<ReceivedDataToken>(10);
            }

            storagePusher = storage.Pusher;
            storagePopper = storage.Popper;
        }

        ReceivedDataToken CreateTokenFromContext(HttpListenerContext context)
        {
            Stream stream = context.Request.InputStream;
            StreamReader reader = new(stream, context.Request.ContentEncoding);
            return new ReceivedDataToken { Value = reader.ReadToEnd() };
        }

        void Listen()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8001/");
            listener.Start();

            Debug.Log("Receiving started");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ReceivedDataToken token = CreateTokenFromContext(context);
                context.Response.Close();

                storagePusher.PushData(token);
            }
        }

        public void StartReceiving()
        {
            listenerThread = new Thread(Listen);
            listenerThread.Start();
        }

        public void EndReceiving()
        {
            if (listenerThread is null)
            {
                throw new InvalidOperationException($"The listener {this.ToString()} was not started yet.");
            }
            else if (! listenerThread.IsAlive)
            {
                throw new InvalidOperationException($"The listener thread of {this.ToString()} was already aborted.");
            }
            listenerThread.Abort();
        }

        public bool IsReceiving()
        {
            if (listenerThread is null) return false;
            return listenerThread.IsAlive;
        }

        public bool IsDataReady()
        {
            return storagePopper.IsReady();
        }

        public ReceivedDataToken GetNextReceivedData()
        {
            return storagePopper.PopData();
        }
    }
}
