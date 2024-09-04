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
    public struct DataToken
    {
        const string SEPARATOR = "|";

        public DateTime CreationTime;
        public string Value;

        public override string ToString()
        {
            return CreationTime.ToString() + SEPARATOR + Value;
        }

        public DataToken(string tokenString)
        {
            string[] timeAndValue = tokenString.Split(SEPARATOR);
            if (timeAndValue.Length == 1)
            {
                Value = tokenString;
                CreationTime = DateTime.Now;
            }
            else
            {
                CreationTime = DateTime.Parse(timeAndValue[0]);
                Value = timeAndValue[1];
            }
        }
    }


    interface INetworkSender
    {
        void SetTarget(Uri targetAddress);
        void SendData(DataToken data);
        void SendData(DataToken data, Uri targetAddress);
    }

    interface INetworkReceiver
    {
        void StartReceiving();
        void EndReceiving();

        bool IsReceiving();
        Uri GetReceivingAddress();

        bool IsDataReady();
        DataToken GetNextReceivedData();
    }


    public static class NetworkHelperFunctions
    {
        static string GetHttpPrefix(bool secure)
        {
            if (secure) return "https://";
            return "http://";
        }

        public static Uri GetReceivingAddress(int portNumber, bool local = true, bool secureHttp = false)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                throw new IOException("Network unavailable.");
            }

            if (local)
            {
                IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress address in ipEntry.AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return new Uri(GetHttpPrefix(secureHttp) + address + ":" + portNumber.ToString());
                    }
                }
                throw new IOException("Address not found.");
            }
            else
            {
                string address = new WebClient().DownloadString("http://icanhazip.com").Trim();
                return new Uri(GetHttpPrefix(secureHttp) + address + ":" + portNumber.ToString());
            }
        }

        public static string GetAddressInFormatForListener(Uri address)
        {
            string[] adressParts = address.ToString().Split(":");
            return $"{adressParts[0]}://*:{adressParts[2]}";
        }

        public static Uri GetUriFromAddress(string address, bool secureHttp = false)
        {
            return new Uri(GetHttpPrefix(secureHttp) + address);
        }

        public static int GetRandomFreePort()
        {
            bool IsPortFree(int portNumber)
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                
                foreach (IPEndPoint listener in properties.GetActiveTcpListeners())
                {
                    if (listener.Port == portNumber)
                    {
                        return false;
                    }
                }
                foreach (IPEndPoint listener in properties.GetActiveUdpListeners())
                {
                    if (listener.Port == portNumber)
                    {
                        return false;
                    }
                }

                return true;
            }

            int portNumber = 8000;
            while (!IsPortFree(portNumber))
            {
                portNumber++;
            }
            return portNumber;
        }
    }


    public class SimpleSender : INetworkSender
    {
        Uri target = null;
        HttpClient targetClient = new HttpClient();

        public void SetTarget(Uri targetAddress)
        {
            target = targetAddress;
        }

        public void SendData(DataToken data, Uri targetAddress)
        {
            targetClient.PostAsync(targetAddress, new StringContent(data.ToString()));
        }

        public void SendData(DataToken data)
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
        Uri address;

        IDataStorage<DataToken> storage;
        IDataStoragePusher<DataToken> storagePusher;
        IDataStoragePopper<DataToken> storagePopper;

        public SimpleReceiver(Uri receivingAddress)
        {
            address = receivingAddress;

            storage = new ConstantCapacityDataStorage<DataToken>(100);

            storagePusher = storage.Pusher;
            storagePopper = storage.Popper;
        }

        DataToken CreateTokenFromContext(HttpListenerContext context)
        {
            Stream stream = context.Request.InputStream;
            StreamReader reader = new(stream, context.Request.ContentEncoding);
            return new DataToken(reader.ReadToEnd());
        }

        void PushDataIntoStorage(DataToken token)
        {
            storagePusher.PushData(token);
        }

        void Listen()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(NetworkHelperFunctions.GetAddressInFormatForListener(address));
            listener.Start();

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                DataToken token = CreateTokenFromContext(context);
                context.Response.StatusCode = 200;
                context.Response.Close();

                PushDataIntoStorage(token);
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

        public Uri GetReceivingAddress()
        {
            return address;
        }

        public bool IsDataReady()
        {
            return storagePopper.IsReady();
        }

        public DataToken GetNextReceivedData()
        {
            return storagePopper.PopData();
        }
    }
}
