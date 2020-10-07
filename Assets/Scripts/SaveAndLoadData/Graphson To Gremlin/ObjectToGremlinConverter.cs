using System;
using System.Collections.Generic;
using System.Text;

namespace GraphsonToGremlin
{
    public static class ObjectToGremlinConverter
    {
        public static List<string> StructureToQueryList(CosmosDbStructure db)
        {
            List<string> commands = new List<string>();
            foreach(Vertex v in db.vertices)
            {
                string command = VertexToQuery(v);
                commands.Add(command);
            }

            foreach (Edge e in db.edges)
            {
                string command = EdgeToQuery(e);
                commands.Add(command);
            }
            return commands;
        }

        public static string StructureToQueryString(CosmosDbStructure db)
        {
            string commands = "";
            foreach (Vertex v in db.vertices)
            {
                string command = VertexToQuery(v);
                commands += command + '\n';
            }

            foreach (Edge e in db.edges)
            {
                string command = EdgeToQuery(e);
                commands += command + '\n';
            }
            return commands;
        }

        public static List<string> JsonToQueryList(string json)
        {
            CosmosDbStructure structure = JsonToGremlinLoader.GraphsonToObject(json);
            List<string> commands = StructureToQueryList(structure);
            return commands;
        }

        public static string JsonToQueryString(string json)
        {
            CosmosDbStructure structure = JsonToGremlinLoader.GraphsonToObject(json);
            string commands = StructureToQueryString(structure);
            return commands;
        }

        public static string VertexToQuery(Vertex v)
        {
            string command = "g.addV('" + v.label +
                "')";

            if (v.id != null && v.id != "")
            {
                command += ".property('id', '" + v.id + "')";
            }

            foreach(Property prop in v.property.properties)
            {
                if (prop.propertyNames != null && prop.propertyNames != "")
                {
                    command += ".property('" + prop.propertyNames + "', '" + prop.value + "')";
                }
                else
                {
                    command += ".property('" + prop.id + "', '" + prop.value + "')";
                }
            }
            
            return command;
        }
        public static string EdgeToQuery(Edge e)
        {
            string command =
                "g.V().has('id', '" + e.outV + "')" + ".addE('" +
                e.label + "').to(g.V().has('id', '" + e.inV + "'))";

            if (e.id != null && e.id != "")
                command += ".property('id', '" + e.id + "')";

            foreach (KeyValuePair<string, string> prop in e.properties)
            {
                command += ".property('" + prop.Key + "', '" + prop.Value + "')";
            }

            //g.addV('person').property('id', 'id here...').property('name', 'Mostafa')
            return command;
        }
    }
}
