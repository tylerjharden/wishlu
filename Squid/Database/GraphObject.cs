using Newtonsoft.Json;
using Squid.Log;
using System;
using System.Linq;

namespace Squid.Database
{
    public class GraphObject
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTimeOffset CreatedOn { get; set; }

        [JsonProperty("LastModifiedOn")]
        public DateTimeOffset LastModifiedOn { get; set; }
                
        [JsonIgnore]
        internal virtual Type Type { get { return this.GetType(); } }

        internal Schloss.Data.Neo4j.Node<GraphObject> Node { get; set; }

        public GraphObject() { }

        // Create the graph node to represent this object
        public virtual void Create()
        {            
            this.CreatedOn = DateTimeOffset.Now;
            this.LastModifiedOn = DateTimeOffset.Now;

            // do not allow empty Guid
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            Graph.Instance.Cypher
                .Create("(n:" + Type.Name + " {p})")
                .WithParam("p", this)
                .ExecuteWithoutResults();

            //Logger.Log("GraphObject:Create() succeeded for " + this.Id);

            //Cache.Add(this.Id.ToString(), this);
        }

        // Create the graph node to represent this object only if one does not exist already, if it already exists, no data is updated
        public virtual void CreateUnique()
        {
            this.CreatedOn = DateTimeOffset.Now;
            this.LastModifiedOn = DateTimeOffset.Now;

            // do not allow empty Guid
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            Graph.Instance.Cypher
                .Merge("(n:" + Type.Name + " { Id: {id} })")
                .OnCreate()
                .Set("n = {p}")
                .WithParam("id", this.Id)
                .WithParam("p", this)
                .ExecuteWithoutResults();

            //Logger.Log("GraphObject:CreateUnique() succeeded for " + this.Id);

            //Cache.Add(this.Id.ToString(), this);
        }

        public virtual GraphObject GetById(Guid id)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(n:" + Type.Name + ")")
                    .Where((GraphObject n) => ((GraphObject)n).Id == id)
                    .Return(n => n.As<GraphObject>())
                    .Results.First();
            }
            catch { return null; }
        }

        public static T GetById<T>(Guid id) where T : GraphObject
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(n:" + typeof(T).Name + ")")
                    .Where((T n) => n.Id == id)
                    .Return(n => n.As<T>())
                    .Results.First();
            }
            catch { return null; }
        }

        // Populate this object with information contained in the respective graph node
        public virtual GraphObject Fetch()
        {
            //Logger.Log("GraphObject:Fetch() for " + this.Id);

            return Graph.Instance.Cypher
                .Match("(n:" + Type.Name + ")")
                .Where((GraphObject n) => ((GraphObject)n).Id == this.Id)
                .Return(n => n.As<GraphObject>())
                .Results.First();            
        }

        // Update graph node to reflect new data in this object
        public virtual void Update()
        {
            //Logger.Log("GraphObject:Update() for " + this.Id);

            this.LastModifiedOn = DateTimeOffset.Now;

            Graph.Instance.Cypher
                .Match("(n:" + Type.Name + ")")
                 .Where((GraphObject n) => n.Id == this.Id)
                 .Set("n = {p}")
                 .WithParam("p", this)
                 .ExecuteWithoutResults();

            //Cache.Set(this.Id.ToString(), this);
        }

        // Delete the graph node linked to this object
        // NOTE: This will fail and throw an exception if it has remaining relationships
        public virtual void Delete()
        {
            //Logger.Log("GraphObject:Delete() for " + this.Id);

            Graph.Instance.Cypher
                .Match("(n:" + Type.Name + ")")
                 .Where((GraphObject n) => n.Id == this.Id)
                 .Delete("n")
                 .ExecuteWithoutResults();

            //Cache.Remove(this.Id.ToString());
        }

        // Delete the graph node this object correlates to, as well as all relationships
        public virtual void DeleteAll()
        {
            //Logger.Log("GraphObject:DeleteAll() for " + this.Id);

            Graph.Instance.Cypher
                .Match("(n:" + Type.Name + ")-[r]-(o)")
                 .Where((GraphObject n) => n.Id == this.Id)
                 .Delete("n,r")
                 .ExecuteWithoutResults();
        }
                
        // Set a specified property to a specified value on the graph node
        public virtual void Set(string property, object value)
        {
            //Logger.Log("GraphObject:Set() for " + this.Id + " Property: " + property);

            Graph.Instance.Cypher
                .Match("(n:" + Type.Name + ")")
                 .Where((GraphObject n) => n.Id == this.Id)
                 .Set("n." + property + " = {p}")                 
                 .WithParam("p", value)
                 .Set("n.LastModifiedOn = {t}")
                 .WithParam("t", this.LastModifiedOn)
                 .ExecuteWithoutResults();

            //Cache.Set(this.Id + ":" + property, value);

            //this.Dirty();
        }

        // Update the object and graph node's last modified time
        internal void Dirty()
        {
            //Logger.Log("GraphObject:Dirty() for " + this.Id);

            this.LastModifiedOn = DateTimeOffset.Now;

            Graph.Instance.Cypher
                .Match("(n:" + Type.Name + ")")
                 .Where((GraphObject n) => n.Id == this.Id)
                 .Set("n.LastModifiedOn = {p}")
                 .WithParam("p", this.LastModifiedOn)
                 .ExecuteWithoutResults();

            //Cache.Set(this.Id.ToString(), this);
        }
    }
}
