using Squid.Log;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class HomeController : BaseController
    {
        public string ReturnMessage = "";
                
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {            
            base.OnActionExecuting(filterContext);
        }

        [AllowAnonymous]
        public ActionResult UnderConstruction()
        {
            return View("UnderConstruction");
        }

        //[OutputCache(Duration = 86400, Location = System.Web.UI.OutputCacheLocation.Client)]
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            return View();
        }
        
        [Authorize]
        public ActionResult Bookmarklet()
        {
            return View("Bookmarklet");
        }

        //[OutputCache(Duration = 86400, Location = System.Web.UI.OutputCacheLocation.Client)] 
        [AllowAnonymous]
        public ActionResult Join()
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            return RedirectToActionPermanent("index", "join");
        }

        //[OutputCache(Duration = 86400, Location = System.Web.UI.OutputCacheLocation.Client)]
        [AllowAnonymous]
        public ActionResult ContactUs()
        {
            return View("ContactUs");
        }

        //[OutputCache(Duration = 86400, Location = System.Web.UI.OutputCacheLocation.Client)]
        [AllowAnonymous]
        public ActionResult About()
        {
            return View("About");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult SignIn(SignInModel model)
        {
            if (model == null)
                return Json(new { result = false, message = "The server has received an invalid model." });

            if (model.LoginId == null || String.IsNullOrEmpty(model.LoginId.Trim()))
                return Json(new { result = false, message = "Please enter a valid e-mail address or username." });

            if (model.Password == null || String.IsNullOrEmpty(model.Password.Trim()) || model.Password.Length < 6)
                return Json(new { result = false, message = "Please enter a valid password." });
                        
            try
            {
                Squid.Users.User wlUser = new Squid.Users.User();

                try
                {
                    wlUser = Squid.Users.User.LoginHandle(model.LoginId, model.Password);

                    if (wlUser == null) // may have been an e-mail address!
                    {
                        wlUser = Squid.Users.User.Login(model.LoginId, model.Password);

                        if (wlUser == null)
                        {
                            return Json(new { result = false, message = "The specified username / e-mail address does not exist." });
                        }
                    }
                }
                catch (Squid.PasswordErrorException e)
                {
                    Logger.Log("Failed login attempt: " + e.ToString());
                    return Json(new { result = false, message = "The specified password was invalid." });
                }
                catch (Squid.UserInactiveException e)
                {
                    Logger.Log("Failed login attempt: " + e.ToString());
                    return Json(new { result = false, message = "The specified user account is inactive. Please contact support." });
                }
                catch (Squid.LoginIdNotFoundException e)
                {
                    Logger.Log("Failed login attempt: " + e.ToString());
                    return Json(new { result = false, message = "The specified login ID does not exist." });
                }

                PopulateSession(wlUser);
                CreateAuthTicket(wlUser);

                string dest = Url.Action("index", "dash");

                if (wlUser.TutorialMode == true)
                {
                    if (wlUser.TutorialStep == 0)
                        dest = Url.Action("see", "tutorial");

                    if (wlUser.TutorialStep == 1)
                        dest = Url.Action("wishlu", "tutorial");

                    if (wlUser.TutorialStep == 2)
                        dest = Url.Action("wish", "tutorial");

                    if (wlUser.TutorialStep == 3)
                        dest = Url.Action("invite", "tutorial");

                    if (wlUser.TutorialStep == 4)
                        dest = Url.Action("stores", "tutorial");

                    if (wlUser.TutorialStep == 5)
                        dest = Url.Action("profile", "tutorial");

                    if (wlUser.TutorialStep == 6)
                        dest = Url.Action("bookmarklet", "tutorial");
                }

                if (model.ReturnUrl != null && !String.IsNullOrEmpty(model.ReturnUrl.Trim()))
                    dest = model.ReturnUrl;

                return Json(new { result = true, destination = dest });

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "An unhandled error has occurred while attempting to login. We have been notified and are working to fix it!" });
            }                        
        }
        
        [Authorize]
        public ActionResult SignOut()
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies["WishLuAuth"] != null)
            {
                var c = new HttpCookie("WishLuAuth");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);

                Logger.Log("Authorization cookie has been removed.");
            }

            ViewBag.SuccessMessage = "You have been logged out successfully.";

            return RedirectToAction("signin");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult SignIn(string returnurl = "")
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            ViewBag.ReturnUrl = returnurl;

            HttpCookie googleCookie = new HttpCookie("googleplus_state", Guid.NewGuid().ToString());
            googleCookie.Expires = DateTime.Now.AddMinutes(15);
            Response.Cookies.Add(googleCookie);

            ViewData["googleplus_state"] = googleCookie.Value;

            return View("SignIn");
        }

        [AllowAnonymous]
        public ActionResult Register(string returnurl)
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");

            return RedirectToAction("register", "join", new { @returnurl = returnurl });
        }
        
        [AllowAnonymous]
        public ActionResult Privacy()
        {
            return View("Privacy");
        }

        [AllowAnonymous]
        public ActionResult TOS()
        {
            return View("TOS");
        }
    }

    public class SignInModel
    {
        public string LoginId { get; set; }
        public string Password { get; set; }        
        public string ReturnUrl { get; set; }
    }
}