using System;
using System.Web.Mvc;

namespace Disco.Controllers
{
    [Authorize]
    public class GiftController : BaseController
    {
        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult Other()
        {
            return View();
        }

        public ActionResult My()
        {
            return View();
        }

        public ActionResult View(Guid id)
        {
            return View();
        }

        public ActionResult Create()
        {            
            return View();
        }

        public ActionResult Cancel()
        {            
            return View();
        }

        public ActionResult Confirm()
        {            
            return View();
        }         

        public ActionResult Give()
        {
            return View();
        }
    }   
}
