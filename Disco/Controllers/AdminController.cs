using Milkshake;
using System;
using System.Web.Mvc;

namespace Disco.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {        
        public ActionResult Index()
        {
            ValidateAdmin();

            return View();
        }

        public ActionResult email()
        {
            ValidateAdmin();

            GetCurrentUser().SendEmailVerification();

            return RedirectToAction("index", "dash");
        }

        public ActionResult nue(int day)
        {
            ValidateAdmin();

            //GetCurrentUser().SendEmailVerification();
            new Squid.Mail.MailController().NewUserEmail(GetCurrentUser(), day).Deliver();

            return RedirectToAction("index", "dash");
        }

        public ActionResult StoreManager()
        {
            ValidateAdmin();

            return View("StoreManager");
        }

        public ActionResult StoreWizard()
        {
            ValidateAdmin();

            return View("StoreWizard");
        }

        public ActionResult AddStore(FormCollection formCollection)
        {
            string name = formCollection["shopname"];
            string website = formCollection["shopsite"];
            string image = formCollection["shopimage"];

            string facebook = formCollection["shopfacebook"];
            string twitter = formCollection["shoptwitter"];
            string google = formCollection["shopgoogle"];
            string wanelo = formCollection["shopwanelo"];
            string pinterest = formCollection["shoppinterest"];
            string instagram = formCollection["shopinstagram"];
            string youtube = formCollection["shopyoutube"];
            string tumblr = formCollection["shoptumblr"];

            bool verified = formCollection["isverified"] == "1";
            bool brick = formCollection["isbrick"] == "1";
            bool online = formCollection["isonline"] == "1";
            bool chain = formCollection["ischain"] == "1";
            bool boutique = formCollection["isboutique"] == "1";
            bool featured = formCollection["isfeatured"] == "1";
            StoreLevel level;

            switch (formCollection["shoplevel"])
            {
                case "Free":
                    level = StoreLevel.Free;
                    break;

                case "Bronze":
                    level = StoreLevel.Bronze;
                    break;

                case "Silver":
                    level = StoreLevel.Silver;
                    break;

                case "Gold":
                    level = StoreLevel.Gold;
                    break;

                case "Platinum":
                    level = StoreLevel.Platinum;
                    break;

                case "Diamond":
                    level = StoreLevel.Diamond;
                    break;

                default:
                    level = StoreLevel.None;
                    break;
            }

            Store s = new Store();
            s.Id = Guid.NewGuid();

            s.Name = name;
            s.Website = website;
            s.Logo = image;

            s.FacebookId = facebook;
            s.TwitterId = twitter;
            s.GooglePlusId = google;
            s.WaneloId = wanelo;
            s.PinterestId = pinterest;
            s.InstagramId = instagram;
            s.YoutubeId = youtube;
            s.TumblrId = tumblr;

            s.IsVerified = verified;
            s.IsBrickAndMortar = brick;
            s.IsOnline = online;
            s.IsChain = chain;
            s.IsFeatured = featured;
            s.IsBoutique = boutique;

            s.Level = level;

            Milkshake.Store.Add(s);

            return View("Index");
        }

        public ActionResult Store(Guid id)
        {
            ValidateAdmin();

            Store model = Milkshake.Store.GetById(id);

            return View("Store", model);
        }

        public ActionResult AddAlt(Guid id, string name)
        {
            ValidateAdmin();

            try
            {
                Store model = Milkshake.Store.GetById(id);

                bool result = model.AddAlternative(name);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RemoveAlt(Guid id, string name)
        {
            ValidateAdmin();

            try
            {
                Store model = Milkshake.Store.GetById(id);

                bool result = model.RemoveAlternative(name);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}