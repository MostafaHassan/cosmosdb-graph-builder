using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;

public class SceneCreator : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject connectionPrefab;
    public EntityManager em;

    public void Start()
    {
        
    }
    
    public void GenerateScene(CleanSceneData.ImportDataStructure data)
    {
        if (data == null)
            return;

        Dictionary<string, GameObject> nodesToInstanceID = new Dictionary<string, GameObject>();

        if (data.vertices.Any())
        {
            // Iterate through all vertices
            for (int i = 0; i < data.vertices.Count; i++)
            {
                CleanSceneData.Vertex v = data.vertices[i];

                GameObject nodeObj = em.CreateNode(new Vector2(v.x, v.y));
                Node n = nodeObj.GetComponent<Node>();

                n.label = v.label;
                n.propertiesDict = v.properties;
                n.x = v.x;
                n.y = v.y;
                n.color = v.color;

                n.SetVisualLabel();
                n.ShowDialogBubble();

                n.ShowNodeColor();

                bool status = int.TryParse(v.instanceID, out n.instanceID);
                nodesToInstanceID.Add(v.instanceID, nodeObj);
            }
        }

        if (data.edges.Any())
        {
            // Iterate through all edges
            for (int i = 0; i < data.edges.Count; i++)
            {
                CleanSceneData.Edge e = data.edges[i];

                if (nodesToInstanceID.ContainsKey(e.fromInstanceID) && nodesToInstanceID.ContainsKey(e.toInstanceID))
                {
                    GameObject nodeFrom = nodesToInstanceID[e.fromInstanceID];
                    GameObject nodeTo = nodesToInstanceID[e.toInstanceID];

                    LineRenderer line = em.CreateEdge(nodeFrom, nodeTo);
                    Connect lineData = line.GetComponent<Connect>();
                    lineData.from = nodeFrom;
                    lineData.to = nodeTo;
                    lineData.label = e.label;
                    lineData.propertiesDict = e.properties;

                    em.UpdateNewEdge(line, nodeFrom, nodeTo);
                }
                else
                {
                    print("Could not find instance id for the objects connected to this edge");
                }
            }
        }
    }

    public void GenerateScene(GraphsonToGremlin.CosmosDbStructure data)
    {
        print("Generating cosmos scene");
        CleanSceneData.ImportDataStructure newStructure = CosmosDBStructure_TO_ImportDataStructure(data);

        GenerateScene(newStructure);
    }

    public CleanSceneData.ImportDataStructure CosmosDBStructure_TO_ImportDataStructure(GraphsonToGremlin.CosmosDbStructure data)
    {
        CleanSceneData.ImportDataStructure newStructure = new CleanSceneData.ImportDataStructure();
        
        newStructure.vertices = new List<CleanSceneData.Vertex>();
        newStructure.edges = new List<CleanSceneData.Edge>();

        if (data.vertices != null && data.vertices.Any())
        {
            foreach (GraphsonToGremlin.Vertex v in data.vertices)
            {
                CleanSceneData.Vertex newV = BridgeToGraphson.ConvertGraphVertexToSceneNode(v);
                newStructure.vertices.Add(newV);
            }
        }

        if (data.edges != null && data.edges.Any())
        {
            foreach (GraphsonToGremlin.Edge e in data.edges)
            {
                CleanSceneData.Edge newE = BridgeToGraphson.ConvertGraphEdgeToSceneEdge(e);

                newStructure.edges.Add(newE);
            }
        }
        return newStructure;
    }
}