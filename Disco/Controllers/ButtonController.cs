using System;
using System.Net;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class ButtonController : BaseController
    {        
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (!Request.IsAuthenticated || GetCurrentUserId() == null || GetCurrentUserId() == Guid.Empty)
                RedirectToAction("index", "join", new { @returnurl = Url.Encode(Request.Url.ToString()) });

            RedirectToAction("share");

            return View();
        }
        
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Share(string url, string image, string name, string description = "", string price = "0.00")
        {
            if (!Request.IsAuthenticated || GetCurrentUserId() == null || GetCurrentUserId() == Guid.Empty)
                RedirectToAction("index", "join", new { @returnurl = Url.Encode(Request.Url.ToString()) });

            ButtonModel model = new ButtonModel();
            model.Url = url;
            model.Image = WebUtility.UrlDecode(WebUtility.UrlEncode(image));
            model.Name = name.Trim();
            model.Description = description.Trim();

            if (model.Description == "undefined")
                model.Description = "";

            return View("Share", model);
        }        
    }

    public class ButtonModel
    {
        public string Url;
        public string Image;
        public string Name;
        public string Description;
    }    
}