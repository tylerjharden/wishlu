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
    public enum WishStatus
    {
        Requested,
        Promised,
        Revealed,
        Confirmed,
        Reserved,
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

    public enum WishPrivacy
    {
        @public,
        @private
    }

    public enum WishMethod
    {
        bookmarklet,
        chrome_extension,
        firefox_extension,
        safari_extension,
        search,
        upload,
        copy,
        theft
    }

    public class Wish : SocialGraphObject
    {
        // Properties                
        [JsonProperty("Name")]
        public String Name { get; set; }

        [JsonProperty("Description")]
        public String Description { get; set; }

        [JsonProperty("description_html")]
        public string DescriptionHtml { get; set; }

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

        [JsonIgnore]
        public string Image
        {
            get
            {
                if (string.IsNullOrEmpty(ImageUrl))
                    return "https://assets.wishlu.com/images/DefaultWish.jpg";

                return ImageUrl.Replace("http://", "https://");
            }
        }

        [JsonProperty("privacy")]
        public WishPrivacy Privacy { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }
                
        [JsonProperty("WishUrl")]
        public String WishUrl { get; set; }

        [JsonProperty("Rating")] // 0 - 5
        public int Rating { get; set; }

        [JsonProperty("RatingCount")]
        public int RatingCount { get; set; }

        [JsonProperty("WishStatus")]
        public WishStatus WishStatus { get; set; }

        [JsonIgnore]
        public WishStatus Status
        {
            get
            {
                if (this.GetConfirmedGifts().Count >= this.Quantity)
                    return WishStatus.Confirmed;

                if (this.GetRevealedGifts().Count >= 1)
                    return WishStatus.Revealed;

                if (this.GetReservedGifts().Count > 0 || this.GetPurchasedGifts().Count > 0)
                    return WishStatus.Reserved;

                return WishStatus.Requested; // default valid wish status
            }
        }

        [JsonIgnore]
        public bool IsGiftable
        {
            get
            {
                // TODO: Convince Molly & Patrick that this is a good idea.
                //Wishlu lu = Wishlu.GetWishLuById(this.GetAssignmentId());

                //if (lu.EventDateTime.HasValue && lu.EventDateTime.Value.AddDays(1) < DateTimeOffset.Now)
                //    return false; // this wish is in a wishlu whose event gifting window has already passed

                // gifting window has not passed, or wishlu does not have a specified date
                // wish is giftable is the number of confirmed gifts is less than the requested quantity
                return (this.GetConfirmedGifts().Count < this.Quantity);
            }
        }
                      
        [JsonProperty("Quantity")]
        public int Quantity { get; set; }

        [JsonProperty("Purchased")]
        public int Purchased { get; set; }
        
        [JsonProperty("Size")]
        public string Size { get; set; }

        [JsonProperty("Color")]
        public string Color { get; set; }

        [JsonProperty("is_uploaded")]
        public bool IsUploaded { get; set; }

        [JsonProperty("is_rewish")]
        public bool IsRewish { get; set; }
        
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
        [JsonProperty("like_count")]
        public int LikeCount { get; set; }

        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        [JsonProperty("rewish_count")]
        public int RewishCount { get; set; }

        [JsonProperty("TimesCopied")]
        public int TimesCopied { get; set; }

        [JsonProperty("TimesStolen")]
        public int TimesStolen { get; set; }

        [JsonProperty("TimesGrabbed")]
        public int TimesGrabbed { get; set; }

        // The regular expression object to be used to validate the GTIN code.   
        [JsonIgnore]
        private static Regex GtinCodeRegex { get; set; }

        // Static COnstructor
        static Wish()
        {
            GtinCodeRegex = new Regex("^((\\d{8})|(\\d{12,14}))$");
        }

        // Events        
        public event EventHandler Created; // item is initially created
        public event EventHandler Deleted; // item is deleted
        public event EventHandler Updated; // item is updated
        public event EventHandler Copied; // item is copied
        public event EventHandler Stolen; // item is stolen by another user
        public event EventHandler Rewished; // item is copied/grabbed/rewished by another user
        public event EventHandler Assigned; // item is assigned to a wishlu

        // Public Constructor
        public Wish()
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
            //validationErrors.ValidatePattern("GtinCode", GtinCode, GtinCodeRegex, "Service.Wish.InvalidGtinCode");
        }
        
        public override void Create()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

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
                
        public override void Delete()
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

            var u = User.GetUserById(this.UserId);

            if (u.ShouldSendPush("activity_item_copied"))
            {
                // Push Notification
                Notification n = new Notification();
                n.SenderId = userId;                
                n.NotificationType = NotificationType.Info;
                n.Url = "/i/" + newWish.Id;
                n.Content = "<b>" + User.GetUserFullName(userId) + "</b> has copied your wish.";
                n.CreateNotification();
                n.AddRecipient(this.UserId);
                n.Push();
            }

            if (u.ShouldSendEmail("activity_item_copied"))
            {
                // TODO: Item copied email notification
            }

            if (u.ShouldSendMobile("activity_item_copied"))
            {
                u.SendText(User.GetUserFullName(userId) + " has copied your wish.");
            }
                        
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
                                
                // Notify wishlu subscribers
                Notification n = new Notification();
                //n.UserId = this.ReceiverId;
                n.SenderId = this.UserId;
                n.NotificationType = NotificationType.Info;
                n.Content = "<b>" + User.GetUserFullName(this.UserId) + "</b> has added a new item to their wishlu <b>" + Wishlu.GetWishLuName(wishluId) + "</b>."; // promise to give you " + w.Name + ".";
                n.Url = "/i/" + this.Id;
                n.CreateNotification();

                n.AddRecipients(Wishlu.GetFollowerIds(wishluId)); // add all users subscribed to notifications for this wishlu
                n.Push(); // push out the notificationpublic
                
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

                // set timeout for 5 seconds (Optional)
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
                
        public List<Gift> GetAllGifts()
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(g:Gift)<-[:GIFT]-(w:Wish)")
                 .Where((Wish w) => w.Id == this.Id)
                 .Return(g => g.As<Gift>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }

        public List<Gift> GetReservedGifts()
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(g:Gift)<-[:GIFT]-(w:Wish)")
                 .Where((Wish w) => w.Id == this.Id)
                 .AndWhere((Gift g) => g.Status.ToString() == GiftStatus.Reserved.ToString())
                 .Return(g => g.As<Gift>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }

        public List<Gift> GetPurchasedGifts()
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(g:Gift)<-[:GIFT]-(w:Wish)")
                 .Where((Wish w) => w.Id == this.Id)
                 .AndWhere((Gift g) => g.Status.ToString() == GiftStatus.Purchased.ToString())
                 .Return(g => g.As<Gift>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }

        public List<Gift> GetRevealedGifts()
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(g:Gift)<-[:GIFT]-(w:Wish)")
                 .Where((Wish w) => w.Id == this.Id)
                 .AndWhere((Gift g) => g.Status.ToString() == GiftStatus.Revealed.ToString())
                 .Return(g => g.As<Gift>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }

        public List<Gift> GetConfirmedGifts()
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(g:Gift)<-[:GIFT]-(w:Wish)")
                 .Where((Wish w) => w.Id == this.Id)
                 .AndWhere((Gift g) => g.Status.ToString() == GiftStatus.Confirmed.ToString())
                 .Return(g => g.As<Gift>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }

        public List<Gift> GetCanceledGifts()
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(g:Gift)<-[:GIFT]-(w:Wish)")
                 .Where((Wish w) => w.Id == this.Id)
                 .AndWhere((Gift g) => g.Status.ToString() == GiftStatus.Canceled.ToString())
                 .Return(g => g.As<Gift>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }

        public Gift Gift(Guid userId, DateTimeOffset revealDate)
        {            
            Gift gift = Wishes.Gift.CreateGiftForWish(Id, userId, UserId, revealDate);

            return gift;
        }

        public static Gift Gift(Guid wishId, Guid userId, DateTimeOffset revealDate)
        {
            Wish wish = GetWishById(wishId);
            return wish.Gift(userId, revealDate);
        }
                
        public Gift GetGift(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(g:Gift)<-[:GIFT]-(wish:Wish)")
                     .Where((Gift g) => g.WishId == this.Id)
                     .AndWhere((Gift g) => g.GiverId == userId)
                     .AndWhere((Gift g) => g.Status.ToString() != GiftStatus.Canceled.ToString())
                     .Return(g => g.As<Gift>())
                     .Results.Single();
            }
            catch { return null; }
        }
                
        public static Wish CopyWish(Guid id)
        {
            return Wish.GetWishById(id).Copy();
        }
    }
}