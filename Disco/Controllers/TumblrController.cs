using Newtonsoft.Json.Linq;
using Squid.Log;
using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Disco.Controllers
{
    public class TumblrController : BaseController
    {
        [AllowAnonymous]
        public
        ActionResult
        SignIn(string code)
        {
            Logger.Log("Twitter:SignIn has been entered. User Access Code: " + code);

            Squid.Users.User wlUser = new Squid.Users.User();

            string access_token = "";

            if (HttpRuntime.Cache["fb_access_token"] == null)
            {
                string token = Squid.Users.FacebookManager.GetOauthToken(code, "http://wishlu.com/Facebook/SignIn");

                access_token = Squid.Users.FacebookManager.GetExtendedAccessToken(token);

                //HttpRuntime.Cache.Insert("fb_access_token", access_token, null, DateTime.Now.AddMinutes(Convert.ToDouble(args["expires"])), TimeSpan.Zero);

                Session["fb_access_token"] = access_token;
            }
            else
            {
                access_token = HttpRuntime.Cache["fb_access_token"].ToString();
            }

            Logger.Log("Facebook Access Token has been obtained. Token: " + access_token);
                                    
            try
            {
                wlUser = Squid.Users.User.FacebookLoginStatic(access_token);
            }
            catch (Exception e)
            {
                Logger.Error("An error occured inside of User:FacebookLogicStatic. Exception: " + e.ToString());

                ViewBag.ErrorMessage = "There was an error logging in. Please check your username and password and try again." + e.ToString();
                return RedirectToAction("Index","Home");
            }

            if (wlUser.IsActive)
            {
                //Session["WUSID"] = wlUser.SessionId.ToString();
                Session["UID"] = wlUser.Id;
                Session["FirstName"] = wlUser.FirstName;
                Session["LastName"] = wlUser.LastName;
                Session["Email"] = wlUser.Email;
                Session["DOB"] = wlUser.DateOfBirth;
                Session["ImageURL"] = wlUser.Image;
                Session["LANGUAGE"] = "en-us";
                Session["IsAdmin"] = wlUser.IsAdminUser;

                var authTicket = new FormsAuthenticationTicket(1, wlUser.Id.ToString(), DateTime.Now,
                                                       DateTime.Now.AddMinutes(30), true, "");

                string cookieContents = FormsAuthentication.Encrypt(authTicket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieContents)
                {
                    Expires = authTicket.Expiration,
                    Path = FormsAuthentication.FormsCookiePath
                };
                Response.Cookies.Add(cookie);

                Logger.Log("Login successful. User active. FormsAuthenticationTicket created. Redirecting user to URL: " + TempData["ReturnUrl"]);

                if (TempData["ReturnUrl"] != null)
                    return Redirect(TempData["ReturnUrl"].ToString());

                Logger.Log("ReturnUrl was null. Redirected to the Scroll:Index");

                return RedirectToAction("Index", "Dash");
            }
            Logger.Error("Login was successful but the user is inactive. Redirecting user to homepage.");
            ViewBag.WarningMessage = "Facebook login has been processed, but user is inactive.";
            return RedirectToAction("Index","Home");
        }

        [AllowAnonymous]
        public
        ActionResult
        Register(string code)
        {
            Logger.Log("FacebookController:Register has been entered. User code: " + code);

            string access_token = "";

            if (HttpRuntime.Cache["fb_access_token"] == null)
            {
                string token = Squid.Users.FacebookManager.GetOauthToken(code,"http://wishlu.com/Facebook/Register");

                access_token = Squid.Users.FacebookManager.GetExtendedAccessToken(token);

                //HttpRuntime.Cache.Insert("fb_access_token", access_token, null, DateTime.Now.AddMinutes(Convert.ToDouble(args["expires"])), TimeSpan.Zero);

                Session["fb_access_token"] = access_token;
            }
            else
            {
                access_token = HttpRuntime.Cache["fb_access_token"].ToString();
            }

            Logger.Log("Got OAuth token from FB. Token: " + access_token);
            
            try
            {
                Squid.Users.User wlUser = new Squid.Users.User();
                
                WebClient client = new WebClient();
                string JsonResult = client.DownloadString(string.Concat(
                       "https://graph.facebook.com/me?access_token=", access_token));

                JObject jsonUserInfo = JObject.Parse(JsonResult);

                string username = jsonUserInfo.Value<string>("username");
                string email = jsonUserInfo.Value<string>("email");
                string language = ""; // jsonUserInfo.Value<string>("locale").ToLower();
                string userid = jsonUserInfo.Value<string>("id");
                string firstname = jsonUserInfo.Value<string>("first_name");
                string lastname = jsonUserInfo.Value<string>("last_name");
                string birthday = jsonUserInfo.Value<string>("birthday");

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
                        return RedirectToAction("Index", "Home");
                    }

                    if (wlUser.IsActive)
                    {
                        //Session["WUSID"] = wlUser.SessionId.ToString();
                        Session["UID"] = wlUser.Id;
                        Session["FirstName"] = wlUser.FirstName;
                        Session["LastName"] = wlUser.LastName;
                        Session["Email"] = wlUser.Email;
                        Session["DOB"] = wlUser.DateOfBirth;
                        Session["ImageURL"] = wlUser.Image;
                        Session["LANGUAGE"] = "en-us";
                        Session["IsAdmin"] = wlUser.IsAdminUser;

                        var authTicket = new FormsAuthenticationTicket(1, wlUser.Id.ToString(), DateTime.Now,
                                                               DateTime.Now.AddMinutes(30), true, "");

                        string cookieContents = FormsAuthentication.Encrypt(authTicket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieContents)
                        {
                            Expires = authTicket.Expiration,
                            Path = FormsAuthentication.FormsCookiePath
                        };
                        Response.Cookies.Add(cookie);

                        return RedirectToAction("Index", "Dash");
                    }
                }
                                
                wlUser.Email = email;
                wlUser.LoginId = email;
                wlUser.FirstName = firstname;
                wlUser.LastName = lastname;
                wlUser.DateOfBirth = Convert.ToDateTime(birthday);
                wlUser.IsActive = true;
                wlUser.IsAdminUser = false;

                wlUser.FacebookPageId = userid;
                                
                string pwd = "password";

                if (language == null || language == String.Empty)
                {
                    wlUser.LanguageId = "en-us";
                }
                else
                {
                    wlUser.LanguageId = language;
                }
                
                wlUser.RegisterUser(pwd, pwd);
                ViewBag.SuccessMessage = "Account successfully registered!";
                                
                try
                {
                    wlUser = Squid.Users.User.Login(email, pwd);                    
                }
                catch (Exception e)
                {
                    Logger.Log("There was an error logging in after registration. Exception: " + e.ToString());
                    ViewBag.WarningMessage = "There was an error logging in. " + e.ToString();
                    //return Redirect("/");
                }

                if (wlUser.IsActive)
                {
                    //Session["WUSID"] = wlUser.SessionId.ToString();
                    Session["UID"] = wlUser.Id;
                    Session["FirstName"] = wlUser.FirstName;
                    Session["LastName"] = wlUser.LastName;
                    Session["Email"] = wlUser.Email;
                    Session["DOB"] = wlUser.DateOfBirth;
                    Session["ImageURL"] = wlUser.Image;
                    Session["LANGUAGE"] = "en-us";
                    Session["IsAdmin"] = wlUser.IsAdminUser;

                    var authTicket = new FormsAuthenticationTicket(1, wlUser.Id.ToString(), DateTime.Now,
                                                       DateTime.Now.AddMinutes(30), true, "");

                    string cookieContents = FormsAuthentication.Encrypt(authTicket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieContents)
                    {
                        Expires = authTicket.Expiration,
                        Path = FormsAuthentication.FormsCookiePath
                    };
                    Response.Cookies.Add(cookie);

                    return RedirectToAction("Setup", "Join");
                }
                ViewBag.WarningMessage = "Facebook login has been processed, but user is inactive.";
                return RedirectToAction("Index", "Home");
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