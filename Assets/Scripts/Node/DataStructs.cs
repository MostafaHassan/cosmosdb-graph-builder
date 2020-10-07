using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CleanSceneData
{
    [System.Serializable]
    public class Connection
    {
        [SerializeField] public int instanceID { get; set; }
    }

    [System.Serializable]
    public class Vertex
    {
        [SerializeField] public string type;
        [SerializeField] public string instanceID;
        [SerializeField] public float x;
        [SerializeField] public float y;
        [SerializeField] public Color color;
        [SerializeField] public string label;
        [SerializeField] public List<Connection> connections;
        [SerializeField] public Dictionary<string, string> properties;
    }
    [System.Serializable]
    public class From
    {
        [SerializeField] public int instanceID;
    }
    [System.Serializable]
    public class To
    {
        [SerializeField] public int instanceID;
    }
    [System.Serializable]
    public class Edge
    {
        [SerializeField] public string type;
        [SerializeField] public string instanceID;
        [SerializeField] public string fromInstanceID; //From from;
        [SerializeField] public string toInstanceID; //To to;
        [SerializeField] public string label;
        [SerializeField] public Dictionary<string, string> properties;
    }

    [System.Serializable]
    public class ImportDataStructure
    {
        [SerializeField] public List<Vertex> vertices;
        [SerializeField] public List<Edge> edges;
    }
}