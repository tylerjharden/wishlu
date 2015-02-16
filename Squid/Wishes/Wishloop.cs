using Schloss.Data.Neo4j.Cypher;
using Newtonsoft.Json;
using Squid.Database;
using Squid.Token;
using Squid.Users;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Squid.Wishes
{
    public enum WishloopType
    {
        // single, system-generated wishloop that always contains all of the user's friends
        AllFriends = 1,
        // the user created this wishloop
        UserDefined = 2
    }

    public class MappedWishloop
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public WishloopType WishloopType { get; set; }
        public Guid UserId { get; set; }
        public int DisplayColor { get; set; }
        public List<User> Members { get; set; }
        public List<Wishlu> Wishlus { get; set; }
    }

    public class Wishloop : GraphObject
    {
        [JsonProperty("Name")]
        public String Name { get; set; }

        [JsonProperty("Description")]
        public String Description { get; set; }

        [JsonProperty("WishloopType")]
        public WishloopType WishloopType { get; set; }

        [JsonProperty("UserId")]
        public Guid UserId { get; set; }

        [JsonProperty("DisplayColor")]
        public int DisplayColor { get; set; }

        [JsonProperty("Hidden")]
        public bool Hidden { get; set; }

        static Wishloop() { }

        public Wishloop()
        {
            Id = Guid.Empty;
            Name = "";
            Description = "";
            UserId = Guid.Empty;
            DisplayColor = Int32.Parse("95D5E1", System.Globalization.NumberStyles.HexNumber);
            Hidden = false;
        }

        [JsonIgnore]
        public String WishloopTypeString
        {
            get
            {
                return WishloopType.ToString();
            }
            set
            {
                WishloopType = (WishloopType)Enum.Parse(typeof(WishloopType), value);
            }
        }

        public bool IsDeletable
        {
            get
            {
                return WishloopType == WishloopType.UserDefined;
            }
        }

        public bool CanUpdateMemberships
        {
            get
            {
                return WishloopType == WishloopType.UserDefined;
            }
        }

        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            validationErrors.ValidateMaxLength("Name", Name, 50);
            validationErrors.ValidateMaxLength("Description", Description, 1000);
            validationErrors.ValidateNotNull("UserId", UserId);
            validationErrors.ValidateEnum("WishloopType", WishloopType);
        }

        public override void Create()
        {
            if (this.Id == Guid.Empty)
                this.Id = Guid.NewGuid();

            List<ValidationError> validationErrors = new List<ValidationError>();
                        
            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();
                        
            base.Create();
        }

        public static Wishloop CreateAllFriendsWishloop(Guid userId)
        {
            Wishloop wishLoop = new Wishloop();

            wishLoop.Name = "friends"; //WishLuSession.GetText("Service.Wishloop.AllFriendsWishloopName"       );
            wishLoop.Description = "wishloop containing all of my friends."; //WishLuSession.GetText("Service.Wishloop.AllFriendsWishloopDescription");
            wishLoop.WishloopType = WishloopType.AllFriends;
            wishLoop.UserId = userId;
            wishLoop.DisplayColor = Int32.Parse("95D5E1", System.Globalization.NumberStyles.HexNumber);

            wishLoop.Create();

            return wishLoop;
        }

        public static List<Wishloop> GetUsersWishloops(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                  .Match("(loop:Wishloop {UserId:{id}})")
                  .WithParam("id", userId)
                  .Where("(loop.Hidden = false OR loop.Hidden IS NULL)")                  
                  .Return(loop => loop.As<Wishloop>())
                  .OrderBy("loop.WishloopType,loop.Name")
                  .Results.ToList();
            }
            catch
            {
                return new List<Wishloop>();
            }
        }

        public static List<MappedWishloop> GetUsersMappedWishloops(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                  .Match("(loop:Wishloop {UserId:{id}})")
                  .WithParam("id", userId)
                  .Where("(loop.Hidden = false OR loop.Hidden IS NULL)")
                  .With("DISTINCT loop")
                  .OptionalMatch("(loop)-[:HAS_MEMBER]-(user:User)")
                  .With("DISTINCT loop,user")
                  .OptionalMatch("(lu:Wishlu)-[:HAS_SUBSCRIBER]-(loop)")
                  .With("DISTINCT loop,user,lu")
                  .Return(() => Return.As<MappedWishloop>("{Id: loop.Id, Name: loop.Name, WishloopType: loop.WishloopType, UserId: loop.UserId, DisplayColor: loop.DisplayColor, Members: collect(DISTINCT user), Wishlus: collect(DISTINCT lu)}"))
                  .OrderBy("loop.WishloopType,loop.Name")
                  .Results.ToList();
            }
            catch
            {
                return new List<MappedWishloop>();
            }
        }

        public static List<Wishloop> GetFriendsWishloops(Guid requestingUserId, Guid friendUserId)
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(loop:Wishloop)-[r:HAS_MEMBER]->(user:User)")
                     .Where((Wishloop loop) => loop.UserId == friendUserId)
                     .AndWhere((User user) => user.Id == requestingUserId)
                     .AndWhere("(loop.Hidden = false OR loop.Hidden IS NULL)")
                     .Return(loop => loop.As<Wishloop>())
                     .Results.ToList();
            }
            catch
            {
                return new List<Wishloop>();
            }
        }

        public void DeleteWishloop()
        {
            if (!IsDeletable)
            {
                String message = ("Service.Wishloop.DeleteNotAllowed");

                throw new OperationNotAllowedException(message);
            }
                  
            // delete all members and subscribers, then delete this wishloop
            this.DeleteAll();            
        }

        public static void DeleteWishloops(List<Wishloop> wishloops)
        {
            if (wishloops == null)
                return;

            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            foreach (Wishloop wishloop in wishloops)
                wishloop.DeleteWishloop();
            //transaction.Commit();
            //}
        }

        public override void Update()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            base.Update();
        }

        private static Wishloop SearchForWishloopById(Guid wishloopId)
        {
            try
            {
                return Graph.Instance.Cypher
                       .Match("(loop:Wishloop)")
                       .Where((Wishloop loop) => loop.Id == wishloopId)
                       .ReturnDistinct(loop => loop.As<Wishloop>())
                       .Results.Single();
            }
            catch
            {
                return null;
            }
        }

        public static Wishloop GetWishloopById(Guid wishloopId)
        {
            Wishloop wishloop = SearchForWishloopById(wishloopId);

            if (wishloop != null)
                return wishloop;

            String message = ("Service.Wishloop.WishloopNotFound");

            throw new ItemNotFoundException(message);
        }

        public static Wishloop GetAllFriendsWishloopByUserId(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(loop:Wishloop)")
                     .Where((Wishloop loop) => loop.UserId == userId)
                     .AndWhere((Wishloop loop) => loop.WishloopType.ToString() == WishloopType.AllFriends.ToString())
                     .Return(loop => loop.As<Wishloop>())
                     .Results.Single();
            }
            catch
            {
                return null;
            }
        }

        public static void AddMember(Guid userId, Guid wishloopId)
        {
            try
            {
                WishloopMember wishloopMember = new WishloopMember();

                wishloopMember.WishloopId = wishloopId;
                wishloopMember.UserId = userId;
                wishloopMember.CreateWishloopMember();
            }
            catch (WishloopMemberAlreadyRecordedException)
            {
                // Ignore an error caused by a duplcate wishloop member entry.                           //
            }
        }

        public void AddMember(Guid userId)
        {            
            AddMember(userId, Id);
        }

        public void AddMembers(List<Guid> userIds)
        {            
            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            foreach (Guid userId in userIds)
                AddMember(userId);
            //transaction.Commit();
            //}
        }

        public static void AddMembers(Guid wishloopId, List<Guid> userIds)
        {
            Wishloop wishloop = GetWishloopById(wishloopId);

            wishloop.AddMembers(userIds);
        }

        public static void RemoveMember(Guid userId, Guid wishloopId)
        {
            if (Wishloop.GetWishloopById(wishloopId).WishloopType == WishloopType.AllFriends)
                return;

            WishloopMember.DeleteByWishloopIdAndUserId(wishloopId, userId);
        }

        private void RemoveMember(Guid userId)
        {
            RemoveMember(userId, Id);
        }

        public void RemoveMemberAny(Guid userId)
        {
            WishloopMember.DeleteByWishloopIdAndUserId(this.Id, userId);
        }

        public void RemoveMembers(List<Guid> userIds)
        {
            if (!CanUpdateMemberships)
                return;

            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            foreach (Guid userId in userIds)
                RemoveMember(userId);
            //transaction.Commit();
            //}
        }

        public static void RemoveMembers(Guid wishloopId, List<Guid> userIds)
        {
            Wishloop wishloop = GetWishloopById(wishloopId);

            wishloop.RemoveMembers(userIds);
        }

        public static List<Wishloop> GetWishloopsByMember(Guid userId, Guid memberId)
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(loop:Wishloop)-[r:HAS_MEMBER]-(user:User)")
                     .Where((Wishloop loop) => loop.UserId == userId)
                     .AndWhere((User user) => user.Id == memberId)
                     .AndWhere("(loop.Hidden = false OR loop.Hidden IS NULL)")
                     .ReturnDistinct(loop => loop.As<Wishloop>())
                     .Results.ToList();
            }
            catch
            {
                return new List<Wishloop>();
            }
        }

        public static List<User> GetMembers(Guid wishloopId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(loop:Wishloop)-[r:HAS_MEMBER]-(user:User)")
                    .Where((Wishloop loop) => loop.Id == wishloopId)
                    .ReturnDistinct(user => user.As<User>())
                    .Results.ToList();
            }
            catch { return new List<User>(); }
        }

        public static bool HasMember(Guid wishloopId, Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(loop:Wishloop)-[r:HAS_MEMBER]-(user:User)")
                    .Where((Wishloop loop) => loop.Id == wishloopId)
                    .AndWhere((User user) => user.Id == userId)
                    .ReturnDistinct(user => user.CountDistinct())
                    .Results.Single() > 0;
            }
            catch { return false; }
        }

        public bool HasMember(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(loop:Wishloop)-[r:HAS_MEMBER]-(user:User)")
                    .Where((Wishloop loop) => loop.Id == this.Id)
                    .AndWhere((User user) => user.Id == userId)
                    .ReturnDistinct(user => user.CountDistinct())
                    .Results.Single() > 0;
            }
            catch { return false; }
        }

        public List<User> GetMembers()
        {
            return GetMembers(Id);
        }

        public bool UnsubscribeFromAll()
        {
            try
            {
                Graph.Instance.Cypher
               .Match("(wishlu:Wishlu)-[r:HAS_SUBSCRIBER]-(loop:Wishloop)")
               .Where((Wishloop loop) => loop.Id == this.Id)
               .Delete("r")
               .ExecuteWithoutResults();

                return true;
            }
            catch { return false; }       
        }

        public void SubscribeTo(Guid wishlu)
        {
            Wishlu.AddSubscriber(wishlu, this.Id);
        }

        public List<Wishlu> GetWishlus()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)-[r:HAS_SUBSCRIBER]-(loop:Wishloop)")
                    .Where((Wishloop loop) => loop.Id == this.Id)
                    .Return(lu => lu.As<Wishlu>())
                    .Results.ToList();
            }
            catch
            {
                return new List<Wishlu>();
            }
        }

        public List<Guid> GetWishluIds()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)-[r:HAS_SUBSCRIBER]-(loop:Wishloop)")
                    .Where((Wishloop loop) => loop.Id == this.Id)
                    .Return(lu => Return.As<Guid>("lu.Id"))
                    .Results.ToList();
            }
            catch
            {
                return new List<Guid>();
            }
        }
    }
}