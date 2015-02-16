using Newtonsoft.Json;
using Squid.Database;
using Squid.Log;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Squid.Messages
{
    public enum ScopeType
    {
        Inbox = 0,
        IM = 1,
        WishComments = 2,
        WishloopComments = 3,
        WishluComments = 4,
        ScribeComments = 5
    }

    public class MessageScope : GraphObject
    {
        [JsonProperty("ScopeType")]
        public virtual ScopeType ScopeType { get; set; }
        
        [JsonProperty("User1")]
        public Guid User1 { get; set; }

        [JsonProperty("User2")]
        public Guid User2 { get; set; }

        [JsonProperty("LastMessage")]
        public Guid LastMessage { get; set; }

        [JsonProperty("LastSender")]
        public Guid LastSender { get; set; }

        public void AddReceiver(Guid id)
        {
            Graph.Instance.Cypher
               .Match("(scope:" + this.Type.Name + ")")
               .Where((MessageScope scope) => scope.Id == this.Id)
               .Match("(user:User)")
               .Where((User user) => user.Id == id)
               .Create("(user)-[:RECEIVER]->(scope)")
               .ExecuteWithoutResults();
        }

        public void AddMessage(Message msg)
        {
           Graph.Instance.Cypher
               .Match("(scope:" + this.Type.Name + ")")
               .Where((MessageScope scope) => scope.Id == this.Id)
               .Match("(ms:Message)")
               .Where((Message ms) => ms.Id == msg.Id)
               .Create("(scope)-[:CONTAINS]->(ms)")
               .ExecuteWithoutResults();
        }

        public void PushMessage(Message msg)
        {
            Logger.Log("MessageScope:PushMessage() - Id: " + msg.Id);
        }

        public Message GetMessage(Guid id)
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(scope:" + this.Type.Name + ")-[:CONTAINS]->(msg:Message)")
                     .Where((MessageScope scope) => scope.Id == this.Id)
                     .AndWhere((Message msg) => msg.Id == id)
                     .Return(msg => msg.As<Message>())
                     .Results.First();
            }
            catch
            {
                return null;
            }
        }

        public List<Message> GetMessages()
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(scope:" + this.Type.Name + ")-[:CONTAINS]->(msg:Message)")
                     .Where((MessageScope scope) => scope.Id == this.Id)                     
                     .Return(msg => msg.As<Message>())
                     .Results.ToList();
            }
            catch
            {
                return new List<Message>(); // NOTE: Always return an empty list instead of null to prevent exceptions when we iterate in other parts of the app
            }
        }

        public Message GetLastMessage()
        {
            return GetMessages().OrderByDescending(x => x.SendTime).First();
        }
    }

    public class IM : MessageScope
    {
        [JsonIgnore]
        public bool IsUnread
        {
            get { return GetUnreadMessageCount() > 0; }
        }

        public static IM GetConversationById(Guid id)
        {
            try
            {
                return Graph.Instance.Cypher
                         .Match("(convo:IM)")
                         .Where((IM convo) => convo.Id == id)                         
                         .Return(convo => convo.As<IM>())
                         .Results.Single();
            }
            catch { return null; }
        }
                
        public static List<IM> GetConversations(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                         .Match("(convo:IM)")
                         .Where((IM convo) => convo.User1 == userId)
                         .OrWhere((IM convo) => convo.User2 == userId)
                         .Return(convo => convo.As<IM>())
                         .Results.ToList();
            }
            catch
            {
                return new List<IM>();
            }
        }

        public int GetReadMessageCount()
        {
            try
            {
                return (int)Graph.Instance.Cypher
               .Match("(scope:" + this.Type.Name + ")-[:CONTAINS]->(msg:Message)")
               .Where((MessageScope scope) => scope.Id == this.Id)
               .AndWhere((Message msg) => msg.Read == true)
               .Return(msg => msg.Count())
               .Results.First();
            }
            catch { return 0; }
        }

        public int GetUnreadMessageCount()
        {
            try
            {
                return (int)Graph.Instance.Cypher
               .Match("(scope:" + this.Type.Name + ")-[:CONTAINS]->(msg:Message)")
               .Where((MessageScope scope) => scope.Id == this.Id)
               .AndWhere((Message msg) => msg.Read == false)
               .Return(msg => msg.Count())
               .Results.First();
            }
            catch { return 0; }
        }

        public static int GetUnreadConversationsCount(Guid userId)
        {
            try
            {
                return (int)Graph.Instance.Cypher
               .Match("(conv:IM)-[:CONTAINS]->(msg:Message)")               
               .AndWhere((Message msg) => msg.Read == false)
               .AndWhere((Message msg) => msg.SenderId != userId)
               .Return(conv => conv.Count())
               .Results.First();
            }
            catch { return 0; }
        }

        public bool MarkAsRead(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                     .Match("(scope:" + this.Type.Name + ")-[:CONTAINS]->(msg:Message)")
                     .Where((MessageScope scope) => scope.Id == this.Id)
                     .AndWhere((Message msg) => msg.SenderId != userId)
                     .Set("msg.Read = true")
                     .Set("msg.ReadTime = {dto}")
                     .WithParam("dto", DateTimeOffset.Now)
                     .ExecuteWithoutResults();

                return true;
            }
            catch { return false; }
        }

        public Guid GetLastSender()
        {
            return GetLastMessage().SenderId.GetValueOrDefault();
        }
    }
        
    public class Comments : GraphObject
    {
        public void Add(Comment comment)
        { }
    }    
}
