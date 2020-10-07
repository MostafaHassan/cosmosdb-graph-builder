using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity;

public static class DataUtility
{
    public static bool UpdatePropertiesFromText(ref Dictionary<string, string> propertiesDict, string propsToJson, ref string properties)
    {
        if (propsToJson == "")
        {
            propertiesDict.Clear();
            return true;
        }
        var lines = propsToJson.Split('\n');

        Dictionary<string, string> myNewProperties = new Dictionary<string, string>();

        bool succeededWithConvert = true;
        try
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                var keyValuePair = lines[i].Split(':');
                string key = keyValuePair[0].Trim();
                string value = keyValuePair[1].Trim();

                myNewProperties.Add(key, value);
            }
        }
        catch
        {
            UnityEngine.Debug.Log("Could not convert the properties to a dictionary");
            succeededWithConvert = false;
        }

        if (succeededWithConvert)
        {
            propertiesDict = myNewProperties;
        }

        SavePropertiesToJson(propertiesDict, ref properties);
        return succeededWithConvert;
    }

    public static string GetPropertiesAsString(Dictionary<string, string> propertiesDict, ref string properties)
    {
        string props = "";
        if (propertiesDict == null)
            return "";

        foreach (KeyValuePair<string, string> entry in propertiesDict)
        {
            props += entry.Key.Trim() + ": " + entry.Value.Trim() + "\n";
        }

        SavePropertiesToJson(propertiesDict, ref properties);
        return props;
    }

    public static string SavePropertiesToJson(Dictionary<string, string> propertiesDict, ref string properties)
    {
        string json = @"";
        if (propertiesDict == null)
            return "";

        //json = "{\n";
        json = @"{";

        int count = 0;

        foreach (KeyValuePair<string, string> entry in propertiesDict)
        {
            json += '\n' + "\"";
            json += entry.Key.Trim() + "\"" + ": ";
            json += "\"" + entry.Value.Trim() + "\"";

            if (count < propertiesDict.Count - 1)
                json += @",";
            //json += "\n";

            count++;
        }
        json.Remove(json.Length - 1);
        json += @"}";


        properties = json;
        return json;
    }
}