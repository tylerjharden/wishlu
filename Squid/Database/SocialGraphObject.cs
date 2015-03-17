using Squid.Messages;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squid.Database
{
    public class SocialGraphObject : GraphObject
    {
        // Social Likes
        public void Like(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                .Match("(user:User)")
                .Where((User user) => user.Id == userId)
                .Match("(n:" + Type.Name + ")")
                .Where((GraphObject n) => ((GraphObject)n).Id == this.Id)
                .Merge("(user)-[:LIKES]->(n)")
                .ExecuteWithoutResults();
            }
            catch { }
        }

        public void Unlike(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                .Match("(user:User)-[r:LIKES]->(n:" + Type.Name + ")")
                .Where((User user) => user.Id == userId)
                .AndWhere((GraphObject n) => ((GraphObject)n).Id == this.Id)
                .Delete("r")
                .ExecuteWithoutResults();
            }
            catch { }
        }

        public List<User> GetLikes()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[:LIKES]->(n:" + Type.Name + ")")
                    .Where((GraphObject n) => ((GraphObject)n).Id == this.Id)
                    .Return(user => user.As<User>())
                    .Results.ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        public int GetLikesCount()
        {
            try
            {
                return (int)Graph.Instance.Cypher
                .Match("(user:User)-[:LIKES]->(n:" + Type.Name + ")")
                .Where((GraphObject n) => ((GraphObject)n).Id == this.Id)
                .Return(user => user.Count())
                .Results.First();
            }
            catch { return 0; }
        }

        // Social Comments
        public void AddComment(Guid userId, string comment)
        {
            if (string.IsNullOrEmpty(comment))
                return;

            Comment com = new Comment(userId, comment);
            com.Create(); // commit to database

            Guid comid = com.Id;

            Graph.Instance.Cypher
                .Match("(user:User)", "(comm:Comment)", "(n:" + Type.Name + ")")
                .Where((User user) => user.Id == userId)                
                .AndWhere((Comment comm) => comm.Id == comid)
                .AndWhere((GraphObject n) => ((GraphObject)n).Id == this.Id)
                .Merge("(user)-[:AUTHORED]->(comm)")
                .Merge("(n)-[:HAS_COMMENT]->(comm)")
                .ExecuteWithoutResults();
        }

        public List<Comment> GetComments()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(n:" + Type.Name + ")-[:HAS_COMMENT]->(com:Comment)")
                    .Where((GraphObject n) => ((GraphObject)n).Id == this.Id)
                    .Return(com => com.As<Comment>())
                    .Results.ToList();
            }
            catch
            {
                return new List<Comment>();
            }
        }

        public bool UserLikes(Guid id)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[:LIKES]->(n:" + Type.Name + ")")
                    .Where((GraphObject n) => ((GraphObject)n).Id == this.Id)
                    .AndWhere((User user) => user.Id == id)
                    .Return(user => user.Count())
                    .Results.First() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
