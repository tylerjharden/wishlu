using Newtonsoft.Json;
using Squid.Database;
using Squid.Log;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Squid.Messages
{
    public class Comment : GraphObject
    {
        [JsonProperty("AuthorId")]
        public Guid AuthorId { get; set; }

        [JsonProperty("Body")]
        public String Body { get; set; }

        public Comment()
        {
            Id = Guid.Empty;
            AuthorId = Guid.Empty;
            Body = String.Empty;
            
            Logger.Log("new Comment()");
        }

        public void Like(Guid userId)
        {
            Graph.Instance.Cypher
            .Match("(user:User)")
            .Where((User user) => user.Id == userId)
            .Match("(com:Comment)")
            .Where((Comment com) => com.Id == this.Id)
            .CreateUnique("(user)-[:LIKES]->(com)")
            .ExecuteWithoutResults();
        }

        public void Unlike(Guid userId)
        {
            Graph.Instance.Cypher
            .Match("(user:User)-[r:LIKES]->(com:Comment)")
            .Where((User user) => user.Id == userId)
            .AndWhere((Comment com) => com.Id == this.Id)
            .Delete("r")
            .ExecuteWithoutResults();
        }

        public List<User> GetLikes()
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(user:User)-[:LIKES]->(com:Comment)")
                    .Where((Comment com) => com.Id == this.Id)
                    .Return(user => user.As<User>())
                     .Results.ToList();
            }
            catch
            {
                return new List<User>();
            }
        }
    }
}
