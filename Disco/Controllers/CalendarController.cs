using Squid.Wishes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Controllers
{    
    public class CalendarController : BaseController
    {
        [Authorize]
        public ActionResult Index()
        {
            List<MappedWishlu> model = Squid.Users.User.GetMappedUpcomingWishlus(CurrentUser.Id).OrderBy(x => x.EventDateTime.GetValueOrDefault().Date).ToList();
                        
            return View("Index", model);
        }        
    }    
}
