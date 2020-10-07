using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GraphsonToGremlin;

public static class BridgeToGraphson
{
    public static GraphsonToGremlin.Vertex NodeToVertex(Node _node)
    {
        Vertex v = new Vertex();
        v.id = _node.GetInstanceID().ToString();
        v.label = _node.label;
        v.type = _node.type;

        v.property = new Properties();
        v.property.properties = new List<GraphsonToGremlin.Property>();

        foreach (KeyValuePair<string, string> entry in _node.propertiesDict)
        {
            GraphsonToGremlin.Property p = new Property();
            p.id = "";
            p.propertyNames = entry.Key;
            p.value = entry.Value;
            v.property.properties.Add(p);
        }

        return v;
    }

    public static GraphsonToGremlin.Edge ConnectToEdge(Connect _connect)
    {
        Edge e = new Edge();
        e.id = _connect.GetInstanceID().ToString();
        e.label = _connect.label;
        e.type = _connect.type;
        e.inV = _connect.to.GetInstanceID().ToString();
        e.outV = _connect.from.GetInstanceID().ToString();

        e.inVLabel = _connect.to.GetComponent<Node>().label;
        e.outVLabel = _connect.from.GetComponent<Node>().label;

        foreach (KeyValuePair<string, string> entry in _connect.propertiesDict)
        {
            e.properties.Add(entry.Key, entry.Value);
        }

        return e;
    }

    public static CleanSceneData.Vertex ConvertGraphVertexToSceneNode(GraphsonToGremlin.Vertex graphV)
    {
        CleanSceneData.Vertex sceneV = new CleanSceneData.Vertex();

        sceneV.color = UnityEngine.Color.white;
        sceneV.label = graphV.label;
        sceneV.type = graphV.type;
        sceneV.x = UnityEngine.Random.Range(-100, 100);
        sceneV.y = UnityEngine.Random.Range(-100, 100);

        sceneV.instanceID = graphV.id;

        //newV.connections[0].

        sceneV.properties = new Dictionary<string, string>();
        foreach (GraphsonToGremlin.Property p in graphV.property.properties)
        {
            sceneV.properties.Add(p.propertyNames, p.value);
        }

        return sceneV;
    }

    public static CleanSceneData.Edge ConvertGraphEdgeToSceneEdge(GraphsonToGremlin.Edge graphE)
    {
        CleanSceneData.Edge newE = new CleanSceneData.Edge();
        newE.label = graphE.label;
        newE.type = graphE.type;

        newE.instanceID = graphE.id;
        newE.fromInstanceID = graphE.outV;
        newE.toInstanceID = graphE.inV;

        newE.properties = new Dictionary<string, string>();
        if (graphE.properties != null)
        {
            foreach (KeyValuePair<string, string> p in graphE.properties)
            {
                if (p.Key != null && p.Value != null)
                {
                    newE.properties.Add(p.Key, p.Value);
                }
            }
        }
        return newE;
    }

    public static CosmosDbStructure jsonToCosmosDBStructure(string json)
    {
        CosmosDbStructure data = GraphsonToGremlin.JsonToGremlinLoader.GraphsonToObject(json);
        return data;
    }
}