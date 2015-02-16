﻿using System.Web.Mvc;
using Schloss.Extensions;

namespace Disco.Controllers
{    
    public class DashController : BaseController
    {
        [Authorize]
        public ActionResult Index()
        {
            if (GetCurrentUser().DateOfBirth.Month.IsEven())
            {
                return View("Index");
            }
            else
            {
                return View("Index2");
            }            
        }
    }    
}