using Sequences;
using System.Collections.Generic;
using UnityEngine;

public class SequenceBlackboardComponent : MonoBehaviour
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public bool AddToDictionary(string key, object value)
    {
        if (!data.ContainsKey(key))
        {
            data.Add(key, value);
            return true;
        }
        return false;
    }
    public void AddToDictionary(string key, object value, bool overrideEntry)
    {
        if (!data.ContainsKey(key))
        {
            data.Add(key, value);
        }
        else if(overrideEntry)
        {
            data[key] = value;
        }
    }

    public T ReadFromDictionary<T>(string key)
    {
        if (data.ContainsKey(key))
        {
            if (data[key] is T)
            {
                return (T)data[key];
            }
        }

        return default(T);
    }
}
