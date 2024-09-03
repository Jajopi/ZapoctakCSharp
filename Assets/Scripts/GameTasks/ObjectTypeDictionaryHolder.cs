using UnityEngine;
using System.Collections.Generic;
using GameEvents;

public class ObjectTypeDictionaryHolder : MonoBehaviour
{
    public ObjectTypeDictionary ObjectTypeDictionary;

    public Dictionary<string, GameObject> GetDictionary()
    {
        return ObjectTypeDictionary.ToDictionary();
    }
}
