using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This class will gather all data for all nodes and edges and save that to an arrow-file.
// The file can later be imported, which lets the user continue the work.

public class ArrowExporter : MonoBehaviour
{
    public EntityManager entityManager;
    public UserInterface ui;

    public void Start()
    {

    }

    public void FillNodeAndEdgeData(List<GameObject> nodeGOs, ref List<Node> nodes, ref List<Connect> connections)
    {
        for (int i = 0; i < nodeGOs.Count; i++)
        {
            nodes.Add(nodeGOs[i].GetComponent<Node>());

            // Update the nodes instance id
            nodes[i].instanceID = nodeGOs[i].GetInstanceID();

            // Update connection instance IDs
            for (int c = 0; c < nodeGOs[i].transform.GetChild(1).childCount; c++)
            {
                Connect con = nodeGOs[i].transform.GetChild(1).GetChild(c).GetComponent<Connect>();
                con.fromInstanceID = con.from.GetInstanceID();
                con.toInstanceID = con.to.GetInstanceID();

                if (con != null)
                {
                    connections.Add(con);
                    connections[c].instanceID = con.GetInstanceID();
                }
            }
        }
    }

    public void ExportSceneDataToJson()
    {
        List<GameObject> nodeGOs = entityManager.entities;
        List<Node> nodes = new List<Node>();
        List<Connect> connections = new List<Connect>();

        FillNodeAndEdgeData(nodeGOs, ref nodes, ref connections);

        string data = "{" + '\n';
        try
        {
            data += "\"vertices\": [" + '\n';

            // Print all nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                //string properties = JsonConvert.SerializeObject(nodes[i].propertiesDict, Formatting.Indented); // Does not work on Web GL

                string properties = fastJSON.JSON.ToNiceJSON(nodes[i].propertiesDict);
                string nodeToJson = JsonUtility.ToJson(nodes[i], true);

                // Write properties manually
                string propertiesObjString = "," + '\n' + '\t' + "\"properties\": ";
                nodeToJson = nodeToJson.Insert(nodeToJson.Length - 2, propertiesObjString + properties);

                //print(nodeToJson);

                data += nodeToJson;

                if (i < nodes.Count - 1)
                    data += ",";
                data += '\n';
            }
            data += @"]"; // + '\n';
        }
        catch
        {
            print("Could not export vertices");
            ui.ShowMessage("Could not export vertices", Color.red);
        }

        try
        {
            //if there is no edges, do not conintue
            if (connections.Count > 0)
            {
                data += @"," + '\n';

                data += '\n' + "\"edges\": [" + '\n';

                for (int i = 0; i < connections.Count; i++)
                {
                    //string properties = JsonConvert.SerializeObject(connections[i].propertiesDict, Formatting.Indented);
                    string properties = fastJSON.JSON.ToNiceJSON(connections[i].propertiesDict);
                    string connectionToJson = JsonUtility.ToJson(connections[i], true);

                    string propertiesObjString = "," + '\n' + '\t' + "\"properties\": ";
                    connectionToJson = connectionToJson.Insert(connectionToJson.Length - 2, propertiesObjString + properties);

                    //print(connectionToJson);

                    data += connectionToJson;

                    if (i < connections.Count - 1)
                        data += @"," + '\n';
                }
                data += @"]" + '\n';
            }
        }
        catch
        {
            print("Could not export edges");
            ui.ShowMessage("Could not export edges: - data: " + data, Color.red);
        }

        data += @"}";

        data = fastJSON.JSON.Beautify(data);

        ui.ShowExportedData(data);

    }

    public void ExportSceneDataToGremlin()
    {
        string data = "";
        GraphsonToGremlin.CosmosDbStructure structure = SceneDataToCosmosDBStructure();
        data = GraphsonToGremlin.ObjectToGremlinConverter.StructureToQueryString(structure);

        ui.ShowExportedData(data);
    }

    public GraphsonToGremlin.CosmosDbStructure SceneDataToCosmosDBStructure()
    {
        List<GameObject> nodeGOs = entityManager.entities;
        List<Node> nodes = new List<Node>();
        List<Connect> connections = new List<Connect>();

        FillNodeAndEdgeData(nodeGOs, ref nodes, ref connections);

        GraphsonToGremlin.CosmosDbStructure structure = new GraphsonToGremlin.CosmosDbStructure();
        structure.vertices = new List<GraphsonToGremlin.Vertex>();
        structure.edges = new List<GraphsonToGremlin.Edge>();

        foreach (var n in nodes)
        {
            GraphsonToGremlin.Vertex v = new GraphsonToGremlin.Vertex();
            v.id = n.instanceID.ToString();
            v.label = n.label;
            v.type = n.label;

            v.property = new GraphsonToGremlin.Properties();
            v.property.properties = new List<GraphsonToGremlin.Property>();

            if (n.propertiesDict != null)
            {
                foreach (var entry in n.propertiesDict)
                {
                    GraphsonToGremlin.Property p = new GraphsonToGremlin.Property();
                    p.id = entry.Key;
                    p.value = entry.Value;
                    v.property.properties.Add(p);
                }
            }

            structure.vertices.Add(v);
        }

        foreach (var c in connections)
        {
            GraphsonToGremlin.Edge e = new GraphsonToGremlin.Edge();
            e.id = c.instanceID.ToString();
            e.label = c.label;
            e.type = c.label;

            e.inV = c.fromInstanceID.ToString();
            e.inVLabel = c.from.GetComponent<Node>().label;
            e.outV = c.toInstanceID.ToString();
            e.outVLabel = c.to.GetComponent<Node>().label;

            if (c.propertiesDict != null)
            {
                foreach (var entry in c.propertiesDict)
                {
                    e.properties.Add(entry.Key, entry.Value);
                }
            }

            structure.edges.Add(e);
        }

        return structure;
    }
}
