using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Networking;

namespace GameEvents
{
    public struct GameEvent
    {
        public enum EventTypes
        {
            Connect, Create, Action, Invalid
        }

        static EventTypes ParseEventType(string eventString)
        {
            if (eventString == "Connect") return EventTypes.Connect;
            if (eventString == "Create") return EventTypes.Create;
            if (eventString == "Action") return EventTypes.Action;
            return EventTypes.Invalid;
        }

        static string EncodeEventType(EventTypes eventType)
        {
            if (eventType == EventTypes.Connect) return "Connect";
            if (eventType == EventTypes.Create) return "Create";
            if (eventType == EventTypes.Action) return "Action";
            return "Invalid";
        }

        public static Vector3 ParseEventPosition(string positionString)
        {
            string[] coordinates = positionString.Split('~');
            return new Vector3(float.Parse(coordinates[0]),
                               float.Parse(coordinates[1]),
                               float.Parse(coordinates[2]));
        }

        public static string EncodePosition(Vector3 position)
        {
            return $"{position.x}~{position.y}~{position.z}";
        }

        public static Quaternion ParseEventRotation(string rotationString)
        {
            string[] coordinates = rotationString.Split('~');
            return new Quaternion(float.Parse(coordinates[0]),
                               float.Parse(coordinates[1]),
                               float.Parse(coordinates[2]),
                               float.Parse(coordinates[3]));
        }

        public static string EncodeRotation(Quaternion rotation)
        {
            return $"{rotation[0]}~{rotation[1]}~{rotation[2]}~{rotation[3]}";
        }

        public EventTypes Type;
        public bool IsTemporary;
        public Dictionary<string, string> EventAttributes;

        public GameEvent(string encoding)
        {
            EventAttributes = new Dictionary<string, string>();
            IsTemporary = false;
            try
            {
                string[] values = encoding.Split(';');
                Type = ParseEventType(values[0]);
                if (Type == EventTypes.Invalid)
                {
                    throw new FormatException("Event type is Invalid");
                }

                bool skipFirst = true;
                foreach (string value in values)
                {
                    if (skipFirst)
                    {
                        skipFirst = false;
                        continue;
                    }

                    if (value.Length == 0)
                    {
                        continue;
                    }

                    if (value == "Temporary")
                    {
                        IsTemporary = true;
                        continue;
                    }

                    string[] keyValuePair = value.Split(":");
                    EventAttributes.Add(keyValuePair[0], keyValuePair[1]);
                }
            }
            catch (Exception e)
            {
                throw new FormatException($"GameEvent has invalid encoding, exception: {e.Message}");
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(EncodeEventType(Type));
            stringBuilder.Append(";");
            if (IsTemporary)
            {
                stringBuilder.Append("Temporary;");
            }
            foreach (string key in EventAttributes.Keys)
            {
                stringBuilder.Append(key);
                stringBuilder.Append(":");
                stringBuilder.Append(EventAttributes[key]);
                stringBuilder.Append(";");
            }

            return stringBuilder.ToString();
        }
    }


    [Serializable]
    class ObjectTypeDictionaryItem
    {
        public string name;
        public GameObject obj;
    }

    [Serializable]
    class ObjectTypeDictionary
    {
        [SerializeField]
        ObjectTypeDictionaryItem[] items;

        public Dictionary<string, GameObject> ToDictionary()
        {
            Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
            foreach (ObjectTypeDictionaryItem item in items)
            {
                dictionary.Add(item.name, item.obj);
            }
            return dictionary;
        }
    }
}

