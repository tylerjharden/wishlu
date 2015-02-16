using Schloss.Data.Neo4j;
using Squid.Log;
using System;
using System.Net;

namespace Squid.Database
{
    public static class Graph
    {
        // private reference to the Neo4jClient GraphClient
        private static GraphClient client;
        
        // public facing reference to the Neo4jClient GraphClient
        public static GraphClient Instance
        {
            get 
            {
                if (client == null)
                    Initialize(); // if the client hasn't been initialized and set, initialize it

                return client; 
            }
        }
                
        static Graph()
        {
            // initialize the database when the Graph class is accessed for the first time
            Initialize();            
        }

        static void Initialize()
        {
            try
            {
                Logger.Log("Initializing graph database...");

                // optimizations to improve Neo4jClient throughput and 
                ServicePointManager.UseNagleAlgorithm = false; // can reduce up to 100ms delay induced by unnecessary packet waiting (see: http://romikoderbynew.com/2012/01/17/slow-httpwebrequest-getresponse/)
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.DefaultConnectionLimit = 200;

                // create a definite reference to the HttpClient
                HttpClientWrapper clienthttp = new HttpClientWrapper();
                                
                // wishlu neo4j hosted at 10.0.0.30, neo4j database port is always 7474 in our design, pass in our HttpClient
                client = new GraphClient(new Uri("http://10.0.0.30:7474/db/data"), clienthttp);
                client.Connect();

                client.OperationCompleted += client_OperationCompleted;
                
                Logger.Log("Connected to graph database!");                                
            }
            catch (Exception ex)
            {
                // log any exceptions, reset client to null if an error occurred
                Logger.Error(ex);
                client = null;                
            }
        }

        static void client_OperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            //Logger.Log("Graph Operation Completed\n" + e.QueryText + "\nResources Returned: " + e.ResourcesReturned + "\nTime: " + e.TimeTaken.Milliseconds + "ms");
        }
    }
}
