using Squid.Log;
using System;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using TweetSharp;

namespace Disco.Controllers
{
    public class TwitterController : BaseController
    {
        private string _consumerKey = "gCCWHgXOECoSLWl3WMIQ";
        private string _consumerSecret = "Cq1LV53RMWVGbYY37ktRDh7yWuAIPj5Gb6MLqPcc";

        [AllowAnonymous]
        public ActionResult Authenticate()
        {
            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);

            var url = Url.Action("signin", "twitter", null, "http");

            OAuthRequestToken requestToken = service.GetRequestToken(url);

            Uri uri = service.GetAuthenticationUrl(requestToken);
            return new RedirectResult(uri.ToString(), false);
        }

        [AllowAnonymous]
        public ActionResult Authorize()
        {
            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);

            var url = Url.Action("register", "twitter", null, "http");

            OAuthRequestToken requestToken = service.GetRequestToken(url);

            Uri uri = service.GetAuthorizationUri(requestToken);
            return new RedirectResult(uri.ToString(), false);
        }

        [Authorize]        
        public ActionResult Connect()
        {
            //if (Request.IsAuthenticated)
            //    return RedirectToAction("index", "dash");

            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);

            var url = Url.Action("link", "twitter", null, "http");

            OAuthRequestToken requestToken = service.GetRequestToken(url);

            Uri uri = service.GetAuthorizationUri(requestToken);
            return new RedirectResult(uri.ToString(), false);
        }

        [Authorize]
        public ActionResult Link(string oauth_token, string oauth_verifier)
        {            
            Logger.Log("Twitter:Authenticate has been entered. oauth_token: " + oauth_token + " oauth_verifier: " + oauth_verifier);

            Squid.Users.User wlUser = new Squid.Users.User();

            var requestToken = new OAuthRequestToken { Token = oauth_token };

            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);
            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);

            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

            Session["tw_access_token"] = accessToken;

            Logger.Log("Twitter Access Token has been obtained. Token: " + accessToken.Token);

            TwitterUser user = service.VerifyCredentials(new VerifyCredentialsOptions());

            Logger.Log("Twitter user verified. Id: " + user.Id + " Handle: " + user.ScreenName);

            wlUser = GetCurrentUser();

            wlUser.TwitterAccessToken = accessToken.Token;
            wlUser.TwitterSecretToken = accessToken.TokenSecret;
            wlUser.TwitterUserId = user.Id.ToString();
            wlUser.Set("TwitterAccessToken", accessToken.Token);
            wlUser.Set("TwitterSecretToken", accessToken.TokenSecret);
            wlUser.Set("TwitterUserId", user.Id.ToString());

            return RedirectToAction("social", "user");
        }

        [Authorize]
        public ActionResult Disconnect()
        {
            Squid.Users.User wlUser = GetCurrentUser();

            wlUser.TwitterAccessToken = "";
            wlUser.TwitterSecretToken = "";
            wlUser.TwitterUserId = "";
            wlUser.Set("TwitterAccessToken", "");
            wlUser.Set("TwitterSecretToken", "");
            wlUser.Set("TwitterUserId", "");

            return RedirectToAction("social", "user");
        }

        [AllowAnonymous]
        public
        ActionResult
        SignIn(string oauth_token, string oauth_verifier)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            Logger.Log("Twitter:SignIn has been entered. oauth_token: " + oauth_token + " oauth_verifier: " + oauth_verifier);

            Squid.Users.User wlUser = new Squid.Users.User();

            var requestToken = new OAuthRequestToken { Token = oauth_token };

            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);
            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);
            
            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);
            
            Session["tw_access_token"] = accessToken;
            
            Logger.Log("Twitter Access Token has been obtained. Token: " + accessToken.Token);

            TwitterUser user = service.VerifyCredentials(new VerifyCredentialsOptions());

            Logger.Log("Twitter user verified. Id: " + user.Id + " Handle: " + user.ScreenName);
            
            try
            {
                wlUser = Squid.Users.User.TwitterLoginStatic(user);
            }
            catch
            {                
                TempData["ErrorMessage"] = "The Twitter account you have attempted to login with is not associated with any wishlu account. Please login with your username / e-mail and go to Social under Settings to connect with Twitter.";
                return RedirectToAction("signin", "home");
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

                Logger.Log("ReturnUrl was null. Redirected to Dash:Index");

                return RedirectToAction("index", "dash");
            }

            Logger.Error("Login was successful but the user is inactive. Redirecting user to homepage.");
            ViewBag.WarningMessage = "Twitter login has been processed, but user is inactive.";
            return RedirectToAction("index","home");
        }
                
        [AllowAnonymous]
        public
        ActionResult
        Register(string oauth_token, string oauth_verifier)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            Logger.Log("Twitter:Register has been entered. oauth_token: " + oauth_token + " oauth_verifier: " + oauth_verifier);
                       
            var requestToken = new OAuthRequestToken { Token = oauth_token };

            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);
            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);

            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

            Session["tw_access_token"] = accessToken;

            Logger.Log("Twitter Access Token has been obtained. Token: " + accessToken.Token);

            TwitterUser user = service.VerifyCredentials(new VerifyCredentialsOptions());
            string userid = user.Id.ToString();

            Logger.Log("Twitter user verified. Id: " + user.Id + " Handle: " + user.ScreenName);
                        
            // If user exists, sign in as opposed to register (this handles people who accidentally click the twitter join button as opposed to signing in)
            if (Squid.Users.User.UserExistsTwitter(userid))
            {
                Logger.Log("User already exists! Redirecting from Registration to SignIn");

                Squid.Users.User wlUser = new Squid.Users.User();

                try
                {
                    wlUser = Squid.Users.User.TwitterLoginStatic(user);
                }
                catch (Exception e)
                {
                    ViewBag.ErrorMessage = "There was an error logging in. Please check your username and password and try again." + e.ToString();
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
                else
                {
                    return RedirectToAction("index", "home");
                }
            }

            dynamic viewModel = new ExpandoObject();

            viewModel.Id = userid;
            viewModel.Token = accessToken.Token;
                                    
            return View("Register", viewModel);
        }

        [AllowAnonymous]
        public ActionResult Create(FormCollection formCollection)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            String email = formCollection["joinEMail"];
            String password = formCollection["joinPassword"];
            String firstname = formCollection["joinFirstName"];
            String lastname = formCollection["joinLastName"];
            String dob = formCollection["joinBirthday"];

            /*String key = formCollection["joinKey"];
            HashAlgorithm algorithm = SHA512.Create();
            byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(key));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            key = sb.ToString();

            string keys = System.IO.File.ReadAllText(@"C:\alphakeys.dat");

            if (!keys.Contains(key))
            {
                TempData["ErrorMessage"] = "Invalid invitation code.";
                return RedirectToAction("signin", "home");
            }

            keys = keys.Replace(key, "");*/

            bool hideage = false;

            if (formCollection["hideage"] == "true")
                hideage = true;

            char gender = 'm';

            if (formCollection["joinGender"] != null)
                gender = formCollection["joinGender"][0];

            String tid = formCollection["joinTwitterId"];
            String ttok = formCollection["joinTwitterToken"];

            Squid.Users.User wlUser = new Squid.Users.User();

            try
            {
                wlUser.Email = email;
                wlUser.LoginId = email;
                wlUser.FirstName = firstname;
                wlUser.LastName = lastname;
                wlUser.DateOfBirth = Convert.ToDateTime(dob);
                wlUser.IsActive = true;
                wlUser.IsAdminUser = false;

                wlUser.HideAge = hideage;

                wlUser.Gender = gender;

                wlUser.TwitterUserId = tid;
                wlUser.TwitterAccessToken = ttok;
                                
                wlUser.LanguageId = "en-us";
                                
                wlUser.RegisterUser(password, password);

                ViewBag.SuccessMessage = "Account successfully registered!";

                try
                {
                    wlUser = Squid.Users.User.Login(email, password);
                }
                catch (Exception e)
                {
                    Logger.Log("There was an error logging in after registration. Exception: " + e.ToString());
                    ViewBag.WarningMessage = "There was an error logging in. " + e.ToString();
                    //return Redirect("/");
                }

                PopulateSession(wlUser);
                CreateAuthTicket(wlUser);

                //System.IO.File.WriteAllText(@"C:\alphakeys.dat", keys);

                return RedirectToAction("setup", "join");                               
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = "There was an error registering the account. " + e.ToString();
                //ViewBag.LanguageResource = new LanguageResource("Web.Home");
                return RedirectToAction("index", "home");
            }
        }
    }
}