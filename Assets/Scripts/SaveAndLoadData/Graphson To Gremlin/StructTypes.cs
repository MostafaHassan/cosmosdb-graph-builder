using System;
using System.Collections.Generic;
using System.Text;

namespace GraphsonToGremlin
{
    public struct Property
    {
        public string id;
        public string propertyNames;
        public string value;
    }
    public struct Properties
    {
        //public string propertyName;
        public List<Property> properties;
    }

    public struct Vertex
    {
        public string id;
        public string label;
        public string type;
        public Properties property;
    };

    public class Edge
    {
        public string id;
        public string label;
        public string type;
        public string edge;
        public string inVLabel;
        public string outVLabel;
        public string inV;
        public string outV;
        public Dictionary<string, string> properties;

        public Edge()
        {
            properties = new Dictionary<string, string>();
        }
    }

    public struct CosmosDbStructure
    {
        public List<Vertex> vertices;
        public List<Edge> edges;
        public bool loaded;
    }
}
