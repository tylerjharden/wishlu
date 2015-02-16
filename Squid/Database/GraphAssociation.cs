using Newtonsoft.Json;
using Squid.Log;
using System;
using System.Linq;

namespace Squid.Database
{
    public class GraphAssociation<T1, T2> where T1 : GraphObject where T2 : GraphObject
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTimeOffset CreatedOn { get; set; }

        [JsonProperty("LastModifiedOn")]
        public DateTimeOffset LastModifiedOn { get; set; }
                
        [JsonIgnore]
        internal virtual GraphAssociationType Type { get; set; }

        [JsonIgnore]
        public T1 Alpha 
        { 
            get 
            { 
                if (alpha == null)
                    PopulateObjects();
 
                return alpha;
            } 
            set
            {
                alpha = value;
            } 
        }
        private T1 alpha;

        [JsonIgnore]
        public T2 Omega
        {
            get
            {
                if (omega == null)
                    PopulateObjects();

                return omega;
            }
            set
            {
                omega = value;
            } 
        }
        private T2 omega;

        public GraphAssociation()
        {            
            this.Id = Guid.Empty;
        }

        // Create the graph node to represent this object (relationship that has no arrows or direction)
        public virtual void CreateMutual()
        {
            this.CreatedOn = DateTimeOffset.Now;
            this.LastModifiedOn = DateTimeOffset.Now;

            Graph.Instance.Cypher
                 .Match("(alpha:" + Alpha.Type.Name + ")")
                 .Where((T1 alpha) => alpha.Id == Alpha.Id)
                 .Match("(omega:" + Omega.Type.Name + ")")
                 .Where((T2 omega) => omega.Id == Omega.Id)
                 .CreateUnique("(alpha)-[:" + Type.ToString() + " {assoc}]-(omega)")
                 .WithParam("assoc", this)
                 .ExecuteWithoutResults();

            //Logger.Log("GraphAssociation:CreateMutual() succeeded for " + this.Id);
        }

        // Create the graph node to represent this object (relationship that 'points' from Alpha --> Omega)
        public virtual void CreateExclusive()
        {
            this.CreatedOn = DateTimeOffset.Now;
            this.LastModifiedOn = DateTimeOffset.Now;

            Graph.Instance.Cypher
                 .Match("(alpha:" + Alpha.Type.Name + ")", "(omega:" + Omega.Type.Name + ")")
                 .Where((T1 alpha) => alpha.Id == Alpha.Id)                 
                 .AndWhere((T2 omega) => omega.Id == Omega.Id)
                 .CreateUnique("(alpha)-[:" + Type.ToString() + " {assoc}]->(omega)")
                 .WithParam("assoc", this)
                 .ExecuteWithoutResults();

            //Logger.Log("GraphAssociation:CreateExclusive() succeeded for " + this.Id);
        }
                
        // Populate this object with information contained in the respective graph node
        public virtual GraphAssociation<T1,T2> Fetch()
        {
            //Logger.Log("GraphAssociation:Fetch() for " + this.Id);

            return Graph.Instance.Cypher
                 .Match("(alpha)-[assoc:" + Type.ToString() + "]-(omega)")
                 .Where((GraphAssociation<T1,T2> assoc) => assoc.Id == this.Id)                 
                 .Return(assoc => assoc.As<GraphAssociation<T1,T2>>())
                 .Results.First();            
        }

        // Load Alpha and Omega objects from database and populate them locally
        public virtual void PopulateObjects()
        {
            //Logger.Log("GraphAssociation:PopulateObjects() for " + this.Id);

            this.Alpha = Graph.Instance.Cypher
                 .Match("(alpha)-[assoc:" + Type.ToString() + "]-(omega)")
                 .Where((GraphAssociation<T1,T2> assoc) => assoc.Id == this.Id)
                 .Return(alpha => alpha.As<T1>())
                 .Results.First();

            this.Omega = Graph.Instance.Cypher
                 .Match("(alpha)-[assoc:" + Type.ToString() + "]-(omega)")
                 .Where((GraphAssociation<T1,T2> assoc) => assoc.Id == this.Id)
                 .Return(omega => omega.As<T2>())
                 .Results.First();            
        }

        // Update graph relationship to reflect new data in this object
        public virtual void Update()
        {
            //Logger.Log("GraphAssociation:Update() for " + this.Id);

            this.LastModifiedOn = DateTimeOffset.Now;

            Graph.Instance.Cypher
                 .Match("(alpha)-[assoc:" + Type + "]-(omega)")
                 .Where((GraphAssociation<T1,T2> assoc) => assoc.Id == this.Id)    
                 .Set("assoc = {p}")
                 .WithParam("p", this)
                 .ExecuteWithoutResults();
        }

        // Delete the graph node linked to this object
        public virtual void Delete()
        {
            //Logger.Log("GraphAssociation:Delete() for " + this.Id);

            Graph.Instance.Cypher
                .Match("(a)-[r:" + Type + "]-(b)")
                 .Where((GraphAssociation<T1,T2> r) => r.Id == this.Id)    
                 .Delete("r")
                 .ExecuteWithoutResults();
        }
                
        // Set a specified property to a specified value on the graph relationship
        public virtual void Set(string property, object value)
        {
            //Logger.Log("GraphAssociation:Set() for " + this.Id + " Property: " + property);

            Graph.Instance.Cypher
                .Match("(a)-[r:" + Type + "]-(b)")
                 .Where((GraphAssociation<T1,T2> r) => r.Id == this.Id)    
                 .Set("r." + property + " = {p}")
                 .WithParam("p", value)
                 .ExecuteWithoutResults();

            this.Dirty();
        }

        internal void Dirty()
        {
            //Logger.Log("GraphAssociation:Dirty() for " + this.Id);

            this.LastModifiedOn = DateTimeOffset.Now;

            Graph.Instance.Cypher
                .Match("(a)-[r:" + Type + "]-(b)")
                 .Where((GraphAssociation<T1,T2> r) => r.Id == this.Id)    
                 .Set("r.LastModifiedOn = {p}")
                 .WithParam("p", this.LastModifiedOn)
                 .ExecuteWithoutResults();
        }
    }

    enum GraphAssociationType
    {
        REQUESTED_FRIENDSHIP = 0,
        ACCEPTED_FRIENDSHIP,
        DECLINED_FRIENDSHIP,
        FRIEND,
        BLOCKED,
        TAGGED,
        TAGGED_AT,
        POKED,
        INVITED,
        INVITED_BY,
        AUTHORED,
        AUTHORED_BY,
        LIKED_BY,
        LIKES,
        MERCHANT,
        PHOTO,
        WISH,
        LOCATION,
        MEMBER_OF,
        HAS_MEMBER,
        PROMISED,
        PROMISED_BY,
        STOLE,
        STOLEN,
        STOLEN_FROM,
        STOLEN_BY,
        WISHED_BY,
        WISHED_FOR,
        CONTAINS_WISH,
        HAS_SUBSCRIBER,
        SUBSCRIBED_TO,
        CONNECTED_ACCOUNT,
        CREATED,
        CREATED_BY
    }
}
