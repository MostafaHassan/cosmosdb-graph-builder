using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System;

[System.Serializable]
public class Connect : MonoBehaviour
{
    public enum EdgeTypes
    {
        Select,
        Has,
        Requires,
        Owns,
        Permits,
        Permission_to,
        Registered_for,
        Registered_as,
    }

    [SerializeField]
    public string type;

    [SerializeField]
    public int instanceID;

    [SerializeField]
    public int fromInstanceID;

    [SerializeField]
    public int toInstanceID;

    [SerializeField]
    public string label;


    public Dictionary<string, string> propertiesDict;

    private string properties;

    [NonSerialized]
    public GameObject from;
    [NonSerialized]
    public GameObject to;

    //public Dictionary<string, string> properties;
    public int lineIndex = 1;

    private void Awake()
    {
        propertiesDict = new Dictionary<string, string>();
    }

    public string GetProperties()
    {
        return properties;
    }

    public void SetProperties(string p)
    {
        properties = p;
    }

    public ref string GetPropertiesREF()
    {
        return ref properties;
    }

    void Start()
    {
        type = "edge";
        instanceID = GetInstanceID();
    }

    public static int FindValueInEnumEdgeTypes(string val)
    {
        var values = EdgeTypes.GetValues(typeof(EdgeTypes));
        for (int x = 0; x < values.Length; x++)
        {
            if (values.GetValue(x).ToString().ToLower() == val.ToLower())
            {
                return x;
            }
        }
        return -1;
    }

    public void SetNextEdgeType()
    {
        // Set caption to next edge type. 
        // Cirulates at last element and jumps over index 0
        int index = FindValueInEnumEdgeTypes(label);
        if (index != -1)
        {
            index++;
            index %= EdgeTypes.GetValues(typeof(EdgeTypes)).Length;
            index = index == 0 ? index + 1 : index;

            EdgeTypes newEdgeType = (EdgeTypes)index;
            label = newEdgeType.ToString().ToLower();
        }
        else
        {
            // If caption could not be found in types set to first
            int firstInEdgeTypes = 1;
            label = ((EdgeTypes)firstInEdgeTypes).ToString().ToLower();
        }
    }
}
