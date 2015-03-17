using Schloss.Data.Neo4j.Cypher;
using Newtonsoft.Json;
using Squid.Database;
using Squid.Log;
using Squid.Users;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Squid.Wishes
{
    public enum DeleteWishluOptions
    {        
        DeleteAllWishes = 1,        
        DoNotDeleteWishes = 2
    }

    public enum WishluType
    {
        // Automatically generated birthday wishlu
        Birthday = 1,
        // Private, floating wishlu that is only visible to the user
        JustMe = 2,
        // Automatically/user generated public wishlu visible to anyone on wishlu
        Public = 3,
        // wishlus created by the user
        UserDefined = 4
    }

    public enum WishluVisibility
    {
        Public = 1, // everyone sees it
        Private = 2, // only the user sees it
        Friends = 3 // wishloops dictate visibility
    }
    
    public class WishDisplayInfo
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
    }

    public class WishluWishes
    {
        public Wishlu Wishlu { get; set; }
        public IEnumerable<Wish> Wishes { get; set; }
    }

    public class MappedWishlu
    {        
        public Guid Id { get; set; }        
        public String Name { get; set; }        
        public WishluType WishLuType { get; set; }
        public WishluVisibility Visibility { get; set; }        
        public Guid UserId { get; set; }
        public DateTimeOffset? EventDateTime { get; set; }
        public int DisplayColor { get; set; }
                
        public string UserFullName { get; set; }
        public string UserProfileImage { get; set; }
    }

    public class Wishlu : GraphObject
    {
        [JsonProperty("Name")]
        public String Name { get; set; }

        [JsonProperty("Description")]
        public String Description { get; set; }

        [JsonProperty("WishLuType")]
        public WishluType WishLuType { get; set; }

        [JsonProperty("Visibility")]
        public WishluVisibility Visibility { get; set; }

        [JsonProperty("UserId")]
        public Guid UserId { get; set; }

        [JsonProperty("owner")]
        public Guid Owner { get; set; }

        [JsonProperty("EventDateTime")]
        public DateTimeOffset? EventDateTime { get; set; }

        [JsonProperty("DisplayColor")]
        public int DisplayColor { get; set; }

        [JsonProperty("Sort")]
        public int Sort { get; set; }

        [JsonIgnore]
        public string Url
        {
            get
            {
                return string.Format("/l/{0}", Id);
            }
        }

        [JsonIgnore]
        private List<Wish> Wishes
        {
            get
            {
                return this.GetWishes();
            }
            set { }
        }

        [JsonIgnore]
        private List<WishDisplayInfo> FeaturedWishes
        {
            get { return this.GetFeaturedWishes(); }
            set { }
        }

        [JsonIgnore]
        private List<Wishloop> Wishloops
        {
            get
            {
                return this.GetWishloops();
            }
            set { }
        }

        static Wishlu()
        { }

        public Wishlu()
        {
            Id = Guid.Empty;
            UserId = Guid.Empty;            
            DisplayColor = Int32.Parse("95D5E1", System.Globalization.NumberStyles.HexNumber);//0x00000000;       
            Name = "";
            Description = "";
            Sort = 7; // Date: Newest First
            Visibility = WishluVisibility.Friends;
        }

        [JsonIgnore]
        public String WishLuTypeString
        {
            get
            {
                return WishLuType.ToString();
            }
            set
            {
                WishLuType = (WishluType)Enum.Parse(typeof(WishluType), value);
            }
        }

        [JsonIgnore]
        public String EventDateTimeString
        {
            get
            {
                return EventDateTime.HasValue ? JsonHelper.DateToJson(EventDateTime.Value.DateTime) : "";
            }
            set
            {
                EventDateTime = JsonHelper.ParseDate(value);
            }
        }

        [JsonIgnore]
        public bool IsDeletable
        {
            get
            {
                return WishLuType != WishluType.JustMe;
            }
        }

        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            validationErrors.ValidateMaxLength("Name", Name, 50);
            validationErrors.ValidateMaxLength("Description", Description, 1000);
            validationErrors.ValidateNotNull("UserId", UserId);
            validationErrors.ValidateEnum("WishLuType", WishLuType);
        }

        public override void Create()
        {
            if (Id == null || Id == Guid.Empty)
                Id = Guid.NewGuid();

            if (string.IsNullOrEmpty(Description))
                Description = "";

            List<ValidationError> validationErrors = new List<ValidationError>();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();
                        
            base.Create();

            Graph.Instance.Cypher
                .Match("(lu:Wishlu)", "(user:User)")
                .Where((Wishlu lu) => lu.Id == Id)
                .AndWhere((User user) => user.Id == this.UserId)
                .Create("(user)-[:CREATED]->(lu)")
                .ExecuteWithoutResults();                        
        }

        public static Wishlu CreateBirthdayWishLu(Guid userId, DateTimeOffset birthday)
        {
            Wishlu wishLu = new Wishlu();
            
            wishLu.Name = "birthday";
            wishLu.Description = "My birthday wishes.";
            wishLu.WishLuType = WishluType.Birthday;
            wishLu.UserId = userId;           
            wishLu.Visibility = WishluVisibility.Friends;
            wishLu.EventDateTime = birthday;
            wishLu.DisplayColor = Int32.Parse("95D5E1", System.Globalization.NumberStyles.HexNumber);

            wishLu.Create();
            
            return wishLu;
        }

        public static Wishlu CreateJustMeWishLu(Guid userId)
        {
            Wishlu wishLu = new Wishlu();

            wishLu.Name = "just me";
            wishLu.Description = "Private wishes that only you can see.";
            wishLu.WishLuType = WishluType.JustMe;
            wishLu.UserId = userId;
            wishLu.EventDateTime = null;
            wishLu.Visibility = WishluVisibility.Private;
            wishLu.DisplayColor = Int32.Parse("95D5E1", System.Globalization.NumberStyles.HexNumber);

            wishLu.Create();
            
            return wishLu;
        }
                
        public static List<Wishlu> GetUsersWishLus(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)")
                    .Where((Wishlu lu) => lu.UserId == userId)
                    .ReturnDistinct(lu => lu.As<Wishlu>())
                    .Results.ToList();
            }
            catch
            {
                return new List<Wishlu>();
            }
        }

        public static List<MappedWishlu> GetUsersMappedWishLus(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User {Id:{userid}})-[:CREATED]-(lu:Wishlu)")
                    .WithParam("userid", userId)
                    .With("DISTINCT user,lu")
                    .Return(() => Return.As<MappedWishlu>("{Id: w.Id, Name: w.Name, WishLuType: w.WishLuType, Visibility: w.Visibility, UserId: w.UserId, EventDateTime: w.EventDateTime, DisplayColor: w.DisplayColor, UserFullName: collect(user.FirstName + ' ' + user.LastName), UserProfileImage: user.ImageUrl}"))
                    .Results.ToList();
            }
            catch
            {
                return new List<MappedWishlu>();
            }
        }

        public static dynamic GetUsersWishLusWishes(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)")
                    .Where((Wishlu lu) => lu.UserId == userId)
                    .OptionalMatch("(lu)-[:CONTAINS_WISH]-(wish:Wish)")
                    .ReturnDistinct((lu, wish) => new WishluWishes
                    {
                        Wishlu = lu.As<Wishlu>(),
                        Wishes = wish.CollectAs<Wish>()
                    })
                    .Results.ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return new List<WishluWishes>();
            }
        }

        public static Wishlu GetUsersJustMeWishLu(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)")
                    .Where((Wishlu lu) => lu.UserId == userId)
                    .AndWhere((Wishlu lu) => lu.WishLuType.ToString() == WishluType.JustMe.ToString())
                    .Return(lu => lu.As<Wishlu>())
                    .Results.Single();
            }
            catch
            {
                return null;
            }
        }

        public static Wishlu GetUsersBirthdayWishLu(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)")
                    .Where((Wishlu lu) => lu.UserId == userId)
                    .AndWhere((Wishlu lu) => lu.WishLuType.ToString() == WishluType.Birthday.ToString())
                    .Return(lu => lu.As<Wishlu>())
                    .Results.Single();
            }
            catch
            {
                return null;
            }
        }
        
        public List<Wish> GetWishes()
        {
            try
            {
                List<Wish> wishes = Graph.Instance.Cypher
                     .Match("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                     .Where((Wishlu wishlu) => wishlu.Id == this.Id)
                     .Return(wish => wish.As<Wish>())
                     .Results.ToList();

                return wishes.OrderBy(x => x.CreatedOn).Reverse().ToList();
            }
            catch
            {
                return new List<Wish>();
            }
        }

        public List<WishDisplayInfo> GetFeaturedWishes()
        {
            try
            {                
                return Graph.Instance.Cypher
                    .Match("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                    .Where((Wishlu wishlu) => wishlu.Id == this.Id)
                    .Return((wish) => new WishDisplayInfo
                    {
                        Id = Return.As<Guid>("wish.Id"),
                        ImageUrl = Return.As<string>("wish.ImageUrl")
                    })
                    .OrderByDescending("wish.CreatedOn")
                    .Limit(6)
                    .Results.ToList();                
            }
            catch (Exception e)
            {
                Logger.Error(e);

                return new List<WishDisplayInfo>();
            }
        }

        public int GetWishCount()
        {
            try
            {
                return (int)Graph.Instance.Cypher
                     .Match("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                     .Where((Wishlu wishlu) => wishlu.Id == this.Id)
                     .Return(wish => wish.Count())
                     .Results.Single();
            }
            catch
            {
                return 0;
            }
        }

        public static string GetWishLuName(Guid id)
        {
            try
            {
                return Graph.Instance.Cypher
                      .Match("(lu:Wishlu)")
                      .Where((Wishlu lu) => lu.Id == id)
                      .Return((user) => new
                      {
                          Name = Return.As<string>("lu.Name")
                      })
                      .Results.Single().Name;
            }
            catch
            {
                return "";
            }
        }

        public void DeleteWishLu(DeleteWishluOptions deleteWishLuOptions)
        {
            if (!IsDeletable)
            {
                String message = "Service.WishLu.DeleteNotAllowed";
                throw new OperationNotAllowedException(message);
            }

            List<Wish> wishes = GetWishes();
            if (deleteWishLuOptions == DeleteWishluOptions.DeleteAllWishes)            
                foreach (Wish wish in wishes)
                    wish.Delete();            
            else            
                foreach (Wish wish in wishes)
                    wish.AssignToJustMeWishLu();
                                    
            this.DeleteAll();            
        }

        public static void DeleteWishLus(List<Wishlu> wishLus, DeleteWishluOptions deleteWishLuOptions)
        {
            if (wishLus == null)
                return;

            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            foreach (Wishlu wishLu in wishLus)
                wishLu.DeleteWishLu(deleteWishLuOptions);
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

        public static Wishlu GetWishLuById(Guid wishLuId)
        {
            try
            {
                Wishlu wishLu = Graph.Instance.Cypher
                    .Match("(lu:Wishlu)")
                    .Where((Wishlu lu) => lu.Id == wishLuId)
                    .Return(lu => lu.As<Wishlu>())
                    .Results.First();

                if (wishLu != null)
                    return wishLu;

                String message = ("Service.WishLu.WishLuNotFound");

                throw new ItemNotFoundException(message);
            }
            catch
            {
                return null;
            }
        }

        public bool AddSubscriber(Guid wishloopId)
        {
            try
            {
                // Just Me is private, no assignment to wishloops
                if (this.WishLuType == Squid.Wishes.WishluType.JustMe)
                    return false;
                
                // user-created public wishlus have no wishloops
                if (this.Visibility == WishluVisibility.Public)
                    return false;

                // user-created private wishlus have no wishloops
                if (this.Visibility == WishluVisibility.Private)
                    return false;

                // create the HAS_SUBSCRIBER relationship in the graph
                Graph.Instance.Cypher
                    .Match("(lu:Wishlu)","(loop:Wishloop)")
                    .Where((Wishlu lu) => lu.Id == this.Id)
                    .AndWhere((Wishloop loop) => loop.Id == wishloopId)
                    .Create("(lu)-[:HAS_SUBSCRIBER]->(loop)")
                    .ExecuteWithoutResults();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AddSubscribers(List<Guid> wishloopIds)
        {                                    
            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            foreach (Guid wishloopId in wishloopIds)
                AddSubscriber(wishloopId);
            //transaction.Commit();
            //}
        }

        public static void AddSubscribers(Guid wishLuId, List<Guid> wishloopIds)
        {
            Wishlu wishLu = GetWishLuById(wishLuId);
                        
            wishLu.AddSubscribers(wishloopIds);
        }

        public static void AddSubscriber(Guid wishLuId, Guid wishloopId)
        {
            Wishlu wishLu = GetWishLuById(wishLuId);
                        
            wishLu.AddSubscriber(wishloopId);
        }

        private bool RemoveSubscriber(Guid wishloopId)
        {
            try
            {
                Graph.Instance.Cypher
                   .Match("(wishlu:Wishlu)-[r:HAS_SUBSCRIBER]-(loop:Wishloop)")
                   .Where((Wishlu wishlu) => wishlu.Id == this.Id)
                   .AndWhere((Wishloop loop) => loop.Id == wishloopId)
                   .Delete("r")
                   .ExecuteWithoutResults();

                return true;
            }
            catch
            {
                return false;
            }       
        }

        public void RemoveSubscribers(List<Guid> wishloopIds)
        {            
            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            foreach (Guid wishloopId in wishloopIds)
                RemoveSubscriber(wishloopId);
            //transaction.Commit();
            //}
        }

        public bool RemoveAllSubscribers()
        {
            try
            {
                Graph.Instance.Cypher
                   .Match("(lu:Wishlu)-[r:HAS_SUBSCRIBER]-(loop:Wishloop)")
                   .Where((Wishlu lu) => lu.Id == this.Id)
                   .Delete("r")
                   .ExecuteWithoutResults();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void RemoveSubscribers(Guid wishLuId, List<Guid> wishloopIds)
        {
            Wishlu wishLu = GetWishLuById(wishLuId);

            wishLu.RemoveSubscribers(wishloopIds);
        }

        public bool AddFollower(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User)", "(wishlu:Wishlu)")
                    .Where((User user) => user.Id == userId)
                    .AndWhere((Wishlu wishlu) => wishlu.Id == this.Id)
                    .Merge("(user)-[:FOLLOWING]-(wishlu)")
                    .ExecuteWithoutResults();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveFollower(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User)-[r:FOLLOWING]-(wishlu:Wishlu)")
                    .Where((User user) => user.Id == userId)
                    .AndWhere((Wishlu wishlu) => wishlu.Id == this.Id)
                    .Delete("r")
                    .ExecuteWithoutResults();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool HasFollower(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:FOLLOWING]-(wishlu:Wishlu)")
                    .Where((User user) => user.Id == userId)
                    .AndWhere((Wishlu wishlu) => wishlu.Id == this.Id)
                    .Return(user => user.Count())
                    .Results.Single() > 0;                                
            }
            catch
            {
                return false;
            }
        }

        public List<User> GetFollowers()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:FOLLOWING]-(wishlu:Wishlu)")
                    .Where((Wishlu wishlu) => wishlu.Id == this.Id)
                    .Return(user => user.As<User>())
                    .Results.ToList();
            }
            catch
            {
                return new List<User>();
            }
        }
                
        public List<Guid> GetFollowerIds()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:FOLLOWING]-(wishlu:Wishlu)")
                    .Where((Wishlu wishlu) => wishlu.Id == this.Id)
                    .Return(() => Return.As<Guid>("user.Id"))
                    .Results.ToList();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public static List<Guid> GetFollowerIds(Guid wishluId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:FOLLOWING]-(wishlu:Wishlu)")
                    .Where((Wishlu wishlu) => wishlu.Id == wishluId)
                    .Return(() => Return.As<Guid>("user.Id"))
                    .Results.ToList();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public static Wishloop GetPrivateWishloop(Guid wishluId)
        {
            try
            {
                // A wishlu's private wishloop will be created when the user specifies a list of friends to share with as opposed to a user-created
                // wishloop. It will also be automatically destroyed. It has the same ID as the wishlu itself.
                return Graph.Instance.Cypher
                    .Match("(loop:Wishloop)")
                    .Where((Wishloop loop) => loop.Id == wishluId)
                    .Return(loop => loop.As<Wishloop>())
                    .Results.Single();
            }
            catch
            {
                return null;
            }
        }

        public Wishloop GetPrivateWishloop()
        {
            try
            {
                // A wishlu's private wishloop will be created when the user specifies a list of friends to share with as opposed to a user-created
                    // wishloop. It will also be automatically destroyed. It has the same ID as the wishlu itself.
                return Graph.Instance.Cypher
                    .Match("(loop:Wishloop)")
                    .Where((Wishloop loop) => loop.Id == this.Id)
                    .Return(loop => loop.As<Wishloop>())
                    .Results.Single();
            }
            catch
            {
                return null;
            }
        }

        public bool CreatePrivateWishloop()
        {
            try
            {
                // if the private wishloop has already been created do not repeat
                if (GetPrivateWishloop() == null)
                {
                    Wishloop loop = new Wishloop();
                    loop.Name = this.Id.ToString();
                    loop.UserId = this.UserId;
                    loop.Description = "";
                    loop.Id = this.Id;
                    loop.Hidden = true;
                    loop.Create();
                }
                                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UsePrivateWishloop()
        {
            try
            {
                if (CreatePrivateWishloop())
                {
                    this.RemoveAllSubscribers();
                    this.AddSubscriber(this.Id);

                    return true;
                }
                else
                    return false;                
            }
            catch
            {
                return false;
            }
        }

        public bool DeletePrivateWishloop()
        {
            try
            {
                Wishloop loop = this.GetPrivateWishloop();

                if (loop == null)
                    return true;
                                
                loop.DeleteWishloop();

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public static List<Wishloop> GetWishloops(Guid wishLuId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)-[:HAS_SUBSCRIBER]-(loop:Wishloop)")
                    .Where((Wishlu lu) => lu.Id == wishLuId)
                    .Return(loop => loop.As<Wishloop>())
                    .Results.ToList();
            }
            catch
            {
                return new List<Wishloop>();
            }
        }

        public List<Wishloop> GetWishloops()
        {
            return GetWishloops(Id);
        }

        public static List<Guid> GetWishloopIds(Guid wishLuId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)-[:HAS_SUBSCRIBER]-(loop:Wishloop)")
                    .Where((Wishlu lu) => lu.Id == wishLuId)
                    .Return(loop => Return.As<Guid>("loop.Id"))
                    .Results.ToList();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public List<Guid> GetWishloopIds()
        {
            return GetWishloopIds(Id);
        }

        public static List<Wishlu> GetFriendsWishLus(Guid requestingUserId, Guid friendUserId)
        {
            return Graph.Instance.Cypher
                .Match("(lu:Wishlu)-[:HAS_SUBSCRIBER]-(loop:Wishloop)-[:HAS_MEMBER]-(user:User)")
                .Where("loop.UserId = {fid} AND user.Id = {rid} AND (lu.Visibility = 'Friends' OR lu.Visibility IS NULL)")
                .OrWhere("(lu.UserId = {fid} AND lu.Visibility = 'Public')")
                .WithParams(new { fid = friendUserId, rid = requestingUserId })
                .ReturnDistinct(lu => lu.As<Wishlu>())
                .Results.ToList();
        }

        public static List<WishluWishes> GetFriendsWishLusWishes(Guid requestingUserId, Guid friendUserId)
        {
            try
            {
                // Cypher Query:
                // Match all wishlus and their wishes that belong to friend User Id
                // Match succeeds if they belong to a wishloop that requesting User can see OR they are marked public
                // Grandfathered wishlus that have no stored Visibility value are treated as 'Friends' Visibility
                // Resulted are distinct, multiple hits to the same wishlu/wish are filtered, no duplicates
                return Graph.Instance.Cypher
                    .Match("(wish:Wish)-[:CONTAINS_WISH]-(lu:Wishlu)-[:HAS_SUBSCRIBER]-(loop:Wishloop)-[:HAS_MEMBER]-(user:User)")
                    .Where("loop.UserId = {fid} AND user.Id = {rid} AND (lu.Visibility = 'Friends' OR lu.Visibility IS NULL)")                    
                    .WithParams(new {fid = friendUserId, rid = requestingUserId})                    
                    .Return((lu, wish) => new WishluWishes
                    {
                        Wishlu = lu.As<Wishlu>(),
                        Wishes = wish.CollectAsDistinct<Wish>()
                    })
                    .Results.Union(GetUsersPublicWishlusWishes(friendUserId)).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return new List<WishluWishes>(); // return an empty list, this allows HTTP requests to process successfully and avoids NullReferenceException in the view
            }
        }

        public static List<WishluWishes> GetUsersPublicWishlusWishes(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(lu:Wishlu)-[:CONTAINS_WISH]-(wish:Wish)")
                    .Where((Wishlu lu) => lu.UserId == userId)
                    .AndWhere((Wishlu lu) => lu.Visibility.ToString() == WishluVisibility.Public.ToString())
                    .Return((lu, wish) => new WishluWishes
                    {
                        Wishlu = lu.As<Wishlu>(),
                        Wishes = wish.CollectAsDistinct<Wish>()
                    })
                    .Results.ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return new List<WishluWishes>(); // return an empty list, this allows HTTP requests to process successfully and avoids NullReferenceException in the view
            }
        }
    }
}