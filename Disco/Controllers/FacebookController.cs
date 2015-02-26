using Newtonsoft.Json.Linq;
using Squid.Log;
using Squid.Users;
using System;
using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class FacebookSetModel
    {
        public string AccessToken { get; set; }
        public string UID { get; set; }
    }

    public class FacebookController : BaseController
    {
        [Authorize]
        [HttpPost]
        public ActionResult Set(FacebookSetModel model)
        {
            if (ModelState.IsValid)
            {
                string access_token = "";

                access_token = Squid.Users.FacebookManager.GetExtendedAccessToken(model.AccessToken);
                Session["fb_access_token"] = access_token;
                
                var user = GetCurrentUser();

                user.FacebookAccessToken = access_token;
                user.FacebookPageId = model.UID;
                user.Update();

                return Json(new { result = true, message = "Your Facebook account has been connected successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        public ActionResult Connect(string code)
        {            
            string access_token = "";

            if (Session["fb_access_token"] == null)
            {
                string token = Squid.Users.FacebookManager.GetOauthToken(code, "https://www.wishlu.com/facebook/connect");

                access_token = Squid.Users.FacebookManager.GetExtendedAccessToken(token);

                //HttpRuntime.Cache.Insert("fb_access_token", access_token, null, DateTime.Now.AddMinutes(60), TimeSpan.Zero);

                Session["fb_access_token"] = access_token;
            }
            else
            {
                //access_token = HttpRuntime.Cache["fb_access_token"].ToString();
                access_token = (string)Session["fb_access_token"];
            }

            Logger.Log("Facebook Access Token has been obtained. Token: " + access_token);

            // Grab some facebook info
            WebClient client = new WebClient();
            string JsonResult = client.DownloadString(string.Concat(
                   "https://graph.facebook.com/me?access_token=", access_token));

            JObject jsonUserInfo = JObject.Parse(JsonResult);
                        
            string userid = jsonUserInfo.Value<string>("id");

            Squid.Users.User wlUser = GetCurrentUser();

            wlUser.FacebookAccessToken = access_token;
            wlUser.FacebookPageId = userid;
            wlUser.Set("FacebookAccessToken", access_token);
            wlUser.Set("FacebookPageId", userid);

            return RedirectToAction("social", "user");
        }

        [Authorize]
        public ActionResult Disconnect()
        {
            Squid.Users.User wlUser = GetCurrentUser();

            wlUser.FacebookAccessToken = "";
            wlUser.FacebookPageId = "";
            wlUser.Set("FacebookAccessToken", "");
            wlUser.Set("FacebookPageId", "");
            
            return RedirectToAction("social", "user");
        }

        [AllowAnonymous]
        public ActionResult SignIn(string code)
        {
            Logger.Log("Facebook:SignIn has been entered. User Access Code: " + code);

            Squid.Users.User wlUser = new Squid.Users.User();

            string access_token = "";

            if (Session["fb_access_token"] == null)
            {
                string token = Squid.Users.FacebookManager.GetOauthToken(code, "https://www.wishlu.com/facebook/signin");

                access_token = Squid.Users.FacebookManager.GetExtendedAccessToken(token);

                Session["fb_access_token"] = access_token;
            }
            else
            {
                access_token = (string)Session["fb_access_token"];
            }

            Logger.Log("Facebook Access Token has been obtained. Token: " + access_token);
                                    
            try
            {
                wlUser = Squid.Users.User.FacebookLoginStatic(access_token);
                wlUser.Set("FacebookAccessToken", access_token);
            }
            catch
            {                            
                TempData["ErrorMessage"] = "The Facebook account you have attempted to login with is not associated with any wishlu account. Please login with your username / e-mail and go to Social under Settings to connect with Facebook.";
                return RedirectToAction("signin","home");
            }

            if (wlUser.IsActive)
            {
                PopulateSession(wlUser);
                CreateAuthTicket(wlUser);

                if (wlUser.TutorialMode == true)
                {
                    if (wlUser.TutorialStep == 0)
                        return RedirectToAction("/tutorial/see");

                    if (wlUser.TutorialStep == 1)
                        return RedirectToAction("/tutorial/see");

                    if (wlUser.TutorialStep == 2)
                        return RedirectToAction("/tutorial/do");                                        
                }
                                
                Logger.Log("Login successful. User active. FormsAuthenticationTicket created. Redirecting user to URL: " + TempData["ReturnUrl"]);

                if (TempData["ReturnUrl"] != null && !String.IsNullOrEmpty(((string)TempData["ReturnUrl"]).Trim()))
                    return Redirect(TempData["ReturnUrl"].ToString());

                Logger.Log("ReturnUrl was null. Redirected to the Scroll:Index");

                return RedirectToAction("index", "dash");
            }
            Logger.Error("Login was successful but the user is inactive. Redirecting user to homepage.");
            ViewBag.WarningMessage = "The user account you have attempted to login to is inactive. Please contact support.";
            TempData["WarningMessage"] = ViewBag.ErrorMessage;
            return RedirectToAction("index","home");
        }

        [AllowAnonymous]
        public ActionResult Register(string code)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            Logger.Log("FacebookController:Register has been entered. User code: " + code);

            string access_token = "";

            if (Session["fb_access_token"] == null)
            {
                string token = Squid.Users.FacebookManager.GetOauthToken(code, "https://www.wishlu.com/facebook/register");

                access_token = Squid.Users.FacebookManager.GetExtendedAccessToken(token);

                Session["fb_access_token"] = access_token;
            }
            else
            {                
                access_token = (string)Session["fb_access_token"];
            }

            Logger.Log("Got OAuth token from FB. Token: " + access_token);

            Squid.Users.User wlUser = new Squid.Users.User();

            // Grab some facebook info
            WebClient client = new WebClient();
            string JsonResult = client.DownloadString(string.Concat(
                   "https://graph.facebook.com/me?access_token=", access_token));

            JObject jsonUserInfo = JObject.Parse(JsonResult);

            string username = jsonUserInfo.Value<string>("username");
            string email = jsonUserInfo.Value<string>("email");            
            string userid = jsonUserInfo.Value<string>("id");
            string firstname = jsonUserInfo.Value<string>("first_name");
            string lastname = jsonUserInfo.Value<string>("last_name");
            string birthday = jsonUserInfo.Value<string>("birthday");

            if (birthday == null)
                birthday = "01/01/0001";

            DateTimeOffset dob = DateTimeOffset.Parse(birthday);

            // If user exists, sign in as opposed to register (this handles people who accidentally click the facebook join button as opposed to signing in)
            if (Squid.Users.User.UserExists(userid))
            {
                Logger.Log("User already exists! Redirecting from Registration to SignIn");

                try
                {
                    wlUser = Squid.Users.User.FacebookLoginStatic(access_token);
                }
                catch (Exception e)
                {
                    ViewBag.ErrorMessage = "There was an error logging in. Please check your username and password and try again." + e.ToString();
                    TempData["ErrorMessage"] = ViewBag.ErrorMessage;
                    return RedirectToAction("index", "home");
                }

                if (wlUser.IsActive)
                {
                    PopulateSession(wlUser);
                    CreateAuthTicket(wlUser);

                    if (wlUser.TutorialMode == true)
                    {
                        if (wlUser.TutorialStep == 0)
                            return RedirectToAction("/tutorial/see");

                        if (wlUser.TutorialStep == 1)
                            return RedirectToAction("/tutorial/see");

                        if (wlUser.TutorialStep == 2)
                            return RedirectToAction("/tutorial/do");
                    }

                    return RedirectToAction("index", "dash");
                }
            }

            dynamic viewModel = new ExpandoObject();

            viewModel.Id = userid;
            viewModel.FirstName = firstname;
            viewModel.LastName = lastname;
            viewModel.Email = email;
            viewModel.Birthday = dob;
            viewModel.Token = access_token;

            return View("Register", viewModel);
        }

        [AllowAnonymous]        
        public ActionResult Invite(string key, string code)
        {
            if (Request.IsAuthenticated)
            {
                // If logged in, simply create friendship
                Squid.Users.User.GetInvitingUser(key).CreateFriendship(GetCurrentUserId());

                return RedirectToAction("index", "dash");
            }

            Logger.Log("FacebookController:Register has been entered. User code: " + code);

            string access_token = "";

            if (Session["fb_access_token"] == null)
            {
                string token = Squid.Users.FacebookManager.GetOauthToken(code, "https://www.wishlu.com/facebook/invite?key=" + key);

                access_token = Squid.Users.FacebookManager.GetExtendedAccessToken(token);
                
                Session["fb_access_token"] = access_token;
            }
            else
            {                
                access_token = (string)Session["fb_access_token"];
            }

            Logger.Log("Got OAuth token from FB. Token: " + access_token);

            Squid.Users.User wlUser = new Squid.Users.User();

            // Grab some facebook info
            WebClient client = new WebClient();
            string JsonResult = client.DownloadString(string.Concat(
                   "https://graph.facebook.com/me?access_token=", access_token));

            JObject jsonUserInfo = JObject.Parse(JsonResult);

            string username = jsonUserInfo.Value<string>("username");
            string email = jsonUserInfo.Value<string>("email");            
            string userid = jsonUserInfo.Value<string>("id");
            string firstname = jsonUserInfo.Value<string>("first_name");
            string lastname = jsonUserInfo.Value<string>("last_name");
            string birthday = jsonUserInfo.Value<string>("birthday");

            if (birthday == null)
                birthday = "01/01/0001";

            DateTimeOffset dob = DateTimeOffset.Parse(birthday);

            // If user exists, sign in as opposed to register (this handles people who accidentally click the facebook join button as opposed to signing in)
            if (Squid.Users.User.UserExists(userid))
            {
                Logger.Log("User already exists! Redirecting from Registration to SignIn");

                try
                {
                    wlUser = Squid.Users.User.FacebookLoginStatic(access_token);
                }
                catch (Exception e)
                {
                    ViewBag.ErrorMessage = "There was an error logging in. Please check your username and password and try again." + e.ToString();
                    TempData["ErrorMessage"] = ViewBag.ErrorMessage;
                    return RedirectToAction("index", "home");
                }

                if (wlUser.IsActive)
                {
                    PopulateSession(wlUser);
                    CreateAuthTicket(wlUser);

                    if (wlUser.TutorialMode == true)
                    {
                        if (wlUser.TutorialStep == 0)
                            return RedirectToAction("/tutorial/see");

                        if (wlUser.TutorialStep == 1)
                            return RedirectToAction("/tutorial/see");

                        if (wlUser.TutorialStep == 2)
                            return RedirectToAction("/tutorial/do");
                    }

                    // If already a wishlu member, simply create friendship
                    Squid.Users.User.GetInvitingUser(key).CreateFriendship(wlUser.Id);

                    return RedirectToAction("index", "dash");
                }
            }
                        
            dynamic viewModel = new ExpandoObject();

            viewModel.Id = userid;
            viewModel.FirstName = firstname;
            viewModel.LastName = lastname;
            viewModel.Email = email;
            viewModel.Birthday = dob;
            viewModel.Token = access_token;
            viewModel.Key = key;
            viewModel.Inviter = Squid.Users.User.GetInvitingUser(key);
            viewModel.Code = code;

            return View("Invite", viewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Create(FacebookCreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.Email))
                    return Json(new { result = false, message = "Please provide an e-mail address for your new account." });

                if (String.IsNullOrEmpty(model.FirstName))
                    return Json(new { result = false, message = "Please provide your first name." });

                if (String.IsNullOrEmpty(model.LastName))
                    return Json(new { result = false, message = "Please provide your last name." });

                //if (String.IsNullOrEmpty(model.Key))
                //    return Json(new { result = false, message = "You must provide an invitation code to join. This was either given to you directly or contained in your invitation link." });

                if (String.IsNullOrEmpty(model.Password))
                    return Json(new { result = false, message = "Please provide a password to use to access your account." });

                if (String.IsNullOrEmpty(model.ConfirmPassword))
                    return Json(new { result = false, message = "Please confirm your password." });

                if (model.Password != model.ConfirmPassword)
                    return Json(new { result = false, message = "The passwords you entered do not match." });

                if (model.Password.Length < 6)
                    return Json(new { result = false, message = "Your password length must be at least 6 characters." });

                if (String.IsNullOrEmpty(model.Birthday))
                    return Json(new { result = false, message = "Please provide your birthday." });

                if (model.Gender == 0 || (model.Gender != 'm' && model.Gender != 'f'))
                    return Json(new { result = false, message = "Please provide your gender." });

                bool userInvite = false;
                User inviter = null;
                if (Squid.Users.User.InviteCodeExists(model.Key))
                {
                    userInvite = true;
                    inviter = Squid.Users.User.GetInvitingUser(model.Key);
                }

                /*string keys = "";
                if (!userInvite)
                {
                    String key = model.Key;
                    HashAlgorithm algorithm = SHA512.Create();
                    byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(key));
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("X2"));
                    }
                    key = sb.ToString();

                    keys = System.IO.File.ReadAllText(@"C:\alphakeys.dat");

                    if (!keys.Contains(key))
                    {
                        return Json(new { result = false, message = "The invitation code you provided does not exist or has been previously used." });
                    }
                    keys = keys.Replace(key, "");
                }*/

                try
                {
                    Squid.Users.User wlUser = new Squid.Users.User();

                    wlUser.Email = model.Email;
                    wlUser.LoginId = model.Email;
                    wlUser.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.FirstName);
                    wlUser.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.LastName);

                    try
                    {
                        wlUser.DateOfBirth = DateTimeOffset.ParseExact(model.Birthday, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return Json(new { result = false, message = "The date of birth provided was in an invalid format." });
                    }

                    wlUser.IsActive = true;
                    wlUser.TutorialMode = true;
                    wlUser.TutorialStep = 0;

                    wlUser.Gender = model.Gender;

                    wlUser.HideAge = model.HideAge;

                    // Default the user's profile to their Facebook Profile Picture
                    wlUser.ImageUrl = string.Format("https://graph.facebook.com/{0}/picture", wlUser.FacebookPageId);

                    wlUser.FacebookPageId = model.FacebookId;
                    wlUser.FacebookAccessToken = model.FacebookToken;

                    wlUser.LanguageId = "en-us";

                    wlUser.RegisterUser(model.Password, model.ConfirmPassword);

                    try
                    {
                        wlUser = Squid.Users.User.Login(wlUser.LoginId, model.Password);
                    }
                    catch
                    {
                        return Json(new { result = false, message = "Your account has been created; however, there was an error logging into it." });
                    }

                    PopulateSession(wlUser);
                    CreateAuthTicket(wlUser);

                    if (userInvite)
                    {
                        //inviter.AddFriend(wlUser.Id); // send a friend request from inviting user to new user automatically
                        inviter.CreateFriendship(wlUser.Id); // create friendship from inviting user to new user automatically
                        inviter.Invited(wlUser.Id);                        
                    }

                    //if (!userInvite)
                    //{
                    //    System.IO.File.WriteAllText(@"C:\alphakeys.dat", keys);
                    //}

                    return Json(new { result = true, message = "Your account has been registered successfully." });
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return Json(new { result = false, message = "There was an unexpected error registering your account." });
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }
    }

    [Serializable]
    public class FacebookCreateUserModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthday { get; set; }
        public string Key { get; set; } // alpha key
        public char Gender { get; set; }
        public bool HideAge { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FacebookId { get; set; }
        public string FacebookToken { get; set; }
    }    
}