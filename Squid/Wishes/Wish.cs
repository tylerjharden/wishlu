using Newtonsoft.Json;
using Squid.Database;
using Squid.Housekeeping;
using Squid.Messages;
using Squid.Products;
using Squid.Users;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace Squid.Wishes
{
    public
    enum WishStatus
    {
        Requested,
        Promised,
        Revealed,
        Confirmed,
        Unfulfilled,
        Invalid
    }
    
    public enum WishSource
    {
        Mmilkshake = 0,
        Amazon,
        BestBuy,
        Etsy,
        Button,
        Custom
    }

    public class Wish : SocialGraphObject
    {
        [JsonProperty("Name")]
        public String Name { get; set; }

        [JsonProperty("Description")]
        public String Description { get; set; }

        [JsonProperty("UserId")]
        public Guid UserId { get; set; }

        [JsonProperty("WishLuId")]
        public Guid WishLuId { get; set; }
                
        [JsonProperty("Notes")]
        public String Notes { get; set; }

        [JsonProperty("GtinCode")]
        public String GtinCode { get; set; }

        [JsonProperty("Price")]
        public Decimal Price { get; set; }

        [JsonProperty("ImageUrl")]
        public String ImageUrl { get; set; }
                
        [JsonProperty("WishUrl")]
        public String WishUrl { get; set; }

        [JsonProperty("Rating")] // 0 - 5
        public int Rating { get; set; }

        [JsonProperty("RatingCount")]
        public int RatingCount { get; set; }

        [JsonProperty("WishStatus")]
        public WishStatus WishStatus { get; set; }

        [JsonProperty("ManuallyConfirmed")]
        public bool ManuallyConfirmed { get; set; }
                                
        [JsonProperty("Quantity")]
        public int Quantity { get; set; }

        [JsonProperty("Purchased")]
        public int Purchased { get; set; }
        
        [JsonProperty("Size")]
        public string Size { get; set; }

        [JsonProperty("Color")]
        public string Color { get; set; }
        
        [JsonProperty("IsStolen")]
        public bool IsStolen { get; set; }

        [JsonProperty("IsGrabbed")]
        public bool IsGrabbed { get; set; }

        [JsonProperty("SourceUserId")]
        public Guid SourceUserId { get; set; }

        [JsonProperty("IsCopy")]
        public bool IsCopy { get; set; }
                
        [JsonProperty("IsRevealed")]
        public bool IsRevealed { get; set; }
                
        [JsonProperty("IsCustom")]
        public bool IsCustom { get; set; }

        // Milkshake
        [JsonProperty("IsMilkshake")]
        public bool IsMilkshake { get; set; }

        [JsonProperty("MilkshakeProductId")]
        public Guid MilkshakeProductId { get; set; }

        [JsonProperty("ShopId")]
        public Guid ShopId { get; set; }

        // Amazon
        [JsonProperty("IsAmazon")]
        public bool IsAmazon { get; set; }

        [JsonProperty("ASIN")]
        public string ASIN { get; set; }

        // BestBuy
        [JsonProperty("IsBestBuy")]
        public bool IsBestBuy { get; set; }

        [JsonProperty("SKU")]
        public string SKU { get; set; }

        // Etsy
        // ENHANCEMENT: IsEtsy, Etsy Identifier

        // =================== //
        // Internal Statistics //
        // =================== //
        [JsonProperty("TimesCopied")]
        public int TimesCopied { get; set; }

        [JsonProperty("TimesStolen")]
        public int TimesStolen { get; set; }

        [JsonProperty("TimesGrabbed")]
        public int TimesGrabbed { get; set; }

        // The regular expression object to be used to validate the GTIN code.   
        [JsonIgnore]
        private static Regex GtinCodeRegex { get; set; }

        static
        Wish()
        {
            GtinCodeRegex = new Regex("^((\\d{8})|(\\d{12,14}))$");
        }

        public
        Wish()
        {
            // Empty Ids
            Id = Guid.Empty;
            UserId = Guid.Empty;
            ShopId = Guid.Empty;
            WishLuId = Guid.Empty;
            SourceUserId = Guid.Empty;            
            GtinCode = "000000000000";

            // Empty/Default Attributes            
            Rating = 0;
            RatingCount = 0;
            Quantity = 1;
            Purchased = 0;
            Color = "";            
            Description = "";            
            Name = "";
            Price = 0.00m;
            Size = "";
            SKU = "";
                        
            // Default Stats            
            IsCustom = false;
            IsMilkshake = false;
            IsAmazon = false;
            IsBestBuy = false;

            IsCopy = false;
            //IsDeletable = true;
            IsGrabbed = false;
            IsRevealed = false;
            IsStolen = false;
            TimesCopied = 0;
            TimesGrabbed = 0;
            TimesStolen = 0;

            // Invalid on construction, only properly created and populated wishes will have a non-Invalid wish status.
            WishStatus = Wishes.WishStatus.Invalid;
        }

        [JsonIgnore]
        public String WishStatusString
        {
            get { return WishStatus.ToString(); }
            set { WishStatus = (WishStatus)Enum.Parse(typeof(WishStatus), value); }
        }

        public bool IsDeletable
        {
            get
            {
                return true;
            }
        }

        public void MakeMembersValid()
        {
            Name = Name.Substring(0, 100);
            Description = Description.Substring(0, 1000);            
            WishUrl = WishUrl.Substring(0, 2000);

            if (Rating < 0 || 5 < Rating)
                Rating = 0;
            if (String.IsNullOrEmpty(GtinCode) || !GtinCodeRegex.IsMatch(GtinCode))
                GtinCode = null;
        }

        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            validationErrors.ValidateMaxLength("Name", Name, 256);
            validationErrors.ValidateMaxLength("Description", Description, 2048);           
            validationErrors.ValidateMaxLength("WishUrl", WishUrl, 2048);
            validationErrors.ValidateNotNull("UserId", UserId);
            validationErrors.ValidateNotNull("Name", Name);
            validationErrors.ValidateRange("Rating", Rating, 0, 5);
            //validationErrors.ValidatePattern   ("GtinCode"             ,GtinCode              ,GtinCodeRegex,"Service.Wish.InvalidGtinCode");
        }

        private static Byte[] GetImageData(String imageUrl)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                    return webClient.DownloadData(imageUrl);
            }
            catch
            {
                return null;
            }
        }

        public override void Create()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            this.WishStatus = Wishes.WishStatus.Requested;

            base.Create();
            AssignToJustMeWishLu();
        }

        public void CreateFromMilkshake(Milkshake.Product p, Milkshake.Offer o, Guid userId, Guid wishluId, int quantity = 1)
        {
            decimal price = 0;

            this.Name = p.Name;
            this.UserId = userId;
            this.WishUrl = o.Url;
            this.GtinCode = p.UPC;
            this.Quantity = quantity;
            //this.Shop = o.Store;
            this.ShopId = o.StoreId;
            this.Description = p.Description;

            this.WishStatus = Squid.Wishes.WishStatus.Requested;

            if (Decimal.TryParse(o.Price.Replace("$", "").Replace(",", ""), out price))
            {
                this.Price = price;
            }
            else
            {
                this.Price = 0;
            }

            this.IsMilkshake = true;
            this.MilkshakeProductId = p.Id;

            this.Create();

            this.SetImage(new Uri(p.Image));

            if (wishluId != null && wishluId != Guid.Empty)
                this.AssignToWishlu(wishluId);
        }

        public static List<Wish> GetUsersWishes(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .OptionalMatch("(user:User)-[:WISHED_FOR]-(wish:Wish)")
                    .Where((User user) => user.Id == userId)
                    .Return(wish => wish.As<Wish>())
                    .Results.ToList();
            }
            catch { return null; }
        }
        
        public void DeleteWish()
        {
            if (!IsDeletable)
            {
                String message = ("Service.Wish.DeleteNotAllowed");

                throw new OperationNotAllowedException(message);
            }

            this.DeleteAll();
        }
                
        public override void Update()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            base.Update();
        }

        public static Wish GetWishById(Guid wishId)
        {
            Wish wish;

            wish = Graph.Instance.Cypher
            .Match("(w:Wish)")
            .Where((Wish w) => w.Id == wishId)
            .Return(w => w.As<Wish>())
            .Results.Single();

            if (wish != null)
                return wish;

            String message = "Service.Wish.WishNotFound";

            throw new ItemNotFoundException(message);
        }
                
        public Wish Copy()
        {
            Wish newWish = new Wish();

            newWish.Name = this.Name;
            newWish.Price = this.Price;
            newWish.ImageUrl = this.ImageUrl;
            newWish.UserId = this.UserId;
            newWish.SourceUserId = this.UserId;            
            newWish.Size = this.Size;
            newWish.SKU = this.SKU;
            newWish.GtinCode = this.GtinCode;
            newWish.Color = this.Color;            
            newWish.Description = this.Description;
            
            newWish.Notes = this.Notes;

            newWish.Quantity = this.Quantity;            
            newWish.WishUrl = this.WishUrl;

            newWish.IsCopy = true;

            this.TimesCopied += 1;
            this.Update();

            newWish.Create();

            // Create a "COPY_OF" relationship
            Graph.Instance.Cypher
             .Match("(w1:Wish)")
             .Where((Wish w1) => w1.Id == this.Id)
             .Match("(w2:Wish)")
             .Where((Wish w2) => w2.Id == newWish.Id)
             .CreateUnique("(w2)-[:COPY_OF]->(w1)")
             .ExecuteWithoutResults();

            return newWish;
        }

        public Wish Copy(Guid wishluId)
        {
            Wish newWish = this.Copy();

            newWish.AssignToWishlu(wishluId);

            return newWish;
        }

        public Wish Steal(Guid userId)
        {
            Wish newWish = new Wish();

            newWish.Name = this.Name;
            newWish.Price = this.Price;
            newWish.ImageUrl = this.ImageUrl;
            newWish.UserId = userId;
            newWish.SourceUserId = this.UserId;            
            newWish.Size = this.Size;
            newWish.SKU = this.SKU;
            newWish.GtinCode = this.GtinCode;
            newWish.Color = this.Color;            
            newWish.Description = this.Description;
            
            newWish.Quantity = this.Quantity;            
            newWish.WishUrl = this.WishUrl;

            newWish.IsStolen = true;

            this.TimesStolen += 1;
            this.Update();

            newWish.WishStatus = Wishes.WishStatus.Requested;

            newWish.Create();

            // Create a "STOLEN_FROM" relationship
            Graph.Instance.Cypher
             .Match("(w1:Wish)")
             .Where((Wish w1) => w1.Id == this.Id)
             .Match("(w2:Wish)")
             .Where((Wish w2) => w2.Id == newWish.Id)
             .CreateUnique("(w2)-[:STOLEN_FROM]->(w1)")
             .ExecuteWithoutResults();

            return newWish;
        }

        public Wish Steal(Guid userId, Guid wishluId)
        {
            Wish newWish = this.Steal(userId);

            newWish.AssignToWishlu(wishluId);

            return newWish;
        }

        public Wish Grab(Guid userId)
        {
            Wish newWish = new Wish();

            newWish.Name = this.Name;
            newWish.Price = this.Price;
            newWish.ImageUrl = this.ImageUrl;
            newWish.UserId = userId;
            newWish.SourceUserId = this.UserId;            
            newWish.Size = this.Size;
            newWish.SKU = this.SKU;
            newWish.GtinCode = this.GtinCode;
            newWish.Color = this.Color;            
            newWish.Description = this.Description;
            
            newWish.Quantity = this.Quantity;
            newWish.WishUrl = this.WishUrl;

            newWish.IsGrabbed = true;

            this.TimesGrabbed += 1;
            this.Update();

            newWish.WishStatus = Wishes.WishStatus.Requested;

            newWish.Create();

            // Create a "GRABBED_FROM" relationship
            Graph.Instance.Cypher
             .Match("(w1:Wish)")
             .Where((Wish w1) => w1.Id == this.Id)
             .Match("(w2:Wish)")
             .Where((Wish w2) => w2.Id == newWish.Id)
             .CreateUnique("(w2)-[:GRABBED_FROM]->(w1)")
             .ExecuteWithoutResults();

            // Notification
            Notification n = new Notification();
            n.SenderId = userId;
            n.UserId = this.UserId;
            n.Id = Guid.NewGuid();
            n.NotificationType = NotificationType.Info;
            n.Url = "/i/" + newWish.Id;
            n.Content = User.GetUserFullName(userId) + " has copied your wish for " + this.Name;
            n.CreateNotification();

            return newWish;
        }

        public Wish Grab(Guid userId, Guid wishluId)
        {
            Wish newWish = this.Grab(userId);

            newWish.AssignToWishlu(wishluId);

            return newWish;
        }
                
        public void RemoveAllAssignments()
        {
            try
            {
                Graph.Instance.Cypher
                  .OptionalMatch("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                  .Where((Wish wish) => wish.Id == this.Id)
                  .Delete("r")
                  .ExecuteWithoutResults();
            }
            catch
            { }
        }

        public bool AssignToWishlu(Guid wishluId)
        {
            try
            {
                if (this.WishLuId == wishluId)
                    return false; // optimization: assigning a wish to its current wishlu should do nothing.

                this.RemoveAllAssignments();

                this.WishLuId = wishluId;
                this.Set("WishLuId", this.WishLuId);

                Graph.Instance.Cypher
                    .Match("(wish:Wish)")
                    .Where((Wish wish) => wish.Id == this.Id)
                    .Match("(wishlu:Wishlu)")
                    .Where((Wishlu wishlu) => wishlu.Id == wishluId)
                    .Create("(wishlu)-[:CONTAINS_WISH]->(wish)")
                    .ExecuteWithoutResults();
                
                return true;
            }
            catch (WishAssignmentAlreadyRecordedException)
            {
                return false;
            }
        }

        public bool AssignToWishlu(Wishlu wishlu)
        {
            // If the specified WishLu does not belong to the same user, throw an exception.                 
            if (wishlu.UserId != this.UserId)
            {
                String message = ("Service.WishAssignment.UserError");
                List<ValidationError> validationErrors = new List<ValidationError>();

                validationErrors.Add(new ValidationError(message, "WishLuId"));
                throw new ValidationException(message, validationErrors);
            }

            return AssignToWishlu(wishlu.Id);
        }
                
        public static bool AssignToWishlu(Guid wishId, Guid wishLuId)
        {
            return GetWishById(wishId).AssignToWishlu(wishLuId);
        }

        public void AssignToJustMeWishLu()
        {
            Wishlu justmeWishLu = Wishlu.GetUsersJustMeWishLu(UserId);
            AssignToWishlu(justmeWishLu);
        }

        public static WishAssignment GetAssignment(Guid wishId)
        {
            try
            {
                return Graph.Instance.Cypher
                  .OptionalMatch("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                  .Where((WishAssignment r) => r.WishId == wishId)
                  .Return(r => r.As<WishAssignment>())
                  .Results.Single();
            }
            catch
            {
                return null;
            }
        }

        public static Guid GetAssignmentId(Guid wishId)
        {
            try
            {
                return Graph.Instance.Cypher
                  .OptionalMatch("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                  .Where((WishAssignment r) => r.WishId == wishId)
                  .Return(wishlu => wishlu.As<Wishlu>().Id)
                  .Results.Single();
            }
            catch
            {
                return Guid.Empty;
            }
        }

        public WishAssignment GetAssignment()
        {
            return GetAssignment(Id);
        }

        public Guid GetAssignmentId()
        {
            if (this.WishLuId == null || this.WishLuId == Guid.Empty)
            {
                Guid assignId = GetAssignmentId(this.Id);
                this.WishLuId = assignId;
                this.Set("WishLuId", this.WishLuId);
            }

            return this.WishLuId;
        }

        public bool SetImage(Byte[] imageDataBytes)
        {
            try
            {
                ImageUrl = Services.Imgur.Imgur.UploadImage(imageDataBytes);
                this.Set("ImageUrl", ImageUrl);

                return true;
            }
            catch { return false; }
        }

        public bool SetImage(String base64String)
        {
            try
            {
                Byte[] data = Convert.FromBase64String(base64String);
                return SetImage(data);
            }
            catch
            {
                return false;
            }
        }

        public bool SetImage(Uri url)
        {
            try
            {
                // Open a connection
                System.Net.HttpWebRequest _HttpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                _HttpWebRequest.AllowWriteStreamBuffering = true;

                // You can also specify additional header values like the user agent or the referrer: (Optional)
                _HttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                // _HttpWebRequest.Referer = "http://www.google.com/";

                // set timeout for 20 seconds (Optional)
                _HttpWebRequest.Timeout = 5000;

                // Request response:
                System.Net.WebResponse _WebResponse = _HttpWebRequest.GetResponse();

                // Open data stream:
                System.IO.Stream _WebStream = _WebResponse.GetResponseStream();

                // convert webstream to image
                System.Drawing.Image img = System.Drawing.Image.FromStream(_WebStream);
                using (MemoryStream ms = new MemoryStream())
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    SetImage(ms.ToArray());
                }

                // Cleanup
                _WebStream.Close();
                _WebResponse.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveImage()
        {
            try
            {
                ImageUrl = "";
                this.Set("ImageUrl", "");

                return true;
            }
            catch { return false; }
        }

        public Promise SelfPromise(Guid userId)
        {
            if (WishStatus == WishStatus.Requested)
                WishStatus = Wishes.WishStatus.Promised;

            if (WishStatus == Wishes.WishStatus.Promised)
                WishStatus = Wishes.WishStatus.Confirmed;

            this.ManuallyConfirmed = true;

            this.Update();

            return Wishes.Promise.CreateSelfConfirmedPromiseForWish(this.Id, userId);
        }

        public Promise Promise(Guid userId, DateTimeOffset revealDate)
        {
            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            if (WishStatus == WishStatus.Requested)
                WishStatus = WishStatus.Promised;

            // This update must be performed even if the status is not being changed.  This will     //
            // ensure that the wish data that this pledge is based on is not stale data.  i.e. it    //
            // will ensure that another user has not pledged or granted the wish in the time since   //
            // this wish object was read from the database.                                          //
            //                                                                                       //
            this.Update();

            Promise promise = Wishes.Promise.CreatePromiseForWish(Id, userId, revealDate);

            //if (promise != null)
            //    promise.SendFirstNotificationMessage();
            //transaction.Commit();
            return promise;
            //}
        }

        public static Promise PromiseWish(Guid wishId, Guid userId, DateTimeOffset revealDate)
        {
            Wish wish = GetWishById(wishId);
            return wish.Promise(userId, revealDate);
        }

        public void Reveal(Promise promise)
        {
            WishStatus = WishStatus.Revealed;
            IsRevealed = true;

            this.Update();

            promise.Reveal();

            User promiser = promise.GetPromiser();

            Wishlu wishlu = Wishlu.GetWishLuById(this.GetAssignmentId());

            Notification n = new Notification();
            n.UserId = this.UserId;
            n.SenderId = promiser.Id;
            n.NotificationType = NotificationType.Info;
            n.Content = promiser.FullName + " has gifted you " + this.Name + ". When you receive it, go to your " + wishlu.Name + " wishlu to confirm as received, or click this notification.";
            n.Url = "/i/" + this.Id;
            n.CreateNotification();

            //User to = User.GetUserById(this.UserId);

            /*EmailTask em = new EmailTask();
            em.EmailNotification = EmailNotification.Reveal;
            em.To = to.LoginId;
            em.Subject = promiser.FullName + " has gifted you one of your items!";
            
            Graph.Instance.Cypher
                .Create("(n:EmailTask" + DateTime.Now.ToString("MMddyy") + " {p})")
                .WithParam("p", em)
                .ExecuteWithoutResults();*/

            new Mail.MailController().RevealEmail(promise).Deliver();
        }

        public void Confirm(Promise promise)
        {
            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            WishStatus = WishStatus.Confirmed;

            // This update must be performed even if the status is not being changed.  This will     //
            // ensure that the wish data that this grant is based on is not stale data.  i.e. it     //
            // will ensure that another user has not granted the wish in the time since this wish    //
            // object was read from the database.                                                    //
            //                    
            this.Purchased = this.Purchased + 1;

            //
            this.Update();

            promise.Confirm();

            User promiser = promise.GetPromiser();

            Notification n = new Notification();
            n.UserId = promiser.Id;
            n.SenderId = this.UserId;
            n.NotificationType = NotificationType.Info;
            n.Content = User.GetUserFullName(this.UserId) + " has confirmed receiving your gift of " + this.Name + ".";
            n.Url = "/i/" + this.Id;
            n.CreateNotification();

            //if (followUpTime == null)
            //MakeGrantNotificationForSinglePledge(pledge);
            //transaction.Commit();
            //}
        }

        public bool HasBeenRevealed()
        {
            return GetRevealedPromises().Count > 0;
        }

        public static void ConfirmWish(Guid promiseId)
        {
            Promise promise = null;
            Wish wish = null;

            try
            {
                promise = Wishes.Promise.GetPromiseById(promiseId);
                wish = promise.GetWish();
            }
            catch (ItemNotFoundException exception)
            {
                String message = "Service.Pledge.PledgeExpiredError";
                throw new PledgeExpiredException(message, exception);
            }
            wish.Confirm(promise);
        }

        public void Cancel(Promise promise)
        {
            if (promise == null)
                return;

            if (promise.WishId != Id)
                return;

            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{

            promise.Cancel();

            if (WishStatus == WishStatus.Promised && GetPromisedPromises().Count == 0)
                WishStatus = WishStatus.Requested;

            // This update must be performed even if the status is not being changed.  This will     //
            // ensure that the wish data that this grant is based on is not stale data.  i.e. it     //
            // will ensure that another user has not granted the wish in the time since this wish    //
            // object was read from the database.                                                    //
            //                                                                                       //
            this.Update();

            User promiser = promise.GetPromiser();

            Notification n = new Notification();
            n.UserId = this.UserId;
            n.SenderId = promiser.Id;
            n.NotificationType = NotificationType.Info;
            n.Content = promiser.FullName + " has canceled their promise to give you " + this.Name + ".";
            n.Url = "/i/" + this.Id;
            n.CreateNotification();

            //transaction.Commit();
            //}
        }

        public static void CancelPromise(Guid promiseId)
        {
            Promise promise = null;
            Wish wish = null;

            try
            {
                promise = Wishes.Promise.GetPromiseById(promiseId);
                wish = promise.GetWish();
            }
            catch (ItemNotFoundException exception)
            {
                String message = "Service.Promise.PromiseExpiredError";

                throw new PledgeExpiredException(message, exception);
            }
            wish.Cancel(promise);
        }

        public List<Promise> GetConfirmedPromises()
        {
            return Wishes.Promise.GetConfirmedPromisesForWish(Id);
        }

        public List<Promise> GetPromisedPromises()
        {
            return Wishes.Promise.GetPromisesForWish(Id);
        }

        public Promise GetPromise(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(user:User)-[r:PROMISED]->(wish:Wish)")
                     .Where((Promise r) => r.WishId == this.Id)
                     .AndWhere((Promise r) => r.UserId == userId)
                     .AndWhere((Promise r) => r.PromiseStatus.ToString() == PromiseStatus.Promised.ToString())
                     .Return(r => r.As<Promise>())
                     .Results.Single();
            }
            catch { return null; }
        }

        public List<Promise> GetRevealedPromises()
        {
            return Wishes.Promise.GetRevealedPromisesForWish(Id);
        }

        public List<Promise> GetAllPromises()
        {
            return Wishes.Promise.GetAllPromisesForWish(Id);
        }

        public static Wish CopyWish(Guid id)
        {
            return Wish.GetWishById(id).Copy();
        }
    }
}