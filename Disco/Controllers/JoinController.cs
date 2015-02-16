using Squid.Log;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class JoinController : BaseController
    {        
        [AllowAnonymous]
        public ActionResult Index(string returnurl)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            ViewBag.ReturnUrl = returnurl;

            return View("Index");
        }
        
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Create(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.Email))
                    return Json(new { result = false, message = "Please provide an e-mail address for your new account." });

                if (String.IsNullOrEmpty(model.FirstName))
                    return Json(new { result = false, message = "Please provide your first name." });

                if (String.IsNullOrEmpty(model.FirstName))
                    return Json(new { result = false, message = "Please provide your last name." });

                //if (String.IsNullOrEmpty(model.Key))
               //     return Json(new { result = false, message = "You must provide an invitation code to join. This was either given to you directly or contained in your invitation link." });

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
        
        [AllowAnonymous]
        public ActionResult Register(string returnurl)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            ViewBag.ReturnUrl = returnurl;

            HttpCookie googleCookie = new HttpCookie("googleplus_state", Guid.NewGuid().ToString());
            googleCookie.Expires = DateTime.Now.AddMinutes(15);            
            Response.Cookies.Add(googleCookie);

            ViewData["googleplus_state"] = googleCookie.Value;

            return View("Register");
        }

        [AllowAnonymous]
        public JsonResult Exists(string code)
        {
            if (string.IsNullOrEmpty(code))
                return JsonResponse(false, "Please provide an invitation code to verify.");

            if (Squid.Users.User.InviteCodeExists(code))
            {
                return JsonResponse(true, "Invitation code is valid.");
            }
            else 
            {
                return JsonResponse(false, "The specified invitation code is invalid or does not exist.");
            }            
        }

        [AllowAnonymous]
        public ActionResult Invite(string code)
        {
            if (Request.IsAuthenticated)
            {
                // If logged in, simply create friendship
                Squid.Users.User.GetInvitingUser(code).CreateFriendship(GetCurrentUserId());

                return RedirectToAction("index", "dash");
            }

            if (String.IsNullOrEmpty(code) || !Squid.Users.User.InviteCodeExists(code))
            {
                TempData["ErrorMessage"] = "The specified invitation code / link is invalid or may have already been used.";
                return RedirectToAction("register", "join");
            }
            
            /*HttpCookie googleCookie = new HttpCookie("googleplus_state", Guid.NewGuid().ToString());
            googleCookie.Expires = DateTime.Now.AddMinutes(15);
            Response.Cookies.Add(googleCookie);

            ViewData["googleplus_state"] = googleCookie.Value;*/

            dynamic model = new ExpandoObject();
            model.Inviter = Squid.Users.User.GetInvitingUser(code);
            model.Code = code;

            return View("Invite", model);
        }
    }

    [Serializable]
    public class CreateUserModel
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
    }    
}

