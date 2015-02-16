// DEPRECATED AND SAVED
using Newtonsoft.Json;
using Squid.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Squid.Scroll
{
    public class ScrollStore : GraphObject
    {
        [JsonProperty("UserId")]
        public Guid UserId { get; set; }

        static
        ScrollStore()
        {
        }

        public
        ScrollStore()
        {
            Id = Guid.Empty;
            UserId = Guid.Empty;
        }

        public void Add(Scribe scribe)
        {
            scribe.Id = Guid.NewGuid();
            scribe.UserId = this.UserId;
            scribe.Create();

            scribe.Set("UserId", this.UserId);

            Graph.Instance.Cypher
                .Match("(store:ScrollStore)")
                .Where((ScrollStore store) => store.Id == this.Id)
                .Match("(sc:Scribe)")
                .Where((Scribe sc) => sc.Id == scribe.Id)
                .Create("(store)-[:CONTAINS_SCRIBE]->(sc)")
                .ExecuteWithoutResults();
        }

        public List<Scribe> GetScribes(int limit)
        {
            int count = (int)Graph.Instance.Cypher
                .Match("(store:ScrollStore)")
                .Where((ScrollStore store) => store.Id == this.Id)
                .Match("(store)-[:CONTAINS_SCRIBE]->(sc:Scribe)")
                .Return(sc => sc.Count())
                .Results.First();

            if (count > 0)
            {
                return Graph.Instance.Cypher
                    .Match("(store:ScrollStore)")
                    .Where((ScrollStore store) => store.Id == this.Id)
                    .Match("(store)-[:CONTAINS_SCRIBE]->(sc:Scribe)")
                    .Return(sc => sc.As<Scribe>())
                    .OrderByDescending("sc.CreatedOn")
                    .Limit(limit)                    
                    .Results.ToList();
            }
            else
            {
                return new List<Scribe>(0);
            }            
        }
    }
}
