using Facebook;
using Squid.Database;
using Squid.Log;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class UserController : BaseController
    {
        //////////////////////
        // General Settings //
        //////////////////////
        [Authorize]
        public ActionResult Index()
        {
            ViewData["Selfie"] = true;

            return View("Index", CurrentUser);
        }

        [Authorize]
        public ActionResult Name(string first, string last)
        {
            if (String.IsNullOrEmpty(first) || String.IsNullOrEmpty(last))
                return Json(false, JsonRequestBehavior.AllowGet);

            try
            {
                var user = CurrentUser;

                if (first != user.FirstName)
                {
                    first = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(first);
                    user.Set("FirstName", first);
                    Session["FirstName"] = first;
                }

                if (last != user.LastName)
                {
                    last = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(last);
                    user.Set("LastName", last);
                    Session["LastName"] = last;
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult Handle(string handle)
        {
            if (String.IsNullOrEmpty(handle))
                return Json(false);

            try
            {
                if (!Squid.Users.User.IsHandleAvailable(handle))
                    return Json(false);

                var user = CurrentUser;

                if (handle != user.Handle)
                {
                    user.Set("Handle", handle);
                    Session["Handle"] = handle;
                }

                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [Authorize]
        public ActionResult Email(string email)
        {
            if (String.IsNullOrEmpty(email))
                return Json(false);

            try
            {
                var user = CurrentUser;

                if (email != user.LoginId)
                {
                    user.Set("LoginId", email);
                    Session["LoginId"] = email;
                }

                if (email != user.Email)
                {
                    user.Set("Email", email);
                    Session["Email"] = email;
                }

                // reset user e-mail verification status
                user.SendEmailVerification();

                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [Authorize]
        public ActionResult Birthday(string dob)
        {
            if (String.IsNullOrEmpty(dob))
                return Json(false);

            try
            {
                var user = CurrentUser;

                user.DateOfBirth = DateTimeOffset.Parse(dob);
                user.Set("DateOfBirth", user.DateOfBirth);

                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [Authorize]
        public ActionResult Password(string password, string confirm)
        {
            if (String.IsNullOrEmpty(password) || String.IsNullOrEmpty(confirm))
                return Json(false, JsonRequestBehavior.AllowGet);

            if (password.Length < 6 || confirm.Length < 6)
                return Json(false, JsonRequestBehavior.AllowGet);

            if (password != confirm)
                return Json(false, JsonRequestBehavior.AllowGet);

            try
            {
                var user = CurrentUser;

                user.UpdatePassword(password, confirm);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Image()
        {
            if (HttpContext.Request.Files.AllKeys.Any())
            {
                try
                {

                    var image = Request.Files["user_profile_image"];

                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.InputStream.CopyTo(ms);
                        CurrentUser.SetUsersImage(ms.ToArray());
                    }

                    return Json(new { result = true, message = "Your profile image was updated successfully." });
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return Json(new { result = false, message = "An unexpected exception occurred while setting your profile image." });
                }
            }
            else
            {
                return Json(new { result = false, message = "No image file was received on the server." });
            }
        }

        public ActionResult Favorites(FavoritesModel model)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.FavoriteThings))
                    model.FavoriteThings = "";
                                
                var user = CurrentUser;
                user.Favorites = model.FavoriteThings;
                user.Set("Favorites", user.Favorites);
                
                return Json(new { result = true, message = "Your favorite things have been updated successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Address(AddressModel model)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.AddressLine1))
                    return Json(new { result = false, message = "Please specify the first line of your address." });

                if (String.IsNullOrEmpty(model.City))
                    return Json(new { result = false, message = "Please specify your city." });

                if (String.IsNullOrEmpty(model.State))
                    return Json(new { result = false, message = "Please select your state." });

                if (String.IsNullOrEmpty(model.ZipCode))
                    return Json(new { result = false, message = "Please provide your 5-digit postal / zip code." });

                if (model.ZipCode.Length < 5)
                    return Json(new { result = false, message = "The postal / zip code you provided is too short. They must be 5 characters." });

                if (model.ZipCode.Length > 5)
                    return Json(new { result = false, message = "The postal / zip code you provided is too long. They must be 5 characters." });

                var user = CurrentUser;
                user.ShipAddress1 = model.AddressLine1;
                user.ShipAddress2 = model.AddressLine2;
                user.ShipCity = model.City;
                user.ShipStateOrProvince = model.State;
                user.ShipZipOrPostalCode = model.ZipCode;

                user.Update();

                return Json(new { result = true, message = "Your shipping address information has been updated successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        public ActionResult Stores()
        {
            var user = CurrentUser;
            List<Milkshake.Store> stores = Milkshake.Store.GetStores().OrderBy(x => x.Name).ToList();

            List<Milkshake.Store> favorites = stores.Where(x => user.FavoriteStores.Contains(x.Id)).ToList();
            stores.RemoveAll(x => user.FavoriteStores.Contains(x.Id));

            dynamic model = new ExpandoObject();
            model.Favorites = favorites;
            model.Other = stores;

            return View("stores", model);
        }

        ///////////////////////
        // Security Settings //
        ///////////////////////       
        [Authorize]
        public ActionResult Security()
        {
            return View("Security", CurrentUser);
        }

        [Authorize]
        public ActionResult Privacy()
        {
            return View("Privacy", CurrentUser);
        }

        [Authorize]
        public ActionResult Set(string key, string value, int group = 0)
        {
            try
            {
                var user = CurrentUser;
                string k = "";

                switch (group)
                {
                    // Privacy
                    case 0:
                    default:
                        k = "Privacy_";
                        break;

                    // User Stats (sizes, preferences, etc)
                    case 1:
                        k = "Stats_";
                        break;

                    // Notification Settings
                    case 2:
                        k = "Notifications_";
                        break;
                }

                k = k + key;


                user.Set(k, value);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        //////////////
        // Blocking //
        //////////////
        [Authorize]
        public ActionResult Blocking()
        {
            return View("Blocking", CurrentUser);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Block(string email)
        {
            if (String.IsNullOrEmpty(email))
                return Json(new { result = false, message = "Please specify a username or email to block." });

            var user = CurrentUser;

            try
            {
                Squid.Users.User blocked = null;
                if (Squid.Users.User.HandleExists(email))
                    blocked = Squid.Users.User.GetUserByHandle(email);
                else if (Squid.Users.User.LoginIdExists(email))
                    blocked = Squid.Users.User.GetUserByLoginId(email);

                if (blocked == null)
                    return Json(new { result = false, message = "The specified username or email does not exist." });

                user.Block(blocked.Id);

                return Json(new { result = true, message = "The specified user has been blocked. You can no longer see each other's content on wishlu, communicate via wishlu, or otherwise interact on the wishlu platform.", name = blocked.FullName, id = blocked.Id });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "An unexpected error occurred while attempting to block this user." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult BlockId(Guid id)
        {
            if (id == null || id == Guid.Empty)
                return Json(new { result = false, message = "Please specify a valid user to block." });

            var user = CurrentUser;

            try
            {
                var blocked = Squid.Users.User.GetUserById(id);
                
                if (blocked == null)
                    return Json(new { result = false, message = "The specified user does not exist." });

                user.Block(blocked.Id);

                return Json(new { result = true, message = "The specified user has been blocked. You can no longer see each other's content on wishlu, communicate via wishlu, or otherwise interact on the wishlu platform.", name = blocked.FullName, id = blocked.Id });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "An unexpected error occurred while attempting to block this user." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Unblock(Guid id)
        {
            if (id == null || id == Guid.Empty)
                return Json(new { result = false, message = "Please specify valid user ID to unblock." });

            var user = CurrentUser;

            try
            {
                Squid.Users.User blocked = Squid.Users.User.GetUserById(id);

                if (blocked == null)
                    return Json(new { result = false, message = "The specified user does not exist." });

                user.Unblock(blocked.Id);

                return Json(new { result = true, message = "The specified user has been unblocked." });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "An unexpected error occurred while attempting to block this user." });
            }
        }

        [Authorize]
        public ActionResult Notifications()
        {
            return View("Notifications", CurrentUser);
        }

        [Authorize]
        public ActionResult Mobile()
        {
            return View("Mobile", CurrentUser);
        }

        [Authorize]
        public JsonResult Phone(string number, int carrier)
        {
            string num = number.Trim().Replace("(", "").Replace(")", "").Replace("-", "").Replace("_", "").Replace("+", "").Trim();

            try
            {
                var user = CurrentUser;

                int code = new Random().Next(10000, 99999);
                user.Set("PhoneCode", code);

                user.PhoneNumber = num;
                user.PhoneCarrier = carrier;
                user.SendText("wishlu Phone Verification", "Your phone verification code is: " + code + ". If you did not prompt this, please ignore this message.");

                // reset so they are not saved before verification step
                user.PhoneNumber = "";
                user.PhoneCarrier = 0;

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public JsonResult VerifyPhone(string number, int carrier, int code)
        {
            try
            {
                var user = CurrentUser;

                int vc = Graph.Instance.Cypher
                    .Match("(u:User)")
                    .Where((Squid.Users.User u) => u.Id == user.Id)
                    .Return((u) => new
                    {
                        PhoneCode = Schloss.Data.Neo4j.Cypher.Return.As<int>("u.PhoneCode")
                    })
                    .Results.Single().PhoneCode;

                if (code != vc)
                    return Json(false, JsonRequestBehavior.AllowGet);

                user.PhoneNumber = number;
                user.PhoneCarrier = carrier;
                user.Set("PhoneCode", null);
                user.Update();

                user.SendText("Thank you for verifying. Your phone number has been successfully added to your wishlu account.");

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public JsonResult DeletePhone()
        {
            try
            {
                var user = CurrentUser;

                user.PhoneNumber = "";
                user.PhoneCarrier = 0;
                user.Update();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult FindFriends()
        {
            List<Squid.Users.User> here = new List<Squid.Users.User>();
            List<dynamic> missing = new List<dynamic>();

            var user = CurrentUser;

            if (user.IsFacebookSynced)
            {
                var accessToken = user.FacebookAccessToken;
                var client = new FacebookClient(accessToken);

                dynamic result = client.Get("me", new { fields = "friends" });

                List<String> ids = new List<string>();
                foreach (dynamic friend in result.friends.data)
                {
                    ids.Add(friend.id);
                }

                here.AddRange(Squid.Users.User.GetFacebookUsers(ids));

                here.RemoveAll(x => x.IsFriend(user.Id));
                //here.RemoveAll(x => x.FriendRequestExists(user.Id));

                foreach (dynamic friend in result.friends.data)
                {
                    string id = friend.id;

                    if (here.Exists(x => x.FacebookAccessToken == id))
                        continue;

                    missing.Add(friend);
                }
            }

            dynamic res = new ExpandoObject();
            res.here = here;
            res.missing = missing;
            res.user = CurrentUser;

            return View("FindFriends", res);
        }

        [Authorize]
        public ActionResult Social()
        {
            HttpCookie googleCookie = new HttpCookie("googleplus_state", Guid.NewGuid().ToString());
            googleCookie.Expires = DateTime.Now.AddMinutes(15);
            Response.Cookies.Add(googleCookie);

            ViewData["googleplus_state"] = googleCookie.Value;

            return View("Social");
        }

        [Authorize]
        public ActionResult Payments()
        {
            return View("Payments", CurrentUser);
        }

        [AllowAnonymous]
        public ActionResult View(Guid id)
        {
            Squid.Users.User user;
            ViewData["Selfie"] = false;

            // wishlu member, logged in
            if (Request.IsAuthenticated && GetCurrentUserId() != null)
            {                
                // viewing own profile
                if (id == CurrentUser.Id)
                {
                    ViewData["Selfie"] = true;
                    return View("View", CurrentUser);
                }
                else
                {
                    user = Squid.Users.User.GetUserById(id);

                    if (CurrentUser.IsBlocked(id))
                        return HttpNotFound();

                    switch (user.ProfileVisibility)
                    {
                        default:
                        case Squid.Users.UserPrivacy.Everyone:
                        case Squid.Users.UserPrivacy.Members:
                            return View("View", user);

                        case Squid.Users.UserPrivacy.FriendsOfFriends:
                            if (GetCurrentUser().IsFriendOfFriend(id))
                                return View("View", user);
                            break;

                        case Squid.Users.UserPrivacy.Friends:
                            if (GetCurrentUser().IsFriend(id))
                                return View("View", user);
                            break;
                    }

                    return HttpNotFound();
                }
            }

            user = Squid.Users.User.GetUserById(id);

            // anonymous user
            if (user.ProfileVisibility == Squid.Users.UserPrivacy.Everyone)
                return View("View", user);
            else
                return HttpNotFound(); // profile not visible to anonymous user           
        }

        [AllowAnonymous]
        public ActionResult ViewUsername(string username)
        {
            Squid.Users.User user = Squid.Users.User.GetUserByHandle(username);

            if (user == null)
                return HttpNotFound();

            return View(user.Id);
        }

        [Authorize]
        public ActionResult Follow(Guid id)
        {
            var user = CurrentUser;

            if (user.IsFollowing(id))
            {
                TempData["Error"] = "You are already following this person.";
                return RedirectToAction("view", new { @id = id });
            }

            var friend = Squid.Users.User.GetUserById(id);

            switch (user.FollowPermission)
            {
                case Squid.Users.UserPrivacy.Friends:
                    if (!user.IsFriend(id))
                    {
                        TempData["Error"] = "This user only accepts follow requests from friends.";
                        return RedirectToAction("view", new { @id = id });
                    }
                    break;

                case Squid.Users.UserPrivacy.FriendsOfFriends:
                    if (!user.IsFriendOfFriend(id))
                    {
                        TempData["Error"] = "This user only accepts follow requests from friends and friends of friends.";
                        return RedirectToAction("view", new { @id = id });
                    }
                    break;
            }

            try
            {
                CurrentUser.Follow(id);
            }
            catch (Exception e)
            {
                TempData["Error"] = "An unexpected error occurred while trying to follow this user. Details: " + e.ToString();
            }

            return RedirectToAction("view", new { @id = id });
        }

        [Authorize]
        public ActionResult Unfollow(Guid id)
        {
            CurrentUser.Unfollow(id);

            return RedirectToAction("view", new { @id = id });
        }

        [Authorize]
        public ActionResult Edit(Guid id)
        {
            if (id == CurrentUser.Id)
                return View("Edit", CurrentUser);
            else
                return View("View", Squid.Users.User.GetUserById(id));
        }

        [Authorize]
        public ActionResult Friends(Guid id)
        {
            if (id == CurrentUser.Id)
                return RedirectToAction("index", "friends");

            return View("Friends", Squid.Users.User.GetUserById(id));
        }

        //[Authorize]
        //public ActionResult View()
        //{
        //    return View("View", Session["UID"]);
        //}

        [Authorize]
        public ActionResult UpdateAvatar()
        {
            return View("UpdateAvatar");
        }
        
        [Authorize]
        public ActionResult UpdateProfileImage(FormCollection formCollection)
        {
            //Squid.Session.WishLuSession.ResumeSession(new Guid(Session["WUSID"].ToString()));

            Squid.Users.User wlUser = Squid.Users.User.GetUserById(GetCurrentUserId());

            if ((formCollection["imgbase64"]).Length > 10)
            {
                wlUser.SetUsersImage(formCollection["imgbase64"].Substring(22));
            }

            return View("Index");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Verify()
        {
            string[] keys = Request.QueryString["token"].Split('!');
            ViewBag.UserID = keys[0];
            ViewBag.Token = keys[1];

            string userid = keys[0];
            string token = keys[1];

            Squid.Users.User user = Squid.Users.User.GetUserById(Guid.Parse(userid));

            if (!user.Verified)
            {
                if (Guid.Parse(token) == user.VerificationCode)
                {
                    user.Verified = true;
                    user.VerificationCode = Guid.Empty;

                    user.Set("Verified", true);
                    user.Set("VerificationCode", Guid.Empty);

                    TempData["SuccessMessage"] = "Thank you for verifying your email address.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid verification token.";
                }
            }
            else
            {
                TempData["WarningMessage"] = "You have already verified your account.";
            }

            return RedirectToAction("signin", "home");
        }
    }

    [Serializable]
    public class AddressModel
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    [Serializable]
    public class FavoritesModel
    {
        public string FavoriteThings { get; set; }
    }
}