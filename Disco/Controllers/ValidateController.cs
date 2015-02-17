using System;
using System.Web.Mvc;

namespace Disco.Controllers
{
    [AllowAnonymous]
    public class ValidateController : BaseController
    {
        [AllowAnonymous]
        [OutputCache(Duration = 0, NoStore = true)]
        public JsonResult Available()
        {
            string email = Request.QueryString["joinEmail"];

            return Json(!Squid.Users.User.LoginIdExists(email), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult Exists()
        {
            string email = Request.QueryString["EMail"];

            return Json(Squid.Users.User.LoginIdExists(email), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Reset(FormCollection formCol)
        {
            return View("Reset");
        }
    }
}