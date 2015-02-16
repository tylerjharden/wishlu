using Facebook;
using System.Collections.Generic;
using System.Dynamic;
using System.Web.Mvc;

namespace Disco.Controllers
{            
    public class SandboxController : BaseController
    {
        [Authorize]
        public ActionResult Index()
        {            
            var accessToken = Session["fb_access_token"].ToString();
            var client = new FacebookClient(accessToken);

            List<dynamic> here = new List<dynamic>();
            List<dynamic> missing = new List<dynamic>();

            dynamic result = client.Get("me", new { fields = "friends" });

            foreach (dynamic friend in result.friends.data)
            {
                string id = friend.id;

                //if (Squid.Users.User.UserExists(id))
                //    here.Add(friend);
                //else
                    missing.Add(friend);
            }

            dynamic res = new ExpandoObject();
            res.here = here;
            res.missing = missing;

            return View("Index", res);
        }
    }
}