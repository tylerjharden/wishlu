using Squid.Log;
using System;
using System.Web;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class AboutController : BaseController
    {        
        [AllowAnonymous]
        public ActionResult Index()
        {            
            return View();
        }

        [AllowAnonymous]
        public ActionResult Faqs()
        {
            return View("Faqs");
        }
    }
}