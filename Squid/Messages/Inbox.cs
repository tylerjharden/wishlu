using Newtonsoft.Json;
using Squid.Database;
using Squid.Log;
using System.Linq;

namespace Squid.Messages
{
    public class Inbox : MessageScope
    {
        [JsonProperty("ScopeType")]
        public override ScopeType ScopeType { get { return Messages.ScopeType.Inbox; } }
                
        public int GetReadMessageCount()
        {
            int count = (int)Graph.Instance.Cypher
           .Match("(scope:" + this.Type.Name + ")-[:CONTAINS]->(msg:Message)")
           .Where((MessageScope scope) => scope.Id == this.Id)
           .AndWhere((Message msg) => msg.Read == true)
           .Return(msg => msg.Count())
           .Results.First();
                        
            return count;
        }

        public int GetUnreadMessageCount()
        {
            int count = (int)Graph.Instance.Cypher
           .Match("(scope:" + this.Type.Name + ")-[:CONTAINS]->(msg:Message)")
           .Where((MessageScope scope) => scope.Id == this.Id)
           .AndWhere((Message msg) => msg.Read == false)
           .Return(msg => msg.Count())
           .Results.First();

            return count;
        }
    }
}
