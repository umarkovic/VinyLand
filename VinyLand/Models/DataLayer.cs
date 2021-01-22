using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace VinyLand.Models
{
    public class DataLayer
    {
        private static GraphClient client;
        private static object objLock = new object();
        

        public static GraphClient GetSession()
        {

            if (client == null)
            {
                lock (objLock)
                {
                    if (client == null)
                    {
                        client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "Bjergsen1!");
                        client.Connect();
                    }
                }
            }

            return client;
        }

        public static int getKey()
        {
            var quey =  new Neo4jClient.Cypher.CypherQuery("MATCH (k:Key) RETURN k.keyValue", new Dictionary<string, object>(), CypherResultMode.Set);
            List<int> key = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(quey).ToList();
            key[0]++;
            var queyy = new Neo4jClient.Cypher.CypherQuery("MATCH (k:Key) SET k.keyValue='"+key[0]+"' RETURN k.keyValue", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(queyy);
            return key[0];
        }

        public static int getKeyOffer()
        {
            var quey = new Neo4jClient.Cypher.CypherQuery("MATCH (k:Key) RETURN k.keyValueOffer", new Dictionary<string, object>(), CypherResultMode.Set);
            List<int> key = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(quey).ToList();
            key[0]++;
            var queyy = new Neo4jClient.Cypher.CypherQuery("MATCH (k:Key) SET k.keyValueOffer='" + key[0] + "' RETURN k.keyValueOffer", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(queyy);
            return key[0];
        }
    }
}