using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squid.Database;
using Squid.Housekeeping;
using Squid.Log;
using Squid.Users;
using Squid.Wishes;

namespace Fantine
{
    public static class WishManager
    {
        public static List<RevealTask> RevealTasks { get; set; }
        private static DateTime LastFetch { get; set; }

        static WishManager()
        {
            RevealTasks = new List<RevealTask>();

            try
            {
                FetchRevealTasks();
            }
            catch { }
        }

        public static bool ShouldFetch()
        {
            try
            {
                return (DateTime.Now - LastFetch).Seconds >= 5;
            }
            catch { return false; }
        }

        public static void Poll()
        {
            if (ShouldFetch())
                FetchRevealTasks();
            
            foreach (RevealTask rt in RevealTasks)
            {
                try
                {
                    Logger.Log("Fantine is revealing a gift! (Id: " + rt.GiftId + ")");

                    Gift g = Gift.GetGiftById(rt.GiftId);
                    //Wish w = g.GetWish();
                    //User u = g.GetGiver();

                    g.Reveal();
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
                finally
                {
                    Graph.Instance.Cypher
                        .Match("(task:RevealTask" + DateTime.Now.ToString("MMddyy") + ")")
                        .Where((RevealTask task) => task.GiftId == rt.GiftId)
                        .Delete("task")
                        .ExecuteWithoutResults();
                }                
            }

            RevealTasks.Clear();
        }

        public static void FetchRevealTasks()
        {
            try
            {
                
                List<RevealTask> results = Graph.Instance.Cypher
                    .Match("(task:RevealTask" + DateTime.Now.ToString("MMddyy") + ")")
                    .Return(task => task.As<RevealTask>())
                    .Results.ToList();

                if (results.Count > 0)
                {
                    RevealTasks.AddRange(results);
                    Logger.Log("Fantine has retrieved " + results.Count + " reveal tasks.");
                }                                
            }
            catch { }
            finally { LastFetch = DateTime.Now; }
        }
    }       
    
}
