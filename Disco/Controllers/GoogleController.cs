using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Plus.v1;
using Squid.Log;
using System;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class GoogleController : BaseController
    {
        // These come from the APIs console:
        //   https://code.google.com/apis/console
        public static ClientSecrets secrets = new ClientSecrets()
        {
            ClientId = "432351898016-1g1q8vbuqhp5nui1fmeu2iuitruumd7n.apps.googleusercontent.com",
            ClientSecret = "9QwEE2Wx7qKIJmMZBZkexLOn"
        };

        // Configuration that you probably don't need to change.
        static public string APP_NAME = "wishlu Google+ Sign-In";

        // Stores token response info such as the access token and refresh token.
        private TokenResponse token;

        // Used to peform API calls against Google+.
        //private PlusService ps = null;
        
        [Authorize]
        public ActionResult Connect(Guid state, string code)
        {            
            Squid.Users.User wlUser = GetCurrentUser();

            Logger.Log("Google:Register has been entered. state: " + state + " code: " + code);

            HttpCookie googleCookie = Request.Cookies["googleplus_state"];

            // Verify a real user made this registration request and validate CSRF state token
            if (googleCookie != null && Guid.Parse(googleCookie.Value) != state)
            {
                Logger.Log("Cookie val: " + googleCookie.Value);
                TempData["ErrorMessage"] = "Invalid state parameter. If this was an authentic attempt at registration, you may have waited too long to be redirected here. Please try again.";
                return RedirectToAction("signin", "home");
            }

            // Use the code exchange flow to get an access and refresh token.
            IAuthorizationCodeFlow flow =
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets,
                    Scopes = new string[] { PlusService.Scope.PlusLogin }
                });

            token = flow.ExchangeCodeForTokenAsync("", code, "http://wishlu.com/google/connect",
                    CancellationToken.None).Result;

            // Create an authorization state from the returned token.
            Session["gp_access_token"] = token.AccessToken;

            Logger.Log("Google+ Access Token has been obtained. Token: " + token.AccessToken);

            // Get tokeninfo for the access token if you want to verify.
            Oauth2Service service = new Oauth2Service(
                new Google.Apis.Services.BaseClientService.Initializer());
            Oauth2Service.TokeninfoRequest request = service.Tokeninfo();
            request.AccessToken = token.AccessToken;

            Tokeninfo info = request.Execute();

            string gplus_id = info.UserId;

            Logger.Log("Google+ user verified. Id: " + gplus_id + " Handle: " + info.VerifiedEmail);

            wlUser.GooglePlusId = gplus_id;
            wlUser.GooglePlusAccessToken = token.AccessToken;
            wlUser.GooglePlusRefreshToken = token.RefreshToken;
            wlUser.Update();

            return RedirectToAction("social", "user");
        }
        
        [Authorize]
        public ActionResult Disconnect()
        {
            Squid.Users.User wlUser = GetCurrentUser();

            wlUser.GooglePlusAccessToken = "";
            wlUser.GooglePlusId = "";
            wlUser.GooglePlusRefreshToken = "";
            wlUser.Set("GooglePlusAccessToken", "");
            wlUser.Set("GooglePlusId", "");
            wlUser.Set("GooglePlusRefreshToken", "");

            return RedirectToAction("social", "user");
        }

        [AllowAnonymous]
        public
        ActionResult
        SignIn(Guid state, string code)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            Logger.Log("Google:SignIn has been entered. state: " + state + " code: " + code);

            HttpCookie googleCookie = Request.Cookies["googleplus_state"];

            // Verify a real user made this registration request and validate CSRF state token
            if (googleCookie != null && Guid.Parse(googleCookie.Value) != state)
            {
                Logger.Log("Cookie val: " + googleCookie.Value);
                TempData["ErrorMessage"] = "The server received an invalid state parameter. If this was an authentic attempt at registration, you may have waited too long to be redirected here. Please try again.";
                return RedirectToAction("signin", "home");
            }

            // Use the code exchange flow to get an access and refresh token.
            IAuthorizationCodeFlow flow =
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets,
                    Scopes = new string[] { PlusService.Scope.PlusLogin }
                });

            token = flow.ExchangeCodeForTokenAsync("", code, "http://wishlu.com/google/signin",
                    CancellationToken.None).Result;

            // Create an authorization state from the returned token.
            Session["gp_access_token"] = token.AccessToken;

            Logger.Log("Google+ Access Token has been obtained. Token: " + token.AccessToken);

            // Get tokeninfo for the access token if you want to verify.
            Oauth2Service service = new Oauth2Service(
                new Google.Apis.Services.BaseClientService.Initializer());
            Oauth2Service.TokeninfoRequest request = service.Tokeninfo();
            request.AccessToken = token.AccessToken;

            Tokeninfo info = request.Execute();

            string gplus_id = info.UserId;

            Logger.Log("Google+ user verified. Id: " + gplus_id + " Handle: " + info.VerifiedEmail);

            Squid.Users.User wlUser = new Squid.Users.User();

            try
            {
                wlUser = Squid.Users.User.GoogleLoginStatic(gplus_id);
            }
            catch
            {
                TempData["ErrorMessage"] = "The Google account you have attempted to login with is not associated with any wishlu account. Please login with your username / e-mail and go to Social under Settings to connect with Google.";
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

                return RedirectToAction("index", "dash");
            }
            else
            {
                return RedirectToAction("index", "home");
            }
        }
                
        [AllowAnonymous]
        public
        ActionResult
        Register(Guid state, string code)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            Logger.Log("Google:Register has been entered. state: " + state + " code: " + code);

            HttpCookie googleCookie = Request.Cookies["googleplus_state"];
            
            // Verify a real user made this registration request and validate CSRF state token
            if (googleCookie != null && Guid.Parse(googleCookie.Value) != state)
            {
                Logger.Log("Cookie val: " + googleCookie.Value);
                TempData["ErrorMessage"] = "Invalid state parameter. If this was an authentic attempt at registration, you may have waited too long to be redirected here. Please try again.";
                return RedirectToAction("signin", "home");
            }

            // Use the code exchange flow to get an access and refresh token.
            IAuthorizationCodeFlow flow =
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets,
                    Scopes = new string[] { PlusService.Scope.PlusLogin }
                });

            token = flow.ExchangeCodeForTokenAsync("", code, "http://wishlu.com/google/register",
                    CancellationToken.None).Result;

            // Create an authorization state from the returned token.
            Session["gp_access_token"] = token.AccessToken;

            Logger.Log("Google+ Access Token has been obtained. Token: " + token.AccessToken);

            // Get tokeninfo for the access token if you want to verify.
            Oauth2Service service = new Oauth2Service(
                new Google.Apis.Services.BaseClientService.Initializer());
            Oauth2Service.TokeninfoRequest request = service.Tokeninfo();
            request.AccessToken = token.AccessToken;

            Tokeninfo info = request.Execute();

            string gplus_id = info.UserId;
                              
            Logger.Log("Google+ user verified. Id: " + gplus_id + " Handle: " + info.VerifiedEmail);
                        
            // If user exists, sign in as opposed to register (this handles people who accidentally click the google+ join button as opposed to signing in)
            if (Squid.Users.User.UserExistsGoogle(gplus_id))
            {
                Logger.Log("User already exists! Redirecting from Registration to SignIn");

                Squid.Users.User wlUser = new Squid.Users.User();

                try
                {
                    wlUser = Squid.Users.User.GoogleLoginStatic(gplus_id);
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

            viewModel.Id = gplus_id;
            viewModel.AccessToken = token.AccessToken;
            viewModel.RefreshToken = token.RefreshToken;
                                    
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

            String gid = formCollection["joinGoogleId"];
            String gatok = formCollection["joinGoogleAccessToken"];
            String grtok = formCollection["joinGoogleRefreshToken"];

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

                wlUser.GooglePlusId = gid;
                wlUser.GooglePlusAccessToken = gatok;
                wlUser.GooglePlusRefreshToken = grtok;
                                
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
                return RedirectToAction("Index", "Home");
            }
        }
    }
}