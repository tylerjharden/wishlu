using Milkshake;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;
using System.Xml;

namespace Disco.Controllers
{    
    public class SitemapController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {            
            return View("Index");
        }

        public ActionResult Members()
        {
            return View("Members");
        }

        public ActionResult Stores()
        {
            return View("Stores");
        }

        public ActionResult Categories()
        {
            return View("Categories");
        }
    }
}