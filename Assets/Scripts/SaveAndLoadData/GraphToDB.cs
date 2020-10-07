using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
//using Newtonsoft.Json;

public static class GraphToDB
{
    public static async Task UploadToCosmosDB(string hostname, int port, string authKey, string database, string collection, GraphsonToGremlin.CosmosDbStructure structure, bool dropData = false)
    {
        try
        {
            var gremlinServer = new GremlinServer(hostname, port, enableSsl: true,
                                                    username: "/dbs/" + database + "/colls/" + collection,
                                                    password: authKey);
        var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType);

        
            Dictionary<string, string> instanceIDToDBID = new Dictionary<string, string>();

            if(dropData)
            {
                Debug.Log("Dropping data");
                var result = await gremlinClient.SubmitWithSingleResultAsync<dynamic>("g.V().drop()");
            }

            foreach (var v in structure.vertices)
            {
                // Removing Unitys id from all vertices in order for CosmosDB to create IDs for us instead
                Debug.Log(String.Format("Running this query: {0}", v.ToString()));
                GraphsonToGremlin.Vertex cloneV = v;
                cloneV.id = "";
                string query = GraphsonToGremlin.ObjectToGremlinConverter.VertexToQuery(cloneV);

                //var resultSet = SubmitRequest(gremlinClient, query).Result;
                try
                {
                    // Create async task to execute the Gremlin query.
                    var resultSet = await gremlinClient.SubmitWithSingleResultAsync<dynamic>(query);
                    string vID = resultSet["id"];

                    instanceIDToDBID.Add(v.id, vID);
                    //Dictionary<int, GameObject> instanceIDs = UnityToGenerealUtility.GetEntitiesInstanceIDs();
                }
                catch
                {
                    Debug.LogError("Could not submit request");
                    GameObject.Find("Canvas").GetComponent<UserInterface>().ShowMessage("Could not submit request", Color.red);
                    return;
                }
            }

            foreach (var e in structure.edges)
            {
                // Removing Unitys id from all edges in order for CosmosDB to create IDs for us instead
                Debug.Log(String.Format("Running this query: {0}", e.ToString()));
                GraphsonToGremlin.Edge cloneE = e;
                cloneE.id = "";
                cloneE.inV = instanceIDToDBID[cloneE.inV];
                cloneE.outV = instanceIDToDBID[cloneE.outV];
                string query = GraphsonToGremlin.ObjectToGremlinConverter.EdgeToQuery(cloneE);

                try
                {
                    // Create async task to execute the Gremlin query.
                    var resultSet = await gremlinClient.SubmitWithSingleResultAsync<dynamic>(query);
                    //string eID = resultSet["id"];
                }
                catch
                {
                    Debug.LogError("Could not submit request");
                    GameObject.Find("Canvas").GetComponent<UserInterface>().ShowMessage("Could not submit request", Color.red);
                    return;
                }
            }

            GameObject.Find("Canvas").GetComponent<UserInterface>().ShowMessage("Successfully updated database", new Color(0, 0.5f, 0.02f));
            return;
        }
        catch
        {
            GameObject.Find("Canvas").GetComponent<UserInterface>().ShowMessage("Could not submit request", Color.red);
            return;
        }
    }

    public static bool UploadToCosmosDB(string hostname, int port, string authKey, string database, string collection, List<string> gremlinQueries)
    {
        var gremlinServer = new GremlinServer(hostname, port, enableSsl: true,
                                                    username: "/dbs/" + database + "/colls/" + collection,
                                                    password: authKey);
        var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType);

        try
        {
            foreach (var query in gremlinQueries)
            {
                Debug.Log(String.Format("Running this query: {0}", query));

                // Create async task to execute the Gremlin query.
                var resultSet = SubmitRequest(gremlinClient, query).Result;
                try
                {
                    if (resultSet.Count > 0)
                    {
                        Debug.Log("\tResult:");
                        foreach (var result in resultSet)
                        {
                            // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                            string output = fastJSON.JSON.ToNiceJSON(result);
                            Debug.Log($"\t{output}");
                        }
                        Debug.Log("");
                    }

                    // Print the status attributes for the result set.
                    // This includes the following:
                    //  x-ms-status-code            : This is the sub-status code which is specific to Cosmos DB.
                    //  x-ms-total-request-charge   : The total request units charged for processing a request.
                    PrintStatusAttributes(resultSet.StatusAttributes);
                    Debug.Log("");
                }
                catch
                {
                    Debug.LogError("Could not submit request");
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Task<ResultSet<object>> SubmitRequest(GremlinClient gremlinClient, string query)
    {
        try
        {
            return gremlinClient.SubmitAsync<object>(query);
        }
        catch (ResponseException e)
        {
            Console.WriteLine("\tRequest Error!");

            // Print the Gremlin status code.
            Debug.LogError($"\tStatusCode: {e.StatusCode}");

            // On error, ResponseException.StatusAttributes will include the common StatusAttributes for successful requests, as well as
            // additional attributes for retry handling and diagnostics.
            // These include:
            //  x-ms-retry-after-ms         : The number of milliseconds to wait to retry the operation after an initial operation was throttled. This will be populated when
            //                              : attribute 'x-ms-status-code' returns 429.
            //  x-ms-activity-id            : Represents a unique identifier for the operation. Commonly used for troubleshooting purposes.
            PrintStatusAttributes(e.StatusAttributes);
            Debug.LogError($"\t[\"x-ms-retry-after-ms\"] : { GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms")}");
            Debug.LogError($"\t[\"x-ms-activity-id\"] : { GetValueAsString(e.StatusAttributes, "x-ms-activity-id")}");

            return null;
            //throw;
        }
    }

    private static void PrintStatusAttributes(IReadOnlyDictionary<string, object> attributes)
    {
        Debug.Log($"\tStatusAttributes:");
        Debug.Log($"\t[\"x-ms-status-code\"] : { GetValueAsString(attributes, "x-ms-status-code")}");
        Debug.Log($"\t[\"x-ms-total-request-charge\"] : { GetValueAsString(attributes, "x-ms-total-request-charge")}");
    }

    public static string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key)
    {
        return fastJSON.JSON.ToJSON(GetValueOrDefault(dictionary, key));
    }

    public static object GetValueOrDefault(IReadOnlyDictionary<string, object> dictionary, string key)
    {
        if (dictionary.ContainsKey(key))
        {
            return dictionary[key];
        }

        return null;
    }
}