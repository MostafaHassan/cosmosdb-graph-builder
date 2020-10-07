using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using CleanSceneData;

public class ArrowImporter : MonoBehaviour
{
    public EntityManager entityManager;
    public UserInterface ui;
    public SceneCreator sCreator;

    public bool LoadScene(string jsonSceneData)
    {
        bool status = true;

        try
        {
            ImportDataStructure _data = JsonUtility.FromJson<ImportDataStructure>(jsonSceneData);
            
            Dictionary<string, object> parsedObject = new Dictionary<string, object>();

            // Try to look for properties
            try
            {
                //var parsedObject = JObject.Parse(jsonSceneData);
                parsedObject = fastJSON.JSON.Parse(jsonSceneData) as Dictionary<string, object>;

                List<object> vertices = parsedObject["vertices"] as List<object>;

                int i = 0;
                foreach (Dictionary<string, object> v in vertices)
                {
                    string propsString = fastJSON.JSON.ToJSON(v["properties"]);

                    // --
                        // Does not work on Web GL
                    //string propsString = v["properties"].ToString();
                    //Dictionary<string, string> _properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(propsString); 
                    //dynamic _properties = fastJSON.JSON.Parse(propsString);
                    // --

                    Dictionary<string, object> _properties = fastJSON.JSON.Parse(propsString) as Dictionary<string, object>;

                    _data.vertices[i].properties = new Dictionary<string, string>();
                    foreach (var p in _properties)
                    {
                        _data.vertices[i].properties.Add(p.Key, p.Value.ToString());
                    }
                    
                    i++;
                }
            }
            catch
            {
                Debug.Log("Could not find or convert properties to dictionary");
                ui.ShowMessage("Could not find or convert properties to dictionary", Color.red);
            }

            
            try
            {
                //var parsedObject = JObject.Parse(jsonSceneData);
                //Dictionary<string, object> parsedObject = fastJSON.JSON.Parse(jsonSceneData) as Dictionary<string, object>;

                //var edges = parsedObject["edges"];

                List<object> edges = parsedObject["edges"] as List<object>;

                int i = 0;
                foreach (Dictionary<string, object> e in edges)
                {
                    //string propsString = e["properties"].ToString();

                    string propsString = fastJSON.JSON.ToJSON(e["properties"]);

                    //Dictionary<string, string> _properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(propsString);

                    // dynamic
                    Dictionary<string, object> _properties = fastJSON.JSON.Parse(propsString) as Dictionary<string, object>;
                    _data.edges[i].properties = new Dictionary<string, string>();
                    foreach (var p in _properties)
                    {
                        _data.edges[i].properties.Add(p.Key, p.Value.ToString());
                    }

                    i++;
                }
            }
            catch
            {
                print("Could not find or convert properties to dictionary");
            }

            sCreator.GenerateScene(_data);
            
        }
        catch
        {
            Debug.Log("Could not import scene");
            ui.ShowMessage("Could not import scene", Color.red);
            status = false;
        }
        
        return status;
    }
}