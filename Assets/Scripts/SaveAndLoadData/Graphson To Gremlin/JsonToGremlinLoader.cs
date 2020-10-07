using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphsonToGremlin
{
    public static class JsonToGremlinLoader
    {
        public static CosmosDbStructure GraphsonToObject(string jsonContent)
        {
            CosmosDbStructure data = new CosmosDbStructure();

            data.vertices = new List<Vertex>();
            data.edges = new List<Edge>();

            UserInterface ui = GameObject.Find("Canvas").GetComponent<UserInterface>();

            try
            {
                //dynamic dynJson = fastJSON.JSON.Parse(jsonContent.Trim());
                List<object> dynJson = fastJSON.JSON.Parse(jsonContent) as List<object>;

                foreach (var items in dynJson)
                {
                    Dictionary<string, object> parsedObject = items as Dictionary<string, object>;

                    foreach (var item in parsedObject)
                    {
                        var key = item.Key.ToString();
                        var val = item.Value.ToString();

                        //return data;
                        //Debug.Log("key: " + item.Key.ToString());
                        //Debug.Log("val: " + item.Value.ToString());

                        try
                        {
                            if (key == "type" && val == "vertex")
                            {
                                // Iterate through items and save data to vertex...   
                                // Creating vertex and save to vertex list
                                try
                                {
                                    Vertex v = new Vertex();
                                    foreach (var vertexItem in parsedObject)
                                    {
                                        if (vertexItem.Key == "id")
                                            v.id = vertexItem.Value.ToString();

                                        if (vertexItem.Key == "label")
                                            v.label = vertexItem.Value.ToString();

                                        if (vertexItem.Key == "type")
                                            v.type = vertexItem.Value.ToString();

                                        
                                        v.property = new Properties();
                                        if (vertexItem.Key == "properties")
                                        {
                                            v.property.properties = new List<Property>();

                                            Dictionary<string, object> _properties = vertexItem.Value as Dictionary<string, object>;

                                            v.property.properties = ReadProperties(ref _properties);
                                        }
                                        
                                    }

                                    data.vertices.Add(v);
                                }
                                catch
                                {
                                    Debug.Log("Could not parse vertex");
                                    ui.ShowMessage("Could not parse vertex", Color.red);
                                }

                            }
                            else if (key == "type" && val == "edge")
                            {
                                // Create edge and save to edge list
                                try
                                {
                                    Edge e = new Edge();
                                    foreach (var edgeItem in parsedObject)
                                    {
                                        if (edgeItem.Key == "id")
                                            e.id = edgeItem.Value.ToString();

                                        else if (edgeItem.Key == "label")
                                            e.label = edgeItem.Value.ToString();

                                        else if (edgeItem.Key == "type")
                                            e.type = edgeItem.Value.ToString();

                                        else if (edgeItem.Key == "inVLabel")
                                            e.inVLabel = edgeItem.Value.ToString();

                                        else if (edgeItem.Key == "outVLabel")
                                            e.outVLabel = edgeItem.Value.ToString();

                                        else if (edgeItem.Key == "inV")
                                            e.inV = edgeItem.Value.ToString();

                                        else if (edgeItem.Key == "outV")
                                            e.outV = edgeItem.Value.ToString();

                                        else if (edgeItem.Key == "properties")
                                        {
                                            Dictionary<string, object> _properties = edgeItem.Value as Dictionary<string, object>;

                                            foreach (object pp in _properties)
                                            {
                                                KeyValuePair<string, object> currentProp = (KeyValuePair<string, object>)pp;
                                                
                                                e.properties.Add(currentProp.Key, currentProp.Value.ToString());
                                            }
                                            
                                            //e.property.properties = ReadProperties(ref _properties);
                                        }
                                    }
                                    data.edges.Add(e);
                                }
                                catch
                                {
                                    Debug.Log("Could not parse edge");
                                    ui.ShowMessage("Could not parse edge", Color.red);
                                }
                            }
                        }

                        catch
                        {
                            Debug.Log("Something went when converting the following item: ");
                            Debug.Log(item.ToString());
                            Debug.Log("The data exported will probably include errors, would you like to try to procceed with the loading?: ");

                            ui.ShowMessage("Something went when converting the following item:" + item.ToString(), Color.red);
                            break;
                        }
                    }
                }
            }
            catch
            {
                Debug.LogError("Something went wrong when parsing json");
                ui.ShowMessage("Something went wrong when parsing json", Color.red);
                return new CosmosDbStructure();
            }

            ui.ShowMessage("Successfully parsed data", new Color(0, 0.5f, 0.02f));
            return data;
        }

        public static List<Property> ReadProperties(ref Dictionary<string, object> _properties)
        {
            List<Property> listOfProps = new List<Property>();
            foreach (var _prop in _properties)
            {
                // Property name
                string propKey = _prop.Key;
                List<object> propValueTemp = _prop.Value as List<object>;

                if (propValueTemp.Count <= 0)
                    continue;

                Dictionary<string, object> propValue = (propValueTemp[0]) as Dictionary<string, object>;

                Property p = new Property();
                p.propertyNames = propKey;
                foreach (object pp in propValue)
                {
                    KeyValuePair<string, object> currentProp = (KeyValuePair<string, object>)pp;
                    string _key = currentProp.Key;
                    string _val = currentProp.Value.ToString();

                    if (currentProp.Key == "id")
                    {
                        p.id = currentProp.Value.ToString();
                    }
                    if (currentProp.Key == "value")
                    {
                        p.value = currentProp.Value.ToString();
                    }
                }

                listOfProps.Add(p);
            }

            return listOfProps;
        }
    }
}
