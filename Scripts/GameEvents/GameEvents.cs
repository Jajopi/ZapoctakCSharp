using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using System.Globalization;

namespace GameEvents
{
    public struct GameEvent
    {
        public enum EventType
        {
            Connect, Create, Action, Control, Invalid
        }

        static EventType ParseEventType(string eventString)
        {
            if (eventString == "Connect") return EventType.Connect;
            if (eventString == "Create") return EventType.Create;
            if (eventString == "Action") return EventType.Action;
            if (eventString == "Control") return EventType.Control;
            return EventType.Invalid;
        }

        static string EncodeEventType(EventType eventType)
        {
            if (eventType == EventType.Connect) return "Connect";
            if (eventType == EventType.Create) return "Create";
            if (eventType == EventType.Action) return "Action";
            if (eventType == EventType.Control) return "Control";
            return "Invalid";
        }

        public static string StandardizeFloats(string stringWithFloats)
        {
            return stringWithFloats.Replace(',', '.');
        }

        public static Vector3 ParseVector3(string encodedString)
        {
            string[] coordinates = StandardizeFloats(encodedString).Split('~');
            return new Vector3(float.Parse(coordinates[0], CultureInfo.InvariantCulture),
                               float.Parse(coordinates[1], CultureInfo.InvariantCulture),
                               float.Parse(coordinates[2], CultureInfo.InvariantCulture));
        }

        public static string EncodeVector3(Vector3 vector)
        {
            return $"{vector.x}~{vector.y}~{vector.z}";
        }

        public static Quaternion ParseQuaternion(string encodedString)
        {
            string[] coordinates = StandardizeFloats(encodedString).Split('~');
            return new Quaternion(float.Parse(coordinates[0], CultureInfo.InvariantCulture),
                                  float.Parse(coordinates[1], CultureInfo.InvariantCulture),
                                  float.Parse(coordinates[2], CultureInfo.InvariantCulture),
                                  float.Parse(coordinates[3], CultureInfo.InvariantCulture));
        }

        public static string EncodeQuaternion(Quaternion quaternion)
        {
            return $"{quaternion[0]}~{quaternion[1]}~{quaternion[2]}~{quaternion[3]}";
        }

        public static string EulerRotation(string eulerAngles)
        {
            return EncodeQuaternion(Quaternion.Euler(ParseVector3(eulerAngles)));
        }

        /*public static List<TValue> ParseList<TValue>(string listString) where TValue : IParsable<TValue>
        {
            List<TValue> values = new List<TValue>();
            foreach (string rawValue in listString.Split("~"))
            {
                values.Add(TValue.Parse(rawValue));
            }
            return values;
        }*/

        public static List<float> ParseListOfFloats(string listString)
        {
            List<float> values = new List<float>();
            foreach (string rawValue in StandardizeFloats(listString).Split("~"))
            {
                values.Add(float.Parse(rawValue, CultureInfo.InvariantCulture));
            }
            return values;
        }

        public static string EncodeList<TValue>(List<TValue> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (TValue value in list)
            {
                sb.Append(value.ToString());
            }
            return sb.ToString();
        }

        public EventType Type;
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
                if (Type == EventType.Invalid)
                {
                    throw new FormatException("Event type is Invalid");
                }

                bool skipFirst = true;
                foreach (string rawValue in values)
                {
                    string value = rawValue.Trim();
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
                    EventAttributes.Add(keyValuePair[0], value.Substring(keyValuePair[0].Length + 1));
                }
            }
            catch (Exception e)
            {
                Debug.Log(encoding);
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
    public class ObjectTypeDictionary
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

