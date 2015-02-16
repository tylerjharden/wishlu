// DEPRECATED AND SAVED
using Newtonsoft.Json;
using Squid.Database;
using Squid.Messages;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Squid.Scroll
{
    public class Scribe : GraphObject
    {
        [JsonProperty("UserId")]
        public Guid UserId { get; set; }

        [JsonProperty("Content")]
        public string Content { get; set; }

        [JsonProperty("ScribeType")]
        public string ScribeType { get; set; }

        public static Scribe GetScribeById(Guid id)
        {
            return Graph.Instance.Cypher
            .Match("(sc:Scribe)")
            .Where((Scribe sc) => sc.Id == id)
            .Return(sc => sc.As<Scribe>())
            .Results.First();
        }

        public void Like(Guid userId)
        {
            Graph.Instance.Cypher
            .Match("(user:User)")
            .Where((User user) => user.Id == userId)
            .Match("(sc:Scribe)")
            .Where((Scribe sc) => sc.Id == this.Id)
            .CreateUnique("(user)-[:LIKES]->(sc)")            
            .ExecuteWithoutResults();
        }

        public void Unlike(Guid userId)
        {
            Graph.Instance.Cypher
            .Match("(user:User)-[r:LIKES]->(sc:Scribe)")
            .Where((User user) => user.Id == userId)            
            .AndWhere((Scribe sc) => sc.Id == this.Id)
            .Delete("r")
            .ExecuteWithoutResults();
        }

        public List<User> GetLikes()
        {           
            int count = (int)Graph.Instance.Cypher
                .Match("(user:User)-[:LIKES]->(sc:Scribe)")
                .Where((Scribe sc) => sc.Id == this.Id)
                .Return(user => user.Count())
                .Results.First();

            if (count > 0)
            {
                return Graph.Instance.Cypher
                     .Match("(user:User)-[:LIKES]->(sc:Scribe)")
                    .Where((Scribe sc) => sc.Id == this.Id)
                    .Return(user => user.As<User>())
                     .Results.ToList();
            }
            else
            {
                return new List<User>();
            }
        }

        public void AddComment(Guid userId, string comment)
        {
            Comment com = new Comment();
            com.AuthorId = userId;
            com.Body = comment;
            com.Id = Guid.NewGuid();
            
            com.Create();

            Guid comid = com.Id;

            Graph.Instance.Cypher
                .Match("(user:User)")
                .Where((User user) => user.Id == userId)
                .Match("(comm:Comment)")
                .Where((Comment comm) => comm.Id == comid)
                .CreateUnique("(user)-[:AUTHORED]->(comm)")
                .ExecuteWithoutResults();

            Graph.Instance.Cypher
                .Match("(sc:Scribe)")
                .Where((Scribe sc) => sc.Id == this.Id)
                .Match("(comm:Comment)")
                .Where((Comment comm) => comm.Id == comid)
                .CreateUnique("(sc)-[:HAS_COMMENT]->(comm)")
                .ExecuteWithoutResults();
        }

        public List<Comment> GetComments()
        {
            int count = (int)Graph.Instance.Cypher
                .Match("(sc:Scribe)-[:HAS_COMMENT]->(com:Comment)")
                .Where((Scribe sc) => sc.Id == this.Id)
                .Return(com => com.Count())
                .Results.First();

            if (count > 0)
            {
                return Graph.Instance.Cypher
                     .Match("(sc:Scribe)-[:HAS_COMMENT]->(com:Comment)")
                    .Where((Scribe sc) => sc.Id == this.Id)
                    .Return(com => com.As<Comment>())
                     .Results.ToList();
            }
            else
            {
                return new List<Comment>();
            }
        }

        public bool UserLikesScribe(Guid id)
        {
            int count = (int)Graph.Instance.Cypher
                .Match("(user:User)-[:LIKES]->(sc:Scribe)")
                .Where((Scribe sc) => sc.Id == this.Id)
                .AndWhere((User user) => user.Id == id)
                .Return(user => user.Count())
                .Results.First();

            if (count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
