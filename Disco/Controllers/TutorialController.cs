using Facebook;
using Squid.Log;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Controllers
{
    [Authorize]
    public class TutorialController : BaseController
    {        
        [Authorize]
        public ActionResult Index()
        {                                    
            return RedirectToAction("index", "home");
        }
        
        [Authorize]
        public ActionResult See()
        {
            var user = GetCurrentUser();

            // if a user has completed the tutorial, do not let them back here
            //if (!user.TutorialMode)
            //    return RedirectToAction("index", "dash");

            Session["TutorialStep"] = 1;
            user.Set("TutorialStep", 1);

            return View("see");
        }

        [Authorize]
        public ActionResult Wishlu()
        {
            var user = GetCurrentUser();

            // if a user has completed the tutorial, do not let them back here
            //if (!user.TutorialMode)
            //    return RedirectToAction("index", "dash");

            Session["TutorialStep"] = 2;
            user.Set("TutorialStep", 2);

            return View("wishlu");
        }

        [Authorize]
        public ActionResult Wish()
        {
            var user = GetCurrentUser();

            // if a user has completed the tutorial, do not let them back here
            //if (!user.TutorialMode)
            //    return RedirectToAction("index", "dash");

            Session["TutorialStep"] = 3;
            user.Set("TutorialStep", 3);

            return View("wish");
        }

        [Authorize]
        public ActionResult Invite()
        {
            var user = GetCurrentUser();

            // if a user has completed the tutorial, do not let them back here
            //if (!user.TutorialMode)
            //    return RedirectToAction("index", "dash");

            Session["TutorialStep"] = 4;
            user.Set("TutorialStep", 4);

            List<Squid.Users.User> here = new List<Squid.Users.User>();
            List<dynamic> missing = new List<dynamic>();
            
            if (user.IsFacebookSynced)
            {
                var accessToken = user.FacebookAccessToken;
                var client = new FacebookClient(accessToken);

                dynamic result = client.Get("me", new { fields = "friends" });

                List<String> ids = new List<string>();
                foreach (dynamic friend in result.friends.data)
                {
                    ids.Add(friend.id);
                }

                here.AddRange(Squid.Users.User.GetFacebookUsers(ids));

                here.RemoveAll(x => x.IsFriend(user.Id));
                
                foreach (dynamic friend in result.friends.data)
                {
                    string id = friend.id;

                    if (here.Exists(x => x.FacebookAccessToken == id))
                        continue;

                    missing.Add(friend);
                }

                dynamic res = new ExpandoObject();
                res.here = here;
                res.missing = missing;
                res.user = GetCurrentUser();

                return View("invite", res);
            }

            return View("invite");
        }

        [Authorize]
        public ActionResult Stores()
        {
            var user = GetCurrentUser();

            Session["TutorialStep"] = 5;
            user.Set("TutorialStep", 5);

            if (user.IsFacebookSynced)
            {
                Squid.Users.FacebookManager.Client.AccessToken = "296645670486904|KK_Jly0DUrHVxOXYaqOfQw_AT4A";
                dynamic likes = Squid.Users.FacebookManager.Client.Get("/" + user.FacebookPageId + "/likes");

                List<string> fids = new List<string>();

                foreach (var page in likes.data)
                {
                    fids.Add(page.id);
                }

                return View("stores", fids);
            }

            return View("stores");
        }

        [Authorize]
        [HttpPost]
        public ActionResult Stores(ChooseStoresModel model)
        {
            var user = GetCurrentUser();

            if (ModelState.IsValid)
            {
                if (model.Stores == null || model.Stores.Count < 1)
                    return Json(new { result = false, message = "Please select atleast one store as a favorite." });

                try
                {
                    foreach (Guid store in model.Stores)
                        if (!user.FavoriteStores.Contains(store))
                            user.FavoriteStores.Add(store);

                    user.Set("FavoriteStores", user.FavoriteStores);

                    return Json(new { result = true, message = "Your favorite stores have been saved successfully. You are now being redirected to your dashboard." });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return Json(new { result = false, message = "An unexpected error occurred while attempting to save your favorite stores." });
                }                
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        public new ActionResult Profile()
        {
            var user = GetCurrentUser();

            Session["TutorialStep"] = 6;
            user.Set("TutorialStep", 6);
                        
            return View("profile", user);
        }

        [Authorize]
        [HttpPost]
        public new ActionResult Profile(ProfileModel model)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.FavoriteThings))
                    model.FavoriteThings = "";

                if (String.IsNullOrEmpty(model.AddressLine1))
                    return Json(new { result = false, message = "Please specify the first line of your address." });

                if (String.IsNullOrEmpty(model.City))
                    return Json(new { result = false, message = "Please specify your city." });

                if (String.IsNullOrEmpty(model.State))
                    return Json(new { result = false, message = "Please select your state." });

                if (String.IsNullOrEmpty(model.ZipCode))
                    return Json(new { result = false, message = "Please provide your 5-digit postal / zip code." });

                if (model.ZipCode.Length < 5)
                    return Json(new { result = false, message = "The postal / zip code you provided is too short. It must be 5 characters." });

                if (model.ZipCode.Length > 5)
                    return Json(new { result = false, message = "The postal / zip code you provided is too long. It must be 5 characters." });

                var user = CurrentUser;

                user.Favorites = model.FavoriteThings;                
                user.ShipAddress1 = model.AddressLine1;
                user.ShipAddress2 = model.AddressLine2;
                user.ShipCity = model.City;
                user.ShipStateOrProvince = model.State;
                user.ShipZipOrPostalCode = model.ZipCode;

                user.Update();

                return Json(new { result = true, message = "Your profile information has been updated successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        public ActionResult Bookmarklet()
        {
            var user = GetCurrentUser();

            // if a user has completed the tutorial, do not let them back here
            //if (!user.TutorialMode)
            //    return RedirectToAction("index", "dash");

            Session["TutorialStep"] = 7;
            user.Set("TutorialStep", 7);

            return View("bookmarklet");
        }
                       
        [Authorize]
        public ActionResult Skip()
        {
            var user = GetCurrentUser();

            Session["TutorialMode"] = false;
            Session["TutorialStep"] = 999;
            user.Set("TutorialMode", false);
            user.Set("TutorialStep", 999);

            return RedirectToAction("index", "dash");
        }
    }

    [Serializable]
    public class ChooseStoresModel
    {
        public List<Guid> Stores { get; set; }
    }

    [Serializable]
    public class ProfileModel
    {
        public string FavoriteThings { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}