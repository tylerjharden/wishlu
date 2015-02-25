using Schloss.Data.Neo4j.Cypher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squid.Database;
using Squid.Housekeeping;
using Squid.Log;
using Squid.Messages;
using Squid.Token;
using Squid.Validation;
using Squid.Wishes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Squid.Users
{
    // Possible values for visibility and privacy settings on wishlu
    public enum UserPrivacy
    {
        Everyone = 0, // Completely anonymous visitors on wishlu, as well as all wishlu members
        Members = 1, // All registered members of wishlu, no anonymous visitors
        FriendsOfFriends = 2, // Only a user's friends, and all friends of friends
        Friends = 3, // Only a user's friends
        OnlyMe = 4 // Visible only to the user
    }
    
    // This class manages logic and data handling for a user on wishlu.
    public class User : GraphObject
    {
        [JsonProperty("FirstName")]
        public String FirstName { get; set; }

        [JsonProperty("LastName")]
        public String LastName { get; set; }

        [JsonProperty("LoginId")]
        public String LoginId { get; set; }

        [JsonProperty("Email")]
        public String Email { get; set; }

        [JsonProperty("Handle")]
        public string Handle { get; set; }

        [JsonProperty("PasswordHash")]
        public Byte[] PasswordHash { get; set; }

        [JsonProperty("PasswordSalt")]
        public Byte[] PasswordSalt { get; set; }

        [JsonProperty("ConnectionIds")]
        public HashSet<string> ConnectionIds { get; set; } // SignalR connections set (for handling multiple live sessions, propagation of push notifications)

        [JsonProperty("LanguageId")]
        public String LanguageId { get; set; }

        [JsonProperty("DateOfBirth")]
        public DateTimeOffset DateOfBirth { get; set; }

        [JsonProperty("Address1")]
        public String Address1 { get; set; }

        [JsonProperty("Address2")]
        public String Address2 { get; set; }

        [JsonProperty("City")]
        public String City { get; set; }

        [JsonProperty("StateOrProvince")]
        public String StateOrProvince { get; set; }

        [JsonProperty("ZipOrPostalCode")]
        public String ZipOrPostalCode { get; set; }

        [JsonProperty("CountryId")]
        public String CountryId { get; set; }

        [JsonProperty("ShipAddress1")]
        public String ShipAddress1 { get; set; }

        [JsonProperty("ShipAddress2")]
        public String ShipAddress2 { get; set; }

        [JsonProperty("ShipCity")]
        public String ShipCity { get; set; }

        [JsonProperty("ShipStateOrProvince")]
        public String ShipStateOrProvince { get; set; }

        [JsonProperty("ShipZipOrPostalCode")]
        public String ShipZipOrPostalCode { get; set; }

        [JsonProperty("ShipCountryId")]
        public String ShipCountryId { get; set; }

        [JsonProperty("PhoneNumber")]
        public String PhoneNumber { get; set; }

        [JsonProperty("PhoneCarrier")]
        public int PhoneCarrier { get; set; }

        [JsonIgnore]
        public string PhoneCarrierString
        {
            get
            {
                switch (this.PhoneCarrier)
                {
                    default:
                    case 0: return "None";
                    case 1: return "AT&T";
                    case 2: return "T-Mobile";
                    case 3: return "Verizon";
                    case 4: return "Sprint";
                    case 5: return "Virgin Mobile";
                    case 6: return "Tracfone";
                    case 7: return "Metro PCS";
                    case 8: return "Boost Mobile";
                    case 9: return "Cricket";
                    case 10: return "Nextel";
                    case 11: return "Alltel";
                    case 12: return "Ptel";
                    case 13: return "Suncom";
                    case 14: return "Qwest";
                    case 15: return "U.S. Cellular";
                }
            }
        }
                
        [JsonProperty("Headline")]
        public String Headline { get; set; }

        [JsonProperty("Website")]
        public String Website { get; set; }

        [JsonProperty("BlogURL")]
        public String BlogURL { get; set; }

        [JsonProperty("FacebookPageId")]
        public String FacebookPageId { get; set; }

        [JsonProperty("FacebookAccessToken")]
        public String FacebookAccessToken { get; set; }

        [JsonProperty("TwitterUserId")]
        public String TwitterUserId { get; set; }

        [JsonProperty("TwitterAccessToken")]
        public String TwitterAccessToken { get; set; }

        [JsonProperty("TwitterSecretToken")]
        public String TwitterSecretToken { get; set; }

        [JsonProperty("GooglePlusId")]
        public String GooglePlusId { get; set; }

        [JsonProperty("GooglePlusAccessToken")]
        public String GooglePlusAccessToken { get; set; }

        [JsonProperty("GooglePlusRefreshToken")]
        public String GooglePlusRefreshToken { get; set; }

        [JsonProperty("IsActive")]
        public bool IsActive { get; set; } // this user is an active account

        [JsonProperty("IsAdminUser")]
        public bool IsAdminUser { get; set; } // this user is an administrator / developer

        [JsonProperty("ImageUrl")]
        public String ImageUrl
        {
            get;
            set;
        }

        [JsonIgnore]
        public string Image
        {
            get { return ImageUrl.Replace("http://", "https://"); }
        }

        [JsonProperty("Gender")]
        public Char Gender { get; set; }

        [JsonProperty("DeviceId")]
        public String DeviceId { get; set; } // NOTE: Not currently used, may be used in the future to identify mobile devices
        
        [JsonProperty("JustMeWishLuId")]
        public Guid JustMeWishLuId { get; set; }

        //[JsonProperty("PublicWishLuId")]
        //public Guid PublicWishLuId { get; set; }

        [JsonProperty("BirthdayWishLuId")]
        public Guid BirthdayWishLuId { get; set; }
        
        [JsonProperty("LoginCount")]
        public int LoginCount { get; set; } // how many times this user has logged in

        [JsonProperty("TutorialMode")]
        public bool TutorialMode { get; set; } // is the user currently going through the tutorial?

        [JsonProperty("TutorialStep")]
        public int TutorialStep { get; set; } // where in the tutorial is the user?

        // Verification
        [JsonProperty("VerificationCode")]
        public Guid VerificationCode { get; set; } // verification code, e-mailed to user to confirm e-mail address

        [JsonProperty("Verified")]
        public bool Verified { get; set; } // is the user's e-mail verified?

        // Password Reset
        [JsonProperty("InReset")]
        public bool InReset { get; set; } // is the user actively awaiting a password reset?

        [JsonProperty("ResetCode")]
        public Guid ResetCode { get; set; } // password reset code e-mailed to user's e-mail address

        [JsonProperty("ResetExpiration")]
        public DateTimeOffset ResetExpiration { get; set; } // 24-hour period after reset request is issued, after which the reset code no longer works

        // Mobile Token (authorization)
        [JsonProperty("Token")]
        public Guid Token { get; set; }

        [JsonProperty("TokenExpirationTime")]
        public DateTimeOffset TokenExpirationTime { get; set; } // when does the mobile authorization token expire?

        ///////////////
        // FAVORITES //
        ///////////////
        [JsonProperty("FavoriteStores")]
        public List<Guid> FavoriteStores { get; set; } // list of Milkshake Store IDs the user has identified as their favorites / they like

        [JsonProperty("Favorites")]
        public String Favorites { get; set; } // "A few of my favorite things"

        ///////////
        // STATS //
        ///////////      
        [JsonProperty("Stats_Shirt")]
        public string Stats_Shirt { get; set; }
        public string Stats_Shoe { get; set; }
        public string Stats_SuitChest { get; set; }
        public string Stats_SuitLength { get; set; }
        public string Stats_Waist { get; set; }
        public string Stats_Inseam { get; set; }
        public string Stats_Ring { get; set; }
        public string Stats_Hat { get; set; }
        public string Stats_Belt { get; set; }
        public string Stats_Skirt { get; set; }
        public string Stats_BraBand { get; set; }
        public string Stats_BraCup { get; set; }
        public string Stats_Collar { get; set; }
        public string Stats_Top { get; set; }
        public string Stats_Outerwear { get; set; }
        public string Stats_SportCoat { get; set; }

        //////////////////////////////
        // PROFILE PRIVACY SETTINGS //
        //////////////////////////////
        [JsonProperty("Privacy_HideAge")]
        public bool HideAge { get; set; } // user's birth year will be hidden

        [JsonProperty("Privacy_HideLastName")]
        public bool HideLastName { get; set; } // user's last name will always be shortened to last initial

        public string Privacy_Address { get; set; }
        public string Privacy_Shirt { get; set; }
        public string Privacy_Shoe { get; set; }
        public string Privacy_Suit { get; set; }        
        public string Privacy_Pant { get; set; }        
        public string Privacy_Ring { get; set; }
        public string Privacy_Hat { get; set; }
        public string Privacy_Belt { get; set; }
        public string Privacy_Skirt { get; set; }
        public string Privacy_Bra { get; set; }        
        public string Privacy_Top { get; set; }
        public string Privacy_Outerwear { get; set; }
        public string Privacy_SportCoat { get; set; }

        //////////////////////
        // PRIVACY SETTINGS //
        //////////////////////
        [JsonProperty("Privacy_ProfileVisibility")]
        public UserPrivacy ProfileVisibility { get; set; } // global profile visibility, who can see profile details?

        [JsonProperty("Privacy_PublicVisibility")]
        public UserPrivacy PublicVisibility { get; set; } // define the audience that public wishlus can be seen by

        [JsonProperty("Privacy_FollowPermission")]
        public UserPrivacy FollowPermission { get; set; } // set who can follow this user

        [JsonProperty("Privacy_FriendRequestPermission")]
        public UserPrivacy FriendRequestPermission { get; set; } // set whether anyone or only friends of friends can friend request this user

        [JsonProperty("Privacy_AllowEMailLookup")]
        public bool AllowEMailLookup { get; set; } // allow wishlu members to pull up this user by their email address

        [JsonProperty("Privacy_AllowPhoneLookup")]
        public bool AllowPhoneLookup { get; set; } // allow wishlu members to pull up this user by their phone number (if set)

        [JsonProperty("Privacy_AllowIndexing")]
        public bool AllowIndexing { get; set; } // allow search engines to index user's profile, wishlus, items, etc.
                
        ///////////////////////////
        // NOTIFICATION SETTINGS //
        ///////////////////////////
        [JsonProperty("Notifications_ShouldSendNotifications")]
        public bool ShouldSendNotifications { get; set; }

        public bool Notifications_Me_Wishlu { get; set; }
        public bool Notifications_Me_Email { get; set; }
        public bool Notifications_Me_Text { get; set; }
        public bool Notifications_Birthdays_Wishlu { get; set; }
        public bool Notifications_Birthdays_Email { get; set; }
        public bool Notifications_Birthdays_Text { get; set; }
        public bool Notifications_Holidays_Wishlu { get; set; }
        public bool Notifications_Holidays_Email { get; set; }
        public bool Notifications_Holidays_Text { get; set; }
        public bool Notifications_Events_Wishlu { get; set; }
        public bool Notifications_Events_Email { get; set; }
        public bool Notifications_Events_Text { get; set; }
        public bool Notifications_Price_Wishlu { get; set; }
        public bool Notifications_Price_Email { get; set; }
        public bool Notifications_Price_Text { get; set; }
        public bool Notifications_Discount_Wishlu { get; set; }
        public bool Notifications_Discount_Email { get; set; }
        public bool Notifications_Discount_Text { get; set; }
        public bool Notifications_News_Wishlu { get; set; }
        public bool Notifications_News_Email { get; set; }
        public bool Notifications_News_Text { get; set; }
        
        //---------------------------------------------------------------------------------------------//
        
        static User() { }

        public User()
        {
            Id = Guid.Empty;
            //ConnectionIds = new HashSet<string>();
            Token = Guid.Empty;
            TokenExpirationTime = DateTimeOffset.MinValue;
            ImageUrl = "//assets.wishlu.com/images/GenericFriend.png"; // default profile picture
            DateOfBirth = DateTimeOffset.MinValue;
            LoginId = "";
            Email = "";
            Gender = 'm'; // male is more gender neutral, default
            TutorialMode = false;
            TutorialStep = 3;
            LanguageId = "en-us"; // english is default language
            InReset = false;
            ResetCode = Guid.Empty;
            ResetExpiration = DateTimeOffset.MinValue;
            VerificationCode = Guid.Empty;
            Verified = false;
            LoginCount = 0;

            // Profile
            PhoneNumber = "";
            PhoneCarrier = 0;

            // Favorites
            FavoriteStores = new List<Guid>();

            // Default Privacy Settings
            HideAge = false; 
            HideLastName = false; 

            ProfileVisibility = UserPrivacy.Members;
            PublicVisibility = UserPrivacy.Everyone;
            FollowPermission = UserPrivacy.Members;
            FriendRequestPermission = UserPrivacy.Members;
            AllowEMailLookup = true;
            AllowPhoneLookup = true;
            AllowIndexing = true; // Search engine indexing

            // Default Notification Settings
            ShouldSendNotifications = true;
            Notifications_Me_Wishlu = true;
        }

        [JsonIgnore]
        public String FullName
        {
            get { return (FirstName + " " + LastName).Trim(); } // helper property to format user's full name
        }

        [JsonIgnore]
        public String DateOfBirthString
        {
            get { return JsonHelper.DateToJson(DateOfBirth.DateTime); }
            set { DateOfBirth = JsonHelper.ParseDate(value); }
        }
        
        [JsonIgnore]
        public bool IsFacebookSynced
        {
            get { return !String.IsNullOrEmpty(this.FacebookPageId); } // is this user connected with Facebook?
        }

        [JsonIgnore]
        public bool IsTwitterLinked
        {
            get { return !String.IsNullOrEmpty(this.TwitterUserId); } // is this user connected with Twitter?
        }

        [JsonIgnore]
        public bool IsGoogleLinked
        {
            get { return !String.IsNullOrEmpty(this.GooglePlusId); } // is this user connected with Google/Google+?
        }

        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            LoginId = CleanUpLoginId(LoginId);

            validationErrors.ValidateMaxLength("FirstName", FirstName, 50); // First name less than 50 characters
            validationErrors.ValidateMaxLength("LastName", LastName, 50); // Last name less than 50 characters
            validationErrors.ValidateMaxLength("LoginId", LoginId, 50); // E-mail address less than 50 characters
            validationErrors.ValidateMaxLength("PasswordHash", PasswordHash, 64);
            validationErrors.ValidateMaxLength("PasswordSalt", PasswordSalt, 16);
            validationErrors.ValidateMaxLength("LanguageId", LanguageId, 10);
            validationErrors.ValidateMaxLength("Address1", Address1, 50);
            validationErrors.ValidateMaxLength("Address2", Address2, 50);
            validationErrors.ValidateMaxLength("City", City, 50);
            validationErrors.ValidateMaxLength("StateOrProvince", StateOrProvince, 50);
            validationErrors.ValidateMaxLength("ZipOrPostalCode", ZipOrPostalCode, 20);
            validationErrors.ValidateMaxLength("CountryId", CountryId, 5);
            validationErrors.ValidateMaxLength("ShipAddress1", ShipAddress1, 50);
            validationErrors.ValidateMaxLength("ShipAddress2", ShipAddress2, 50);
            validationErrors.ValidateMaxLength("ShipCity", ShipCity, 50);
            validationErrors.ValidateMaxLength("ShipStateOrProvince", ShipStateOrProvince, 50);
            validationErrors.ValidateMaxLength("ShipZipOrPostalCode", ShipZipOrPostalCode, 20);
            validationErrors.ValidateMaxLength("ShipCountryId", ShipCountryId, 5);
            validationErrors.ValidateMaxLength("PhoneNumber", PhoneNumber, 24);
            validationErrors.ValidateMaxLength("Email", Email, 50);
            validationErrors.ValidateMaxLength("Headline", Headline, 500);
            validationErrors.ValidateMaxLength("Website", Website, 100);
            validationErrors.ValidateMaxLength("BlogURL", BlogURL, 400);
            validationErrors.ValidateMaxLength("FacebookPageId", FacebookPageId, 100);
            validationErrors.ValidateMaxLength("TwitterUserId", TwitterUserId, 100);
            validationErrors.ValidateMaxLength("DeviceId", DeviceId, 500);
            validationErrors.ValidateNotNull("FirstName", FirstName);
            validationErrors.ValidateNotNull("LastName", LastName);
            validationErrors.ValidateNotNull("LoginId", LoginId);
            validationErrors.ValidateNotNull("LanguageId", LanguageId);
        }
        
        public void PerformAddOperationValidations(List<ValidationError> validationErrors, String plainTextPassword1, String plainTextPassword2)
        {
            PerformGeneralValidations(validationErrors);

            validationErrors.ValidateNotNull("PasswordHash", PasswordHash);
            validationErrors.ValidateNotNull("PasswordSalt", PasswordSalt);

            ValidatePassword(validationErrors, plainTextPassword1, plainTextPassword2);
        }

        public void ValidatePassword(List<ValidationError> validationErrors, String plainTextPassword1, String plainTextPassword2)
        {
            validationErrors.ValidateNotNull("Password1", plainTextPassword1);
            validationErrors.ValidateNotNull("Password2", plainTextPassword2);
            validationErrors.ValidateMinLength("Password1", plainTextPassword1, 6);
            validationErrors.ValidateMinLength("Password2", plainTextPassword2, 6);

            if (plainTextPassword1 != plainTextPassword2)
            {
                String message = ("Service.User.PasswordConfirmationError");

                validationErrors.Add(new ValidationError(message, "Password2"));
            }
        }

        // Take a plain text password and return a salted SHA-512 hash
        private Byte[] HashPassword(String plainTextPassword)
        {
            Encoding encoding = Encoding.Unicode;
            Byte[] passwordBytes = encoding.GetBytes(plainTextPassword ?? String.Empty);
            Byte[] saltedBytes = new Byte[PasswordSalt.Length + passwordBytes.Length];
            SHA512 hasher = new SHA512Managed();

            PasswordSalt.CopyTo(saltedBytes, 0);
            passwordBytes.CopyTo(saltedBytes, PasswordSalt.Length);

            return hasher.ComputeHash(saltedBytes);
        }

        // Verify that the given plain text password matches the pre-existing password hash
        public bool VerifyPassword(String plainTextPassword)
        {
            Byte[] hash = HashPassword(plainTextPassword);

            return Enumerable.SequenceEqual(hash, PasswordHash);            
        }

        // Set the new password, this method also generates new cryptographically secure salt
        public void SetPassword(String plainTextPassword)
        {
            using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
            {
                PasswordSalt = new Byte[16];
                random.GetBytes(PasswordSalt);
            }

            PasswordHash = HashPassword(plainTextPassword);
        }

        // Create mobile / API authentication token for this user
        public void CreateToken(DateTime now)
        {            
            // If the old token did not expire, then reuse its , otherwise generate new GUID         
            if (TokenExpirationTime == null || TokenExpirationTime < now ||
                Token == null || Token == Guid.Empty)
                Token = Guid.NewGuid();

            TokenExpirationTime = now.AddSeconds(5184000); // Set 60 day expiration time
        }

        // Extend the expiration date of the mobile/API authenticate token for this user
        public void ExtendToken(DateTime now)
        {            
            DateTime newExpirationTime = now.AddSeconds(5184000);
            TimeSpan difference = newExpirationTime - TokenExpirationTime;

            if (difference.TotalSeconds < 3600)
            {
                return;
            }

            TokenExpirationTime = newExpirationTime;
            
            this.Set("TokenExpirationTime", this.TokenExpirationTime);
            this.Set("Token", this.Token);
        }

        // Get the mobile/API authentication token for the specified user
        public static Guid GetToken(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                      .Match("(user:User)")
                      .Where((User user) => user.Id == userId)
                      .Return((user) => new
                      {
                          Token = Return.As<Guid>("user.Token")
                      })
                      .Results.Single().Token;
            }
            catch
            {
                return Guid.Empty; // Token does not exist, return empty GUID.
            }
        }

        // Master function that performs all of the logic to setup a new user account
        public void RegisterUser(String password1, String password2)
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
                        
            SetPassword(password1);
            PerformAddOperationValidations(validationErrors, password1, password2);
            validationErrors.ThrowValidationException();
                        
            // Name Capitalization Validation
            this.FirstName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.FirstName.ToLower()); // ensure user first name is "Title Case"
            this.LastName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.LastName.ToLower()); // ensure user last name is "Title Case"

            // Tutorial Mode / Login Count
            this.TutorialMode = true;
            this.LoginCount = 0;
            this.TutorialStep = 0;
            
            // Create database node for user
            this.Create();

            // DEPRECATED & SAVED
            // Setup user scroll store
            /*ScrollStore store = new ScrollStore();
            store.Id = Guid.NewGuid();
            store.UserId = this.Id;

            store.Create();

            Graph.Instance.Cypher
                .Match("(user:User)")
                .Where((User user) => user.Id == this.Id)
                .Match("(ss:ScrollStore)")
                .Where((ScrollStore ss) => ss.Id == store.Id)
                .Create("(user)-[:SCROLL]->(ss)")
                .ExecuteWithoutResults();*/

            //CreateInbox();

            // Create user notification queue
            Graph.Instance.Cypher
                .Match("(u:User)")
                .Where((User u) => u.Id == this.Id)
                .Create("(u)-[:NOTIFICATIONS]->(n:Notifications)")
                .Set("n.Id = {id}")
                .WithParam("id", this.Id)
                .ExecuteWithoutResults();
                
            // Create Birthday wishlu
            Wishlu birthday = Wishlu.CreateBirthdayWishLu(this.Id, this.GetNextBirthday().Value);            
            this.BirthdayWishLuId = birthday.Id;
                        
            // Create Just Me wishlu
            Wishlu justme = Wishlu.CreateJustMeWishLu(this.Id);
            this.JustMeWishLuId = justme.Id;
            
            /*
            // Create public wishlu
            Wishlu publiclu = Wishlu.CreatePublicWishLu(this.Id);
            this.PublicWishLuId = publiclu.Id;
            */
 
            // Create Friends wishloop
            Wishloop allfriends = Wishloop.CreateAllFriendsWishloop(this.Id);

            // Automatically subscribe "My Friends" wishloop to Birthday
            birthday.AddSubscriber(allfriends.Id);
            
            // Send e-mail verification    
            try
            {
                this.SendEmailVerification();
            }
            catch { Logger.Error("There was an error sending validation e-mail."); }

            // Setup E-Mail Notifications
            SetupNotifications();  
            
            // Update the user
            this.Update();
        }

        public void SendEmailVerification()
        {
            // E-Mail Verification
            this.Verified = false;
            this.Set("Verified", false);
            this.VerificationCode = Guid.NewGuid();
            this.Set("VerificationCode", this.VerificationCode);

            new Mail.MailController().ConfirmEmail(this).Deliver();                        
        }

        public Inbox GetInbox()
        {
            try // to get the user's inbox
            {
                return Graph.Instance.Cypher
                     .Match("(user:User)-[:RECEIVER]->(box:Inbox)")
                     .Where((User user) => user.Id == this.Id)
                     .Return(box => box.As<Inbox>())
                     .Results.Single();
            }
            catch // that the inbox does not exist or there was an error, so let's create a new inbox
            {
                return CreateInbox();
            }
        }
                
        private Inbox CreateInbox()
        {
            // Setup user inbox
            Inbox inbox = new Inbox();
            inbox.Id = Guid.NewGuid();

            inbox.Create();
            inbox.AddReceiver(this.Id);

            return inbox;
        }

        // Return friend's wishlus with an event date that are in the future
        public static List<Wishlu> GetUpcomingWishlus(Guid userId)
        {
            try
            {
                List<Wishlu> result = new List<Wishlu>();

                result.AddRange(Wishlu.GetUsersWishLus(userId).Where(x => x.EventDateTime.HasValue &&  x.EventDateTime.Value.Date >= DateTimeOffset.Now.Date));

                result.AddRange(Graph.Instance.Cypher
                    .Match("(user:User)-[:FRIEND]-(friend:User)-[:CREATED]-(w:Wishlu)")
                    .Where((User user) => user.Id == userId)
                    .AndWhere((Wishlu w) => w.EventDateTime != null)
                    .ReturnDistinct(w => w.As<Wishlu>())
                    .Results.Where(x => x.EventDateTime.HasValue && x.EventDateTime.Value >= DateTimeOffset.Now.Date));

                return result;
            }
            catch { return new List<Wishlu>(); }
        }

        public static List<MappedWishlu> GetMappedUpcomingWishlus(Guid userId)
        {
            try
            {
                List<MappedWishlu> result = new List<MappedWishlu>();

                result.AddRange(Wishlu.GetUsersMappedWishLus(userId).Where(x => x.EventDateTime.HasValue && x.EventDateTime.Value.Date >= DateTimeOffset.Now.Date));

                result.AddRange(Graph.Instance.Cypher
                    .Match("(user:User {Id:{userid}})-[:FRIEND]-(friend:User)-[:CREATED]-(w:Wishlu)")
                    .WithParam("userid", userId)
                    .Where("(has(w.EventDateTime) AND w.EventDateTime > {now})")                    
                    .WithParam("now", DateTimeOffset.Now)
                    .With("DISTINCT friend,w")
                    .Return(() => Return.As<MappedWishlu>("{Id: w.Id, Name: w.Name, WishLuType: w.WishLuType, Visibility: w.Visibility, UserId: w.UserId, EventDateTime: w.EventDateTime, DisplayColor: w.DisplayColor, UserFullName: (friend.FirstName + ' ' + friend.LastName), UserProfileImage: friend.ImageUrl}"))
                    .Results.ToList());

                return result;
            }
            catch { return new List<MappedWishlu>(); }
        }

        // Return current user's friend's wishlus with an event date that are in the future
        public List<Wishlu> GetUpcomingWishlus()
        {
            try
            {
                List<Wishlu> result = new List<Wishlu>();

                result.AddRange(Wishlu.GetUsersWishLus(this.Id).Where(x => x.EventDateTime.HasValue && x.EventDateTime.Value.Date >= DateTimeOffset.Now.Date));

                result.AddRange(Graph.Instance.Cypher
                    .Match("(user:User)-[:FRIEND]-(friend:User)-[:CREATED]-(w:Wishlu)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((Wishlu w) => w.EventDateTime != null)
                    .ReturnDistinct(w => w.As<Wishlu>())
                    .Results.Where(x => x.EventDateTime.HasValue && x.EventDateTime.Value.Date >= DateTimeOffset.Now.Date));

                return result;
            }
            catch { return new List<Wishlu>(); }
        }

        // Return the current user's birthday wishlu ID
        public Guid GetBirthdayWishLuId()
        {
            if (this.BirthdayWishLuId == null || this.BirthdayWishLuId == Guid.Empty)
            {
                Wishlu birthday = Wishlu.GetUsersBirthdayWishLu(this.Id);
                this.BirthdayWishLuId = birthday.Id;
                this.Update();
            }
            
            return this.BirthdayWishLuId;
        }

        // Return the current user's just me wishlu ID
        public Guid GetJustMeWishLuId()
        {
            if (this.JustMeWishLuId == null || this.JustMeWishLuId == Guid.Empty)
            {
                Wishlu floating = Wishlu.GetUsersJustMeWishLu(this.Id);
                this.JustMeWishLuId = floating.Id;
                this.Update();
            }

            return this.JustMeWishLuId;
        }

        // Return the inbox of the given user, if it doesn't exist, create it
        public static Inbox GetUsersInbox(Guid id)
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(user:User)-[:RECEIVER]->(box:Inbox)")
                     .Where((User user) => user.Id == id)
                     .Return(box => box.As<Inbox>())
                     .Results.First();
            }
            catch
            {
                // Setup user inbox
                Inbox inbox = new Inbox();
                inbox.Id = Guid.NewGuid();

                inbox.Create();
                inbox.AddReceiver(id);

                return inbox;
            }
        }

        // Text the current user
        public void SendText(string subject, string text)
        {
            // send text via e-mail
            string addy = "";
            string num = this.PhoneNumber.Trim().Replace("(", "").Replace(")", "").Replace("-", "").Replace("_", "").Trim();

            switch (this.PhoneCarrier)
            {
                // AT&T
                case 1:
                    addy = String.Format("{0}@txt.att.net", num);
                    break;
                // T-Mobile
                case 2:
                    addy = String.Format("{0}@tmomail.net", num);
                    break;
                // Verizon
                case 3:
                    addy = String.Format("{0}@vtext.com", num);
                    break;
                // Sprint
                case 4:
                    addy = String.Format("{0}@messaging.sprintpcs.com", num);
                    break;
                // Virgin Mobile
                case 5:
                    addy = String.Format("{0}@vmobl.com", num);
                    break;
                // Tracfone
                case 6:
                    addy = String.Format("{0}@mmst5.tracfone.com", num);
                    break;
                // Metro PCS
                case 7:
                    addy = String.Format("{0}@mymetropcs.com", num);
                    break;
                // Boost Mobile
                case 8:
                    addy = String.Format("{0}@myboostmobile.com", num);
                    break;
                // Cricket
                case 9:
                    addy = String.Format("{0}@sms.mycricket.com", num);
                    break;
                // Nextel
                case 10:
                    addy = String.Format("{0}@messaging.nextel.com", num);
                    break;
                // Alltel
                case 11:
                    addy = String.Format("{0}@message.alltel.com", num);
                    break;
                // Ptel
                case 12:
                    addy = String.Format("{0}@ptel.com", num);
                    break;
                // Suncom
                case 13:
                    addy = String.Format("{0}@tms.suncom.com", num);
                    break;
                // Qwest
                case 14:
                    addy = String.Format("{0}@qwestmp.com", num);
                    break;
                // U.S. Cellular
                case 15:
                    addy = String.Format("{0}@email.uscc.net", num);
                    break;
            }

            // send message                               
            SmtpClient smtp = new SmtpClient("smtpout.secureserver.net");
            NetworkCredential cred = new NetworkCredential("no-reply@wishlu.com", "ballsOfSteel#34");
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = cred;

            MailMessage msg = new MailMessage();
            msg.Subject = subject;
            msg.Body = text;
            msg.To.Add(addy);
            msg.From = new MailAddress("no-reply@wishlu.com", "wishlu");

            smtp.Send(msg);
        }

        // Text the current user with the default subject "wishlu"
        public void SendText(string text)
        {
            SendText("wishlu", text);
        }

        // Returns true if current user's notification queue node exists, if not, tries to create it and returns true, else returns false
        private bool NotificationQueueExists()
        {
            int count = 0;

            count = (int)Graph.Instance.Cypher
                .Match("(u:User {Id:{userid}})-[:NOTIFICATIONS]-(n:Notifications)")
                .WithParam("userid", this.Id)
                .Return(n => n.Count())
                .Results.Single();

            if (count > 0)
                return true;

            try
            {
                Graph.Instance.Cypher
                    .Match("(u:User {Id:{userid}})")
                    .WithParam("userid", this.Id)
                    .Merge("(u)-[:NOTIFICATIONS]-(n:Notifications)")                    
                    .ExecuteWithoutResults();

                /*Graph.Instance.Cypher
                    .Match("(u:User),(n:Notifications)")
                    .Where((User u) => u.Id == this.Id)
                    .AndWhere("n.Id = {id}")
                    .WithParam("id", this.Id)
                    .Create("(u)-[:NOTIFICATIONS]->(n)")
                    .ExecuteWithoutResults();*/

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Push a notification to a user, at this point a user will actually see a notification, receive an e-mail, get a text, etc.
        public static bool PushNotification(Notification note)
        {
            User user = GetUserById(note.UserId);

            if (!user.NotificationQueueExists()) // if the user's notification queue doesn't exist and couldn't be created, we can't proceed
                return false;

            //Logger.Log("Pushing notification, notification queue exists for user.");

            try
            {
                Graph.Instance.Cypher
                    .Match("(u:User {Id:{userid}})-[:NOTIFICATIONS]-(q:Notifications)", "(n:Notification {Id:{noteid}})")
                    .WithParam("userid", user.Id)
                    .WithParam("noteid", note.Id)
                    .Merge("(q)-[:NOTIFICATION]-(n)")                    
                    .ExecuteWithoutResults();

                return true;
            }
            catch { return false; }
        }

        // Get all of the notifications for the given user
        public static List<Notification> GetNotifications(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(u:User {Id:{userid}})-[:NOTIFICATIONS]-(q:Notifications)-[:NOTIFICATION]-(n:Notification)")
                    .WithParam("userid", userId)
                    .ReturnDistinct(n => n.As<Notification>())
                    .OrderByDescending("n.SendTime")
                    .Results.ToList();
            }
            catch
            {
                return new List<Notification>();
            }
        }

        public static List<MappedNotification> GetMappedNotifications(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(u:User {Id:{userid}})-[:NOTIFICATIONS]-(q:Notifications)-[:NOTIFICATION]-(n:Notification)")
                    .WithParam("userid", userId)
                    .With("DISTINCT n")
                    .Match("(s:User {Id:n.SenderId})")
                    .With("DISTINCT n,s")
                    .Return(() => Return.As<MappedNotification>("{Id: n.Id, SenderId: n.SenderId, UserId: n.UserId, Content: n.Content, SendTime: n.SendTime, ReadTime: n.ReadTime, NotificationType: n.NotificationType, Read: n.Read, Url: n.Url, SenderProfileImage: s.ImageUrl, SenderFullName: collect(s.FirstName + ' ' + s.LastName)}"))
                    .OrderByDescending("n.SendTime")
                    .Results.ToList();
            }
            catch
            {
                return new List<MappedNotification>();
            }
        }

        // Get the number of notifications the given user has not yet read
        public static int GetUnreadNotificationsCount(Guid userId)
        {
            try
            {
                return (int)Graph.Instance.Cypher
                    .Match("(u:User {Id:{userid}})-[:NOTIFICATIONS]-(q:Notifications)-[:NOTIFICATION]-(n:Notification {Read:false})")
                    .WithParam("userid", userId)
                    .ReturnDistinct(n => n.CountDistinct())
                    .Results.Single();
            }
            catch
            {
                return 0;
            }
        }

        // DEPRECATED & SAVED
        /*public ScrollStore GetScrollStore()
        {            
            try
            {
                return Graph.Instance.Cypher
                     .Match("(user:User)-[:SCROLL]->(sc:ScrollStore)")
                     .Where((User user) => user.Id == this.Id)
                     .Return(sc => sc.As<ScrollStore>())
                     .Results.First();
            }
            catch
            {
                ScrollStore store = new ScrollStore();
                store.Id = Guid.NewGuid();
                store.UserId = this.Id;

                store.Create();

                Graph.Instance.Cypher
                        .Match("(user:User)")
                        .Where((User user) => user.Id == this.Id)
                        .Match("(ss:ScrollStore)")
                        .Where((ScrollStore ss) => ss.Id == store.Id)
                        .Create("(user)-[:SCROLL]->(ss)")
                        .ExecuteWithoutResults();

                return store;
            }
        }*/
        
        // If the current user has an active password reset, send the email
        private void SendResetPasswordEMail()
        {
            if (!this.InReset)
                return;

            new Squid.Mail.MailController().PasswordResetEmail(this).Deliver();
        }

        // Initiate a password reset, generate a new code, set expiratino 24 hours from now, and send e-mail
        private void InitiatePasswordReset(DateTime now)
        {
            this.InReset = true;
            this.ResetCode = Guid.NewGuid();
            this.ResetExpiration = DateTimeOffset.Now.AddHours(24);

            this.Update();

            SendResetPasswordEMail();
        }
                
        // Initiate password reset for the given user
        public static void InitiatePasswordReset(String loginId, DateTime now)
        {
            User user = GetUserByLoginId(loginId);
            user.InitiatePasswordReset(now);
        }

        public static void InitiatePasswordReset(String loginId)
        {
            InitiatePasswordReset(loginId, DateTime.Now);
        }
                
        public void UpdatePassword(String plainTextPassword1, String plainTextPassword2)
        {
            SetPassword(plainTextPassword1);

            List<ValidationError> validationErrors = new List<ValidationError>();

            ValidatePassword(validationErrors, plainTextPassword1, plainTextPassword2);
            validationErrors.ThrowValidationException();

            this.Set("PasswordHash", this.PasswordHash);
            this.Set("PasswordSalt", this.PasswordSalt);
        }
                          
        // Called from password reset form. Verifies reset code, if successful sets the user's new password
        private void ResetPassword(Guid resetPasswordId, String plainTextPassword1, String plainTextPassword2, DateTime now)
        {
            if (this.InReset == false || this.ResetCode == Guid.Empty)
                throw new PasswordResetLinkExpiredException("The password reset token has expired, been used, or is invalid.");

            if (this.ResetExpiration < now)
                throw new PasswordResetLinkExpiredException("It has been more than 24 hours since you requested your password to be reset, so it has expired. If this was simply a mistake, repeat the steps of forgot password."); // Expired

            if (this.ResetCode != resetPasswordId)
                throw new PasswordResetLinkExpiredException("The password reset code provided has expired or is incorrect."); 

            UpdatePassword(plainTextPassword1, plainTextPassword2);

            this.InReset = false;
            this.ResetCode = Guid.Empty;
            this.ResetExpiration = DateTimeOffset.MinValue;

            this.Update();
        }

        // Sets the given user's new password if it is a legitimate reset request scenario
        public static void ResetPassword(String loginId, Guid resetPasswordId, String plainTextPassword1, String plainTextPassword2, DateTime now)
        {
            User user = GetUserByLoginId(loginId);
            user.ResetPassword(resetPasswordId, plainTextPassword1, plainTextPassword2, now);
        }

        // Sets the given user's new password if it is a legitimate reset request scenario. Lookup user by e-mail address
        public static void ResetPassword(String loginId, Guid resetPasswordId, String plainTextPassword1, String plainTextPassword2)
        {
            ResetPassword(loginId, resetPasswordId, plainTextPassword1, plainTextPassword2, DateTime.Now);
        }

        // Sets the given user's new password if it is a legitimate reset request scenario. Lookup user by ID
        public static void ResetPassword(Guid userId, Guid resetPasswordId, String plainTextPassword1, String plainTextPassword2)
        {
            User user = GetUserById(userId);
            user.ResetPassword(resetPasswordId, plainTextPassword1, plainTextPassword2, DateTime.Now);
        }

        public new static User GetById(Guid id)
        {
            return GraphObject.GetById<User>(id);
        }
                
        // Returns the user with the given ID, else returns null
        public static User SearchForUserByUserId(Guid id)
        {                
            try
            {
                return Graph.Instance.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.Id == id)
                    .Return(u => u.As<User>())
                    .Results.Single();
            }
            catch { return null; }            
        }

        // Returns the user with the given ID, else throws exception
        public static User GetUserById(Guid userId)
        {
            User User = SearchForUserByUserId(userId);

            if (User != null)
                return User;

            String message = "The specified user could not be found: " + userId;

            throw new ItemNotFoundException(message);
        }

        // Helper Function //
        // Guarantees a login ID (or other similar identifier) is cleaned up, cast to lowercase, and trimmed of excess whitespace.
        internal static string CleanUpLoginId(String loginId)
        {
            if (String.IsNullOrEmpty(loginId))
                return ""; // Protects against null strings

            return loginId.ToLower().Trim();
        }
        
        // Returns the user with the given login ID / e-mail address, otherwise returns null
        public static User SearchForUserByLoginId(String loginId)
        {
            if (string.IsNullOrEmpty(loginId))
                return null;
                                                
            try
            {
                loginId = CleanUpLoginId(loginId);

                return Graph.Instance.Cypher
                .Match("(user:User)")
                 .Where((User user) => user.LoginId == loginId)
                 .Return(user => user.As<User>())
                 .Results.Single();
            }
            catch (Exception ex)
            {
                // This isn't an error I feel we need to log. This will happen all the time.
                //Logger.Error(ex);
                return null; 
            }
        }

        // Get's the user that exists with the given '@' handle / username from neo4j. If none exists, returns null.
        public static User GetUserByHandle(string handle)
        {
            handle = handle.ToLower().Replace("@",""); // remove @ incase it gets passed in

            try
            {
                return Graph.Instance.Cypher
                .Match("(user:User)")
                .Where((User user) => user.Handle == handle)
                .Return(user => user.As<User>())
                .Results.Single();
            }
            catch { return null; }
        }

        // Returns true if a given '@' handle / username is available. False if not, and false if an error occurs.
        public static bool IsHandleAvailable(string handle)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                    .Where((User user) => user.Handle == handle)
                    .Return(user => user.Count())
                    .Results.Single() == 0;
            }
            catch
            {
                return false; // an error occurred, so we shouldn't continue setting a username regardless of it being available
            }
        }

        // Returns the user with the given login ID / e-mail address, otherwise throws an exception
        public static User GetUserByLoginId(String loginId)
        {
            User user = SearchForUserByLoginId(loginId);

            if (user != null)
                return user;

            String message = String.Format("The user login ID: {0} does not exist. No user returned.", loginId);

            throw new LoginIdNotFoundException(message);
        }
        
        // Returns the user with the given Facebook ID, returns null in all other situations
        public static User SearchForUserByFacebookId(String facebookId)
        {
            facebookId = CleanUpLoginId(facebookId);

            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.FacebookPageId == facebookId)
                     .Return(user => user.As<User>())
                     .Results.Single();
            }
            catch { return null; }
        }

        // Returns the user with the given Twitter ID, returns null in all other situations
        public static User SearchForUserByTwitterId(String twitterId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.TwitterUserId == twitterId)
                     .Return(user => user.As<User>())
                     .Results.Single();
            }
            catch { return null; }
        }

        // Helper Function, Non-Throwing //
        // Returns the user with the given Google/Google+ ID, returns null in all other situations
        public static User SearchForUserByGoogleId(String plusId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.GooglePlusId == plusId)
                     .Return(user => user.As<User>())
                     .Results.Single();
            }
            catch { return null; }
        }

        // Helper Function, Non-Throwing //
        // Returns true if a user with the given unique ID exists, otherwise returns false. Returns false on all exceptions.
        public static bool UserExists(Guid id)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.Id == id)
                     .Return(user => user.Count())
                     .Results.Single() > 0;
            }
            catch { return false; }
        }

        // Helper Function, Non-Throwing //
        // Returns true if a user with the given Facebook ID exists, otherwise returns false. Returns false on all exceptions.
        public static bool UserExists(String facebookId)
        {
            facebookId = CleanUpLoginId(facebookId);

            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.FacebookPageId == facebookId)
                     .Return(user => user.Count())
                     .Results.Single() > 0;
            }
            catch { return false; }
        }

        // Returns true if a user with the given handle / username exists, otherwise false
        public static bool HandleExists(String handle)
        {
            handle = CleanUpLoginId(handle);

            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.Handle == handle)
                     .Return(user => user.Count())
                     .Results.Single() > 0;
            }
            catch { return false; }
        }

        // Returns true if a user with the given login ID / e-mail address exists, otherwise false
        public static bool LoginIdExists(string loginid)
        {
            loginid = CleanUpLoginId(loginid);

            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.LoginId == loginid)
                     .Return(user => user.Count())
                     .Results.Single() > 0;
            }
            catch { return false; }
        }

        // Returns all users that match the provided collection of Facebook IDs. Returns empty list if empty list is provided. Returns empty list on all exceptions.
        public static List<User> GetFacebookUsers(List<string> ids)
        {
            try
            {
                if (ids.Count > 0)
                {                    
                    return Graph.Instance.Cypher
                        .Match("(u:User)")                        
                        .Where("u.FacebookPageId IN {fbids}")
                        .WithParam("fbids", ids)
                        .ReturnDistinct(u => u.As<User>())
                        .Results.ToList();                    
                }
                else                
                    return new List<User>();                
            }
            catch { return new List<User>(); }
        }

        // Returns true if a user exists with the given Twitter ID, otherwise false
        public static bool UserExistsTwitter(String twitterId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.TwitterUserId == twitterId)
                     .Return(user => user.Count())
                     .Results.Single() > 0;
            }
            catch { return false; }
        }

        // Returns true if a user exists with the given Google/Google+ ID, otherwise false
        public static bool UserExistsGoogle(String plusId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.GooglePlusId == plusId)
                     .Return(user => user.Count())
                     .Results.Single() > 0;
            }
            catch { return false; }
        }

        // Framework Function, Throwing //
        // Returns a user if one exists for the given Facebook ID. Throws a user-friendly exception.
        public static User GetUserByFacebookId(String facebookId)
        {
            User user = SearchForUserByFacebookId(facebookId);

            if (user != null)
                return user;

            String message = String.Format("The specified Facebook ID: {0} is not linked to any user account.", facebookId);

            throw new LoginIdNotFoundException(message);
        }

        // Framework Function, Throwing //
        // Returns a user if one exists for the given Twitter ID. Throws a user-friendly exception.
        public static User GetUserByTwitterId(String twitterId)
        {
            User user = SearchForUserByTwitterId(twitterId);

            if (user != null)
                return user;

            String message = String.Format("The specified Twitter ID: {0} is not linked to any user account.", twitterId);

            throw new LoginIdNotFoundException(message);
        }

        // Framework Function, Throwing //
        // Returns a user if one exists for the given Google/Google+ ID. Throws a user-friendly exception.
        public static User GetUserByGoogleId(String plusId)
        {
            User user = SearchForUserByGoogleId(plusId);

            if (user != null)
                return user;

            String message = String.Format("The specified Google+ ID: {0} is not linked to any user account.", plusId);

            throw new LoginIdNotFoundException(message);
        }

        // Mobile/API Function, Non-Throwing //
        // Returns a user if one exists for the given authorization token. Returns null on all exceptions.                
        public static User SearchForUserByToken(Guid token)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)")
                     .Where((User user) => user.Token == token)
                     .Return(user => user.As<User>())
                     .Results.Single();
            }
            catch { return null; }
        }
        
        // Returns a list of the given user's friends. If no friends, or an error occurs, returns an empty list
        public static List<User> GetUsersFriends(Guid userId)
        {                                    
            try
            {
                return Graph.Instance.Cypher
                        .OptionalMatch("(user:User)-[:FRIEND]-(friend:User)")
                        .Where((User user) => user.Id == userId)
                        .ReturnDistinct(friend => friend.As<User>())
                        .Results.ToList();
            }
            catch
            {
                return new List<User>(0);
            }
        }

        // Social Function, Non-Throwing //
        // Returns a list of a user's "Friends of Friends" circle / suggested friends, ordered descending by the number of mutual friends.
        public static List<User> GetSuggestedFriends(Guid userId)
        {            
            try
            {
                return Graph.Instance.Cypher
                        .Match("(user:User)")
                        .Where((User user) => user.Id == userId)
                        .Match("(user)-[:FRIEND]-(friend:User)-[r:FRIEND]-(fof:User)")
                        .Where("NOT (user)-[:FRIEND]-(fof)")
                        .AndWhere("NOT (user)-[:BLOCKED]-(fof)")
                        .AndWhere((User fof) => fof.Id != userId)
                        .ReturnDistinct((fof, r) => new { fof = fof.As<User>(), r = r.Count() })
                        .OrderByDescending("count(r)")
                        .Results.Select(x => x.fof).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return new List<User>();
            }
        }

        // Returns a list of all of the connection IDs of all of the given user's friends. This is utilized with SignalR for real-time push communication.
        public static List<string> GetUsersFriendsConnectionIds(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .OptionalMatch("(user:User)-[:FRIEND]-(friend:User)")
                    .Where((User user) => user.Id == userId)
                    .Return((friend) => Return.As<List<string>>("friend.ConnectionIds"))
                    .Results.ToList().SelectMany(x => x).ToList();
            }
            catch { return new List<string>(); }           
        }

        // Returns a list of the given user's inbox messages, sorted with newest first.
        public static List<Message> GetUsersMessages(Guid userId)
        {            
            List<Message> messagesList = GetUsersInbox(userId).GetMessages();
            messagesList = messagesList.OrderBy(x => x.SendTime).Reverse().ToList();

            return messagesList;
        }
                
        // Debug Function, Throwing //
        // This is not to be used for production. This was a test function for returning a list of ALL wishlu users. 
        public static List<User> DumpUsers()
        {
            return Graph.Instance.Cypher
                .Match("(user:User)")
                 .Return(user => user.As<User>())
                 .Results.ToList();
        }
         
        // Social Function, Non-Throwing //
        // Searches for any and all users matching full name, first name, last name, e-mail, birth date, or phone number in the given search query.
        private static Regex _emailRegex = new Regex("[-0-9a-zA-Z.+_]+@[-0-9a-zA-Z.+_]+\\.[a-zA-Z]{2,4}");
        private static Regex _dateRegex = new Regex(@"^((0?[13578]|10|12)(-|\/)(([1-9])|(0[1-9])|([12])([0-9]?)|(3[01]?))(-|\/)((19)([2-9])(\d{1})|(20)([01])(\d{1})|([8901])(\d{1}))|(0?[2469]|11)(-|\/)(([1-9])|(0[1-9])|([12])([0-9]?)|(3[0]?))(-|\/)((19)([2-9])(\d{1})|(20)([01])(\d{1})|([8901])(\d{1})))$");
        private static Regex _phoneRegex = new Regex("[0-9]+");
        public List<User> Find(string query)
        {
            // internal search query
            List<User> results = doFind(query);
            
            // return empty results if we found no one
            if (results.Count == 0)
                return results;

            // remove users from results if we blocked them or they blocked us
            results.RemoveAll(x => x.IsBlocked(this.Id));

            return results;
        }

        private static List<User> doFind(string query)
        {      
            query = query.Trim();

            if (_emailRegex.IsMatch(query)) // input was an e-mail address
            {
                try 
                {
                    return Graph.Instance.Cypher
                        .Match("(user:User)")
                        .Where((User user) => user.LoginId == query)
                        .OrWhere((User user) => user.Email == query)
                        .AndWhere("user.Privacy_AllowEMailLookup = true")
                        .Return(user => user.As<User>())
                        .Results.ToList();
                }
                catch { return new List<User>(); }
            }
            else if (_dateRegex.IsMatch(query.Replace(".","/").Replace("-","/"))) // input was a birthday
            {
                query = query.Replace(".","/").Replace("-","/");

                DateTimeOffset dto = DateTimeOffset.Parse(query);

                try
                {
                    return Graph.Instance.Cypher
                        .Match("(user:User)")
                        .Where((User user) => user.DateOfBirth.ToString() == dto.Date.ToString())                        
                        .Return(user => user.As<User>())
                        .Results.ToList();
                }
                catch { return new List<User>(); }
            }
            else if (_phoneRegex.IsMatch(query.Replace("(","").Replace(")","").Replace("-","").Trim()))
            {
                try
                {
                    query = query.Replace("(", "").Replace(")", "").Replace("-", "").Trim();

                    return Graph.Instance.Cypher
                        .Match("(user:User)")
                        .Where((User user) => user.PhoneNumber == query)
                        .AndWhere("user.Privacy_AllowPhoneLookup = true")
                        .Return(user => user.As<User>())
                        .Results.ToList();
                }
                catch { return new List<User>(); }
            }
            else 
            {
                try
                {
                    query = WebUtility.HtmlDecode(query);
                    query = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query);

                    ICypherFluentQuery q = Graph.Instance.Cypher
                        .Match("(user:User)")
                        .Where((User user) => user.FullName == query)
                        .OrWhere((User user) => user.FirstName == query)
                        .OrWhere((User user) => user.LastName == query)
                        .OrWhere((User user) => user.Handle == query.ToLower())
                        .OrWhere((User user) => user.Id.ToString() == query);

                    List<string> tokens = query.Split(new Char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    if (tokens != null && tokens.Count > 0)
                    {
                        foreach (string token in tokens)
                        {
                            string t = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(token);

                            q = q.OrWhere((User user) => user.FullName == t)
                            .OrWhere((User user) => user.FirstName == t)
                            .OrWhere((User user) => user.LastName == t)
                            .OrWhere((User user) => user.Handle == t)
                            .OrWhere((User user) => user.Id.ToString() == t.ToLower());
                        }
                    }

                    return q
                        .ReturnDistinct(user => user.As<User>())
                        .Results.ToList();
                        
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return new List<User>();
                }
            }
        }
        
        // Returns a list of users who can see the given wish
        public static List<User> GetWishsFriends(Guid wishId)
        {
            return GetWishLusFriends(Wish.GetAssignmentId(wishId));
        }

        // Returns a list of users who can see the given wishlu
        public static List<User> GetWishLusFriends(Guid wishLuId)
        {
            try
            {
                List<User> ret = new List<User>();

                List<Wishloop> loops = Wishlu.GetWishloops(wishLuId);

                foreach (Wishloop loop in loops)
                {
                    ret.AddRange(loop.GetMembers());
                }

                return ret;
            }
            catch { return new List<User>(); }
        }

        // Increase the count for number of times logged in
        private void IncrementLoginCounter()
        {
            try
            {
                this.LoginCount = LoginCount + 1;
                this.Set("LoginCount", this.LoginCount);
            }
            catch { }
        }

        // Performs a "login", simply throwing halting exceptions if a user account is inactive or the given password is invalid/incorrect.
        public void Login(String password)
        {
            if (!IsActive)            
                throw new UserInactiveException("Service.User.UserIsInactive");
            
            if (!VerifyPassword(password))
                throw new PasswordErrorException("Invalid password.");

            IncrementLoginCounter();
        }
        
        // Performs a Facebook "login". Performs an OpenGraph call and verifies the authenticated user is returned a matching user account from the database.
        public static User FacebookLoginStatic(String token)
        {
            WebClient client = new WebClient();
            string JsonResult = client.DownloadString(string.Concat(
                   "https://graph.facebook.com/me?access_token=", token));

            JObject jsonUserInfo = JObject.Parse(JsonResult);

            string username = jsonUserInfo.Value<string>("username");
            string email = jsonUserInfo.Value<string>("email");
            string locale = jsonUserInfo.Value<string>("locale");
            string facebook_userID = jsonUserInfo.Value<string>("id");

            User user = GetUserByFacebookId(facebook_userID);

            if (!user.IsActive)
                throw new UserInactiveException("Service.User.UserIsInactive");

            user.IncrementLoginCounter();

            if(user.ImageUrl == "//assets.wishlu.com/images/GenericFriend.png")
            {
                user.ImageUrl = string.Format("//graph.facebook.com/{0}/picture", facebook_userID);
                user.Set("ImageUrl", user.ImageUrl);
            }
            
            return user;
        }
        
        // Performs a Twitter "login". Returns user account associated with the authenticated Twitter user.
        public static User TwitterLoginStatic(TweetSharp.TwitterUser user)
        {
            User ret = GetUserByTwitterId(user.Id.ToString());

            if (!ret.IsActive)
                throw new UserInactiveException("Service.User.UserIsInactive");

            ret.IncrementLoginCounter();

            return ret;
        }

        // Performs a Google "login". Returns user account associated with the authenticated Google user.
        public static User GoogleLoginStatic(string plusId)
        {
            User ret = GetUserByGoogleId(plusId);

            if (!ret.IsActive)
                throw new UserInactiveException("Service.User.UserIsInactive");

            ret.IncrementLoginCounter();

            return ret;
        }

        // Returns the user logged in from the given login ID / e-mail address and password. If user does not exist, returns null.
        public static User Login(String loginId, String password)
        {
            User user = GetUserByLoginId(loginId);

            if (user == null)
                return null;

            user.Login(password);

            return user;
        }

        // Returns the user logged in from the given username / handle and password. If user does not exist, returns null.
        public static User LoginHandle(String username, String password)
        {
            User user = GetUserByHandle(username);

            if (user == null)
                return null;

            user.Login(password);

            return user;
        }
        
        // Calculate the current user's next birthday (either this year if it has not yet passed, or next year if it has already happened)
        public static DateTime CalculateNextBirthday(DateTime dateOfBirth, DateTime currentDateTime)
        {
            DateTime birthDate = dateOfBirth.Date;
            DateTime today = currentDateTime.Date;
            int years = today.Year - birthDate.Year;
            DateTime nextBirthday = birthDate.AddYears(years);

            if (nextBirthday < today)
                nextBirthday = nextBirthday.AddYears(1);
            return nextBirthday;
        }

        // Return the user's next birthday with correct year
        public DateTime? GetNextBirthday()
        {
            if (DateOfBirth == null)
                return null;
            return CalculateNextBirthday(DateOfBirth.DateTime, DateTime.Now);
        }

        // Helper Functions for Invitation Code Generation
        private static string CreateKey()
        {
            int length = 5;

            const string valid = "abcdefghjkmnpqrstuvwxyz123456789";
            StringBuilder res = new StringBuilder();

            while (0 < length--)
            {
                var bytes = new byte[1];
                do
                {
                    new RNGCryptoServiceProvider().GetBytes(bytes);
                }
                while (!IsValidIndex(bytes[0], valid.Length));

                res.Append(valid[bytes[0] % valid.Length]);
            }
            return res.ToString();
        }

        private static bool IsValidIndex(byte index, int length)
        {
            int fullSet = Byte.MaxValue / length;

            return index < length * fullSet;
        }

        public static string GetInvitationCode(Guid userId)
        {
            try
            {
                string code = Graph.Instance.Cypher
                    .Match("(user:User)-[:INVITATION_CODE]-(inv)")
                    .Where((User user) => user.Id == userId)
                    .Return((inv) => Return.As<string>("inv.Code"))
                    .Results.First();

                return code;
            }
            catch
            {
                return GenerateInvitationCode(userId);
            }
        }

        // Add a new invitation code node in the database for the given user. This facilitates a customized/streamlined registration flow, as well as automatic friendship for invited users.
        public static string GenerateInvitationCode(Guid userId)
        {            
            string code = CreateKey();
            code = code.ToLower();

            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User)")
                    .Where((User user) => user.Id == userId)
                    .Create("(inv:InvitationCode {Code: {newinv}})<-[:INVITATION_CODE]-(user)")
                    .WithParam("newinv", code)
                    .ExecuteWithoutResults();

                return code;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        // Generates an invitation code and node for the current user.
        public string GenerateInvitationCode()
        {
            return GenerateInvitationCode(this.Id);
        }

        // Returns true if the given invitation code exists, else false.
        public static bool InviteCodeExists(string code)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            code = code.ToLower();

            try
            {
                return Graph.Instance.Cypher
                    .Match("(inv:InvitationCode)")
                    .Where("inv.Code = {c}")
                    .WithParam("c", code)
                    .Return(inv => inv.Count())
                    .Results.Single() > 0;
            }
            catch { return false; }
        }

        // Delete the given invitation code
        public static bool DeleteInvitationCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            code = code.ToLower();

            try
            {
                Graph.Instance.Cypher
                    .Match("(inv:InvitationCode)-[r]-()")
                    .Where("inv.Code = {c}")
                    .WithParam("c", code)
                    .Delete("inv,r")
                    .ExecuteWithoutResults();

                return true;
            }
            catch { return false; }
        }

        // Get the user who generated the given invite code.
        public static User GetInvitingUser(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            code = code.ToLower();

            try
            {
                return Graph.Instance.Cypher
                    .Match("(inv:InvitationCode)-[:INVITATION_CODE]-(user:User)")
                    .Where("inv.Code = {c}")
                    .WithParam("c", code)
                    .Return(user => user.As<User>())
                    .Results.Single();
            }
            catch { return null; }
        }

        // Create friendship between two users. This also creates a mutual following, and mutual "all friends" wishloop membership.
        public bool CreateFriendship(Guid userId)
        {
            try
            {
                // Create friendship
                bool success = Graph.Instance.Cypher
                    .Match("(user:User),(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .Create("(user)-[r:FRIEND {Since: {cd}}]->(friend)")
                    .WithParam("cd", DateTimeOffset.Now)
                    .CreateUnique("(user)-[:FOLLOWING]->(friend)")
                    .CreateUnique("(friend)-[:FOLLOWING]->(user)")
                    .ReturnDistinct(r => r.CountDistinct())
                    .Results.Single() > 0;

                if (!success)
                    return false;

                // reciprocal "friends" wishloop membership
                Wishloop.GetAllFriendsWishloopByUserId(this.Id).AddMember(userId);
                Wishloop.GetAllFriendsWishloopByUserId(userId).AddMember(this.Id);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Notifies a user when someone they invited joins wishlu via their invitation.
        public bool Invited(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(inviter:User),(user:User)")
                    .Where((User inviter) => inviter.Id == this.Id)
                    .AndWhere((User user) => user.Id == userId)
                    .Create("(inviter)<-[:INVITED_BY]-(user)")
                    .ExecuteWithoutResults();

                Notification n = new Notification();
                n.Content = GetUserFullName(userId) + " has accepted your invitation and joined wishlu.";
                n.NotificationType = NotificationType.Info;
                n.SenderId = userId;
                n.UserId = this.Id;
                n.Url = "/u/" + userId;
                n.CreateNotification();

                return true;
            }
            catch { return false; }
        }
        
        // Sends an e-mail invite to a user. The link in the e-mail contains a unique code that bypasses registration code requirements, 
            // displays a tailored registration page, and automatically creates friendship
        public void Invite(string email)
        {
            Logger.Log("Inviting " + email);

            string code = GenerateInvitationCode();

            new Mail.MailController().InviteEmail(this, email, code).Deliver();     

            /*EmailTask et = new EmailTask();
            et.Id = Guid.NewGuid();
            et.To = email;
            et.Subject = this.FullName + " has invited you to join wishlu.";
            et.Body = "Hello,<br><br>" +
                this.FullName + " would like you to join " + (this.Gender == 'f' ? "her" : "him") + " on wishlu. <br>" +
                "<a href=\"http://www.wishlu.com/join/invite?code=" + code + "\">Join wishlu</a><br><br>";*/

            //Graph.Instance.Cypher
            //    .Create("(n:EmailTask" + DateTimeOffset.Now.ToString("MMddyy") + " {p})")
            //    .WithParam("p", et)
            //    .ExecuteWithoutResults();            
        }
                
        // Send a friend request to the given user. Also notify them of the friend request to prompt action. Returns true only if successful.
        public bool AddFriend(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User),(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .Create("(user)-[:FRIEND_REQUEST]->(friend)")
                    .ExecuteWithoutResults();

                Notification n = new Notification();
                n.Content = this.FullName + " has sent you a friend request.";
                n.NotificationType = NotificationType.FriendRequest;
                n.SenderId = this.Id;
                n.UserId = userId;
                n.CreateNotification();

                return true;
            }
            catch { return false; }
        }

        // Suggest the given friend of the current user to be friends with the suggested user. Notify them of this.
        public bool SuggestFriend(Guid friendId, Guid suggestId)
        {
            try
            {                
                Notification n = new Notification();
                n.Content = this.FullName + " has suggested you be friends with " + GetUserFullName(suggestId) + ".";
                n.NotificationType = NotificationType.SuggestedFriend;
                n.SenderId = this.Id;
                n.UserId = friendId;
                n.Url = "/friends/add/" + suggestId;
                n.CreateNotification();

                return true;
            }
            catch { return false; }
        }

        // Suggest the given friend of the current user to view the given wish. Notify them of this.
        public bool SuggestWish(Guid wishId, Guid friendId)
        {
            try
            {
                Notification n = new Notification();
                n.Content = this.FullName + " has suggested an item you may be interested in adding to one of your wishlus: " +  Wish.GetWishById(wishId).Name;
                n.NotificationType = NotificationType.SuggestedWish;
                n.SenderId = this.Id;
                n.UserId = friendId;
                n.Url = "/i/" + wishId;
                n.CreateNotification();

                return true;
            }
            catch { return false; }
        }

        // Suggest the given friend of the current user to view the given wish. Notify them of this.
        public bool SuggestProduct(Guid productId, Guid friendId)
        {
            try
            {
                Notification n = new Notification();
                n.Content = this.FullName + " has suggested a product you may be interested in adding to one of your wishlus: " + Milkshake.Search.ProductId(productId).Name;
                n.NotificationType = NotificationType.SuggestedProduct;
                n.SenderId = this.Id;
                n.UserId = friendId;
                n.Url = "/p/" + productId;
                n.CreateNotification();

                return true;
            }
            catch { return false; }
        }

        public bool LikeWish(Guid wishId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User),(w:Wish)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((Wish w) => w.Id == wishId)
                    .Create("(user)-[:LIKES]->(w)")
                    .ExecuteWithoutResults();

                return true;
            }
            catch { return false; }
        }

        public bool UnikeWish(Guid wishId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User)-[r:LIKES]-(w:Wish)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((Wish w) => w.Id == wishId)
                    .Delete("r")
                    .ExecuteWithoutResults();

                return true;
            }
            catch { return false; }
        }

        public bool LikesWish(Guid wishId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:LIKES]-(w:Wish)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((Wish w) => w.Id == wishId)
                    .Return(r => r.Count())
                    .Results.First() >= 1;                                
            }
            catch { return false; }
        }

        public List<Wish> GetLikedWishes()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:LIKES]-(w:Wish)")
                    .Where((User user) => user.Id == this.Id)
                    .Return(w => w.As<Wish>())
                    .Results.ToList();                
            }
            catch { return new List<Wish>(); }
        }

        // Removes all friendship between the curernt user and the given user. Returns true if successful. Returns false on error or if they are not currently friends.
        public bool Unfriend(Guid userId)
        {
            try
            {
                if (!IsFriend(userId))
                    return false;

                Graph.Instance.Cypher
                    .Match("(user:User)-[r:FRIEND]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .Delete("r")
                    .ExecuteWithoutResults();

                // reciprocal wishloop member removal
                WishloopMember.DeleteByWishloopOwnerUserIdAndMemberUserId(Id, userId);
                WishloopMember.DeleteByWishloopOwnerUserIdAndMemberUserId(userId, Id);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Returns true if a friend request exists in either direction between the current user and the given user
        public bool FriendRequestExists(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:FRIEND_REQUEST]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .ReturnDistinct(r => r.CountDistinct())
                    .Results.Single() > 0;
            }
            catch { return false; }
        }

        // Returns a list of the current user's incoming friend requests (from other users)
        public List<Guid> GetFriendRequests()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)<-[r:FRIEND_REQUEST]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .ReturnDistinct((friend) => Return.As<Guid>("friend.Id"))
                    .Results.ToList();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public static List<User> GetIncomingFriendRequests(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)<-[r:FRIEND_REQUEST]-(friend:User)")
                    .Where((User user) => user.Id == userId)
                    .ReturnDistinct(friend => friend.As<User>())
                    .Results.ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        // Returns a list of users who have an active friend request that they sent to the current user.
        public List<User> GetFriendRequestUsers()
        {
            return GetIncomingFriendRequests(this.Id);
        }

        public static List<User> GetRequestedFriends(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:FRIEND_REQUEST]->(friend:User)")
                    .Where((User user) => user.Id == userId)
                    .ReturnDistinct(friend => friend.As<User>())
                    .Results.ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        public List<User> GetRequestedFriends()
        {
            return GetRequestedFriends(this.Id);
        }

        
                
        // Attempt to accept a friendship from the given user. If it does not exist, returns false. If friendship cannot be created, returns false.
            // Notify sending user that current user has accepted their friend request.
        public bool AcceptFriendRequest(Guid userId)
        {
            try
            {
                if (!FriendRequestExists(userId))
                    return false;

                if (!CreateFriendship(userId))
                    return false;

                this.DeleteFriendRequest(userId);

                // accepted notification                
                Notification n = new Notification();
                n.Content = this.FullName + " has accepted your friend request.";
                n.NotificationType = NotificationType.Info;
                n.SenderId = this.Id;
                n.UserId = userId;
                n.Url = "/u/" + this.Id;
                n.CreateNotification();
                                
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Cancel a friend request that the current user sent out.
        public bool CancelFriendRequest(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User)-[r:FRIEND_REQUEST]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .Delete("r")
                    .ExecuteWithoutResults();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Delete a friend request that the current user has received. Also delete the corresponding notification.
        public bool DeleteFriendRequest(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User)-[r:FRIEND_REQUEST]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .Delete("r")
                    .ExecuteWithoutResults();

                Notification.DeleteNotification(this.Id, userId);

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // Update the current user's node in the database. Performs validations. Throwing.
        public override void Update()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            base.Update();            
        }
       
       
        // Returns the profile image URL (Imgur) of the given user
        public static string GetUserProfileImage(Guid id)
        {
            try
            {
                return Graph.Instance.Cypher
                      .Match("(user:User)")
                      .Where((User user) => user.Id == id)
                      .Return((user) => new
                          {
                              ImageUrl = Return.As<string>("user.ImageUrl")
                          })
                      .Results.Single().ImageUrl;
            }
            catch
            {
                return "//assets.wishlu.com/images/GenericProfile.png";
            }
        }

        // Returns the formatted full name of the given user
        public static string GetUserFullName(Guid id)
        {
            try
            {
                var a = Graph.Instance.Cypher
                      .Match("(user:User)")
                      .Where((User user) => user.Id == id)
                      .Return((user) => new
                      {
                          FirstName = Return.As<string>("user.FirstName"),
                          LastName = Return.As<string>("user.LastName")
                      })
                      .Results.Single();

                return a.FirstName + " " + a.LastName;
            }
            catch
            {
                return "";
            }
        }

        // Returns true if the given user and the current user are friends.
        public bool IsFriend(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                    .OptionalMatch("(user:User)-[r:FRIEND]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .ReturnDistinct(r => r.CountDistinct())
                    .Results.Single() > 0;
            }
            catch
            {
                return false;
            }                        
        }

        // Returns true if the given user is friends with any of the current user's friends.
        public bool IsFriendOfFriend(Guid friendUserId)
        {
            try
            {
                return Graph.Instance.Cypher
                      .OptionalMatch("(user:User)-[r:FRIEND*2..2]-(friend:User)")
                      .Where((User user) => user.Id == this.Id)
                      .AndWhere((User friend) => friend.Id == friendUserId)
                      .Return(r => r.Count())
                      .Results.Single() > 0;
            }
            catch
            {
                return false;
            }
        }

        // Returns true if the current usewr is following the given user.
        public bool IsFollowing(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                      .OptionalMatch("(user:User)-[r:FOLLOWING]->(friend:User)")
                      .Where((User user) => user.Id == this.Id)
                      .AndWhere((User friend) => friend.Id == userId)
                      .Return(r => r.Count())
                      .Results.Single() > 0;
            }
            catch
            {
                return false;
            }
        }

        // Returns a list of all users that are following the current user.
        public List<User> GetFollowers()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)<-[:FOLLOWING]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .Return(friend => friend.As<User>())
                    .Results.ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        // Returns the number of users following the current user.
        public int GetFollowersCount()
        {
            try
            {
                return (int)Graph.Instance.Cypher
                    .Match("(user:User)<-[:FOLLOWING]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .ReturnDistinct(friend => friend.CountDistinct())
                    .Results.Single();
            }
            catch
            {
                return 0;
            }
        }

        // Returns a list of all of the users that the current user is following.
        public List<User> GetFollowing()
        {
            try
            {
                return Graph.Instance.Cypher
                        .Match("(user:User)-[:FOLLOWING]->(friend:User)")
                        .Where((User user) => user.Id == this.Id)
                        .Return(friend => friend.As<User>())
                        .Results.ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        // Returns the number of users the current user is following.
        public int GetFollowingCount()
        {
            try
            {
                return (int)Graph.Instance.Cypher
                        .Match("(user:User)-[:FOLLOWING]->(friend:User)")
                        .Where((User user) => user.Id == this.Id)
                        .ReturnDistinct(friend => friend.CountDistinct())
                        .Results.Single();
            }
            catch
            {
                return 0;
            }
        }

        // Returns a list of all of the given user's wishes (disregards any privacy or wishloop/wishlu systems)
        public static List<Wish> GetUsersWishes(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                        .Match("(user:User)-[:CREATED]->(lu:Wishlu)-[:CONTAINS_WISH]->(wish:Wish)")
                        .Where((User user) => user.Id == userId)                        
                        .ReturnDistinct(wish => wish.As<Wish>())                        
                        .Results.ToList();
            }
            catch { return new List<Wish>(); }
        }

        // Returns a limited set list of wishes from user accounts the current user is following.
            // This method is utilized to create the dashboard "feed"
        public List<FeedItem> GetFollowingWishes()
        {
            try
            {
                return Graph.Instance.Cypher
                        .Match("(user:User)-[:FOLLOWING]->(friend:User)-[:CREATED]->(lu:Wishlu)-[:CONTAINS_WISH]->(wish:Wish)")
                        .Where((User user) => user.Id == this.Id)
                        .AndWhere((Wishlu lu) => lu.WishLuType.ToString() != "JustMe")
                        .ReturnDistinct((wish, lu, friend) => new FeedItem{
                            Wish = wish.As<Wish>(),
                            WishLu = lu.As<Wishlu>(),
                            User = friend.As<User>()
                        })
                        .OrderByDescending("wish.CreatedOn")
                        .Limit(50)
                        .Results.ToList();
            }
            catch { return new List<FeedItem>(); }
        }

        // Returns a limited set list of wishes from user accounts the given user is following.
        public static List<FeedItem> GetFollowingWishes(Guid userId)
        {
            try
            {
                return Graph.Instance.Cypher
                        .Match("(user:User)-[:FOLLOWING]->(friend:User)-[:CREATED]->(lu:Wishlu)-[:CONTAINS_WISH]->(wish:Wish)")
                        .Where((User user) => user.Id == userId)
                        .AndWhere((Wishlu lu) => lu.WishLuType.ToString() != "JustMe")
                        .ReturnDistinct((wish, lu, friend) => new FeedItem
                        {
                            Wish = wish.As<Wish>(),
                            WishLu = lu.As<Wishlu>(),
                            User = friend.As<User>()
                        })
                        .OrderByDescending("wish.CreatedOn")
                        .Limit(50)
                        .Results.ToList();
            }
            catch
            {
                return new List<FeedItem>();
            }
        }

        // Generates and returns a  "filler feed" containing Milkshake products from a selection of stores.
        // NOTE: 5 products each from 10 stoires for a total of 50 products.
        public static List<Milkshake.Product> GetFillerFeed()
        {
            try
            {
                List<Milkshake.Store> stores = Milkshake.Store.GetStores();

                stores = stores.OrderBy(x => Guid.NewGuid()).Take(10).ToList();

                List<Milkshake.Product> products = new List<Milkshake.Product>();

                foreach (Milkshake.Store store in stores)
                {
                    List<Milkshake.Product> temp = store.GetProducts().OrderBy(x => Guid.NewGuid()).Take(5).ToList();                                        
                    products.AddRange(temp);                    
                }

                while (products.Count < 50)
                {
                    stores = Milkshake.Store.GetStores();

                    stores = stores.OrderBy(x => Guid.NewGuid()).Take(10).ToList();

                    foreach (Milkshake.Store store in stores)
                    {
                        List<Milkshake.Product> temp = store.GetProducts().OrderBy(x => Guid.NewGuid()).Take(5).ToList();
                        products.AddRange(temp);
                    }
                }

                return products;
            }
            catch
            {
                return new List<Milkshake.Product>();
            }
        }

        public List<Milkshake.Store> GetFavoriteStores()
        {
            return Milkshake.Store.GetStores(this.FavoriteStores);
        }

        // Returns a pageable list of following wishes. This function facilitates endless scroll on the dashboard feed.
        public List<FeedItem> GetFollowingWishes(int page = 0)
        {
            try
            {
                return Graph.Instance.Cypher
                        .Match("(user:User)-[:FOLLOWING]->(friend:User)-[:CREATED]->(lu:Wishlu)-[:CONTAINS_WISH]->(wish:Wish)")
                        .Where((User user) => user.Id == this.Id)
                        .AndWhere((Wishlu lu) => lu.WishLuType.ToString() != "JustMe")
                        .ReturnDistinct((wish, lu, friend) => new FeedItem
                        {
                            Wish = wish.As<Wish>(),
                            WishLu = lu.As<Wishlu>(),
                            User = friend.As<User>()
                        })
                        .OrderByDescending("wish.CreatedOn")
                        .Skip(50*page)
                        .Limit(50)                        
                        .Results.ToList();
            }
            catch
            {
                return new List<FeedItem>();
            }
        }

        // Returns a pageable list of following wishes for the given user.
        public static List<FeedItem> GetFollowingWishes(Guid userId, int page = 0)
        {
            try
            {
                return Graph.Instance.Cypher
                        .Match("(user:User)-[:FOLLOWING]->(friend:User)-[:CREATED]->(lu:Wishlu)-[:CONTAINS_WISH]->(wish:Wish)")
                        .Where((User user) => user.Id == userId)
                        .AndWhere((Wishlu lu) => lu.WishLuType.ToString() != "JustMe")
                        .ReturnDistinct((wish, lu, friend) => new FeedItem
                        {
                            Wish = wish.As<Wish>(),
                            WishLu = lu.As<Wishlu>(),
                            User = friend.As<User>()
                        })
                        .OrderByDescending("wish.CreatedOn")
                        .Skip(50 * page)
                        .Limit(50)
                        .Results.ToList();
            }
            catch
            {
                return new List<FeedItem>();
            }
        }

        // Current user follows the given user.
        public void Follow(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User),(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .CreateUnique("(user)-[:FOLLOWING]->(friend)")
                    .ExecuteWithoutResults();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        // Unfollow the given user.
        public void Unfollow(Guid userId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User)-[r:FOLLOWING]->(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userId)
                    .Delete("r")
                    .ExecuteWithoutResults();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        // Block the given user. This removes any friendship or pending friend requests. Search results, public wishes, profile data, etc. will be mutually hidden.
            // Only the blocking user or an administrator can undo this.
        public bool Block(Guid userid)
        {
            try
            {
                this.Unfollow(userid);
                this.Unfriend(userid);
                this.DeleteFriendRequest(userid);
                this.CancelFriendRequest(userid);

                Graph.Instance.Cypher
                    .Match("(user:User)", "(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userid)
                    .Create("(user)-[:BLOCKED]->(friend)")
                    .ExecuteWithoutResults();
                                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return false;
            }
        }

        // Unblock the given user from the current user's block list.
        public bool Unblock(Guid userid)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(user:User)-[r:BLOCKED]->(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userid)
                    .Delete("r")
                    .ExecuteWithoutResults();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return false;
            }
        }

        // Returns true if the current user has blocked the given user, or vice versa.
        public bool IsBlocked(Guid userid)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[r:BLOCKED]-(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .AndWhere((User friend) => friend.Id == userid)                    
                    .ReturnDistinct(r => r.CountDistinct())
                    .Results.Single() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return false;
            }
        }
        
        // Returns a list of all user IDs the current user has blocked.
        public List<Guid> GetBlockedUserIds()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[:BLOCKED]->(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .Return((friend) => Return.As<Guid>("friend.Id"))
                    .Results.ToList();                    
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return new List<Guid>();
            }
        }

        // Returns a list of all user IDs the given user has blocked.
        public static List<Guid> GetBlockedUserIds(Guid userid)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[:BLOCKED]->(friend:User)")
                    .Where((User user) => user.Id == userid)
                    .Return((friend) => Return.As<Guid>("friend.Id"))
                    .Results.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return new List<Guid>();
            }
        }

        // Returns a list of all the users the current user has blocked.
        public List<User> GetBlockedUsers()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[:BLOCKED]->(friend:User)")
                    .Where((User user) => user.Id == this.Id)
                    .Return(friend => friend.As<User>())
                    .Results.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return new List<User>();
            }
        }

        // Returns a list of all the users the given user has blocked
        public static List<User> GetBlockedUsers(Guid userid)
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(user:User)-[:BLOCKED]->(friend:User)")
                    .Where((User user) => user.Id == userid)
                    .Return(friend => friend.As<User>())
                    .Results.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return new List<User>();
            }
        }

        // Add the given store to the current user's list of favorite stores / stores they like.
        public void FollowStore(Guid id)
        {
            try
            {
                if (!FavoriteStores.Contains(id))
                    FavoriteStores.Add(id);

                this.Set("FavoriteStores", FavoriteStores);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        // Remove the given store from the current user's list of favorite stores.
        public void UnfollowStore(Guid id)
        {
            try
            {
                if (FavoriteStores.Contains(id))
                    FavoriteStores.Remove(id);

                this.Set("FavoriteStores", FavoriteStores);
            }
            catch (Exception ex) { Logger.Error(ex.ToString()); }
        }
                
        // Returns true if the given user has gifted the given wish, only if not canceled.
        public bool HasGifted(Wish wish)
        {
            foreach (Gift g in wish.GetAllGifts())
                if (g.GiverId == this.Id && g.Status != GiftStatus.Canceled)
                    return true;

            return false;
        }

        public List<Gift> GetGiftsGiven()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(u:User)-[:GIVER]->(g:Gift)")
                    .Where((User u) => u.Id == this.Id)
                    .Return(g => g.As<Gift>())
                    .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }

        public List<Gift> GetGiftsReceived()
        {
            try
            {
                return Graph.Instance.Cypher
                    .Match("(u:User)<-[:RECEIVER]->(g:Gift)")
                    .Where((User u) => u.Id == this.Id)
                    .Return(g => g.As<Gift>())
                    .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }
                
        // Returns true if the given wishloop ID is a wishloop created by the current user.
        public bool IsUsersWishloop(Guid wishloopId)
        {
            try
            {
                Wishloop wishloop = Wishloop.GetWishloopById(wishloopId);

                if (wishloop == null)
                    return false;

                return wishloop.UserId == Id;
            }
            catch
            {
                return false;
            }
        }

        // Returns a list of all of the current user's wishloops that contain the given user.
        public List<Wishloop> GetWishloopsByMember(Guid memberId)
        {
            return Wishloop.GetWishloopsByMember(this.Id, memberId);
        }

        // Adds the given friend to the given wishloop. Throws an exception if the wishloop does not belong to the current user.
        private void AddFriendToWishloop(Guid friendUserId, Guid wishloopId)
        {
            if (!IsUsersWishloop(wishloopId))
            {
                String message = "Service.User.NotUsersWishloop";

                throw new NotUsersWishloopException(message, "Wishloop ID: " + wishloopId.ToString());
            }

            Wishloop.AddMember(friendUserId, wishloopId);
        }

        // Adds the given friend to all of the given current user's wishloops.
        public void AddFriendToWishloops(Guid friendUserId, List<Guid> wishloopIds)
        {            
            if (!IsFriend(friendUserId))
            {
                String message = "Service.User.NotAFriend";

                throw new NotAFriendException(message, "Friend ID: " + friendUserId.ToString());
            }

            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            foreach (Guid wishloopId in wishloopIds)
                AddFriendToWishloop(friendUserId, wishloopId);
            //transaction.Commit();
            //}
        }

        public static void AddFriendToWishloops(Guid userId, Guid friendUserId, List<Guid> wishloopIds)
        {
            User user = GetUserById(userId);
            user.AddFriendToWishloops(friendUserId, wishloopIds);
        }
                
        // Removes the given user from the given current user's wishloop.
        private void RemoveFriendFromWishloop(Guid friendUserId, Guid wishloopId)
        {
            if (!IsUsersWishloop(wishloopId))
            {
                String message = "Service.User.NotUsersWishloop";

                throw new NotUsersWishloopException(message, "Wishloop ID: " + wishloopId.ToString());
            }

            Wishloop.RemoveMember(friendUserId, wishloopId);
        }

        public void RemoveFriendFromWishloops(Guid friendUserId, List<Guid> wishloopIds)
        {            
            //using (DatabaseTransactionGuard transaction = new DatabaseTransactionGuard(databaseInstance))
            //{
            foreach (Guid wishloopId in wishloopIds)
                RemoveFriendFromWishloop(friendUserId, wishloopId);
            //transaction.Commit();
            //}
        }

        public static void RemoveFriendFromWishloops(Guid userId, Guid friendUserId, List<Guid> wishloopIds)
        {
            User user = GetUserById(userId);
            user.RemoveFriendFromWishloops(friendUserId, wishloopIds);
        }
                
        // Uploads the given image data to Imgur and sets the user's profile image.
        public void SetUsersImage(Byte[] imageDataBytes)
        {            
            this.ImageUrl = Services.Imgur.Imgur.UploadImage(imageDataBytes);
            this.Set("ImageUrl", this.ImageUrl);
        }

        // Decode the given Base64 encoded string, then upload and set the user's profile image.
        public void SetUsersImage(String imageDataBase64String)
        {
            Debug.Assert(!String.IsNullOrEmpty(imageDataBase64String));

            Byte[] imageDataBytes = Convert.FromBase64String(imageDataBase64String);

            SetUsersImage(imageDataBytes);
        }
                        
        // Create the queue of Fantine e-mail tasks for the current user's new user e-mail flow.
        public void SetupNotifications()
        {
            // E-Mail 2 (Day 1) (Send Now
            EmailTask em = new EmailTask();
            em.Id = this.Id;

            try
            {
                new Mail.MailController().NewUserEmail(this, 1).Deliver();
            }
            catch { Logger.Error("There was an error sending the new user email."); }
            //em.EmailNotification = EmailNotification.Day1;                
            //Graph.Instance.Cypher
            //    .Create("(n:EmailTask" + DateTime.Now.ToString("MMddyy") + " {p})")
            //    .WithParam("p", em)
            //    .ExecuteWithoutResults();
            
            // Remaining e-mails, schedule for future delivery via Fantine

            try
            {
                // E-Mail 3 (Day 2)
                em.EmailNotification = EmailNotification.Day2;
                Graph.Instance.Cypher
                    .Create("(n:EmailTask" + DateTime.Now.AddDays(1).ToString("MMddyy") + " {p})")
                    .WithParam("p", em)
                    .ExecuteWithoutResults();

                // E-Mail 4 (Day 3)
                em.EmailNotification = EmailNotification.Day3;
                Graph.Instance.Cypher
                    .Create("(n:EmailTask" + DateTime.Now.AddDays(2).ToString("MMddyy") + " {p})")
                    .WithParam("p", em)
                    .ExecuteWithoutResults();

                // E-Mail 5 (Day 5)
                em.EmailNotification = EmailNotification.Day5;
                Graph.Instance.Cypher
                    .Create("(n:EmailTask" + DateTime.Now.AddDays(4).ToString("MMddyy") + " {p})")
                    .WithParam("p", em)
                    .ExecuteWithoutResults();

                // E-Mail 6 (Day 8)
                em.EmailNotification = EmailNotification.Day8;
                Graph.Instance.Cypher
                    .Create("(n:EmailTask" + DateTime.Now.AddDays(7).ToString("MMddyy") + " {p})")
                    .WithParam("p", em)
                    .ExecuteWithoutResults();

                // E-Mail 7 (Day 13)
                em.EmailNotification = EmailNotification.Day13;
                Graph.Instance.Cypher
                    .Create("(n:EmailTask" + DateTime.Now.AddDays(12).ToString("MMddyy") + " {p})")
                    .WithParam("p", em)
                    .ExecuteWithoutResults();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        // Delete the current user's profile image.
        public void RemoveProfileImage()
        {
            this.ImageUrl = "";
            this.Update();
        }
                
        // System.Object overrides
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(User))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            if (Object.ReferenceEquals(this, null) || Object.ReferenceEquals(obj, null))
                return false;

            return this.Id == ((User)obj).Id;        
        }

        public override int GetHashCode()
        {
            if (Object.ReferenceEquals(this, null)) return 0;

            int hashId = this.Id == null ? 0 : this.Id.GetHashCode();

            return hashId;
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }

    public class FeedItem
    {
        public Wish Wish { get; set; }
        public Wishlu WishLu { get; set; }
        public User User { get; set; }
    }

    public class InviteFriendship
    {
        [JsonProperty("EMail")]
        public string EMail { get; set; }

        [JsonProperty("UserId")]
        public Guid UserId { get; set; }
    }

    public class UserComparer : IEqualityComparer<User>
    {
        public bool Equals(User x, User y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id;
        }

        public int GetHashCode(User user)
        {
            if (Object.ReferenceEquals(user, null)) return 0;

            int hashId = user.Id == null ? 0 : user.Id.GetHashCode();

            return hashId;
        }
    }
}