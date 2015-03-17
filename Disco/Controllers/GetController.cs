using Milkshake;
using Squid.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Controllers
{    
    [Serializable]
    public class NotificationsModel
    {
        public List<Squid.Messages.MappedNotification> Notifications { get; set; }
        public int Count { get; set; }
    }
    
    public class GetController : BaseController
    {   
        [Authorize]
        [HttpGet]
        public ActionResult Gifts()
        {
            return PartialView("Gifts");
        }


        [Authorize]
        [HttpGet]
        public ActionResult Notifications()
        {
            NotificationsModel model = new NotificationsModel();
            model.Notifications = Squid.Users.User.GetMappedNotifications(GetCurrentUserId());
            model.Count = model.Notifications.Count(x => x.Read == false);

            return PartialView("Notifications", model);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Calendar()
        {            
            List<Squid.Wishes.MappedWishlu> events = Squid.Users.User.GetMappedUpcomingWishlus(GetCurrentUserId()).OrderBy(x => x.EventDateTime.Value.Date).ToList();
            events.RemoveAll(x => !x.EventDateTime.HasValue || x.EventDateTime.Value.Date > DateTimeOffset.Now.AddDays(30).Date);

            return PartialView("Calendar", events);
        }

        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult Products()
        {
            ViewBag.ProductName = Request.QueryString["query"];

            if (Request.QueryString["page"] == null)
                ViewBag.Page = 0;
            else
                ViewBag.Page = Int32.Parse(Request.QueryString["page"]);

            if (Request.QueryString["limit"] != null)
                ViewBag.Limit = Int32.Parse(Request.QueryString["limit"]);

            //Squid.Products.Graph.GraphProvider graphSearch = new Squid.Products.Graph.GraphProvider();
            //List<Milkshake.Product> results = graphSearch.Search((string)ViewBag.ProductName, (int)ViewBag.Page);

            var results = Milkshake.Search.ProductName((string)ViewBag.ProductName, (int)ViewBag.Page, 50);

            return PartialView("Products", results);
        }
        
        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult Shops()
        {
            if (Request.QueryString["query"] == null)            
                return PartialView("Shops", Store.GetStores().Where(x => Milkshake.Search.StoreIdCount(x.Id) > 0).ToList());            
            else
                return PartialView("Shops", Store.Find(Request.QueryString["query"]));
        }

        [AllowAnonymous]
        public ActionResult ShopProducts(Guid id, int page = 0)
        {
            return PartialView("Products", Milkshake.Search.StoreId(id, page, 50));
            //return PartialView("ShopProducts", Store.GetById(id).GetProducts(page));
        }

        [AllowAnonymous]
        public ActionResult MoreLike(Guid id, int page = 0, int limit = 50)
        {
            return PartialView("Products", Milkshake.Search.MoreLike(id,page,limit));
        }

        [AllowAnonymous]
        public ActionResult MoreFrom(Guid id, int page = 0, int limit = 5)
        {
            Milkshake.Offer o = Milkshake.Search.ProductId(id).Offers[0];

            return PartialView("ProductImages", Milkshake.Search.MoreFrom(id,o.StoreId,page,limit));
        }

        [AllowAnonymous]
        public ActionResult RootCategories()
        {
            return PartialView("Categories", Category.GetRootCategories());
        }

        [AllowAnonymous]
        public ActionResult AllCategories()
        {
            return PartialView("Categories", Category.GetAllCategories());
        }

        [AllowAnonymous]
        public ActionResult Subcategories(Guid id)
        {
            return PartialView("Categories", Category.GetSubCategories(id));
        }

        [AllowAnonymous]
        public ActionResult CategoryProducts(Guid id, int page = 0)
        {
            return PartialView("Products", Search.CategoryProducts(id, page));
            //return PartialView("CategoryProducts", Category.GetProducts(id, page));
        }
        
        [Authorize]
        [ValidateInput(false)]
        public ActionResult People(string query)
        {
            return PartialView("People", GetCurrentUser().Find(query));
        }

        [AllowAnonymous]
        public ActionResult Feed(int page = 0)
        {
            //List<Squid.Wishes.Wish> model = GetCurrentUser().GetFollowingWishes().OrderBy(x => x.CreatedOn).Reverse().ToList();
            //model.RemoveAll(x => x.GetAssignmentId() == Guid.Empty);

            if (Request.IsAuthenticated)
            {
                List<Squid.Users.FeedItem> model = Squid.Users.User.GetFollowingWishes(GetCurrentUserId(), page);

                // New user or empty feed
                if (model.Count == 0)
                {
                    List<Milkshake.Product> filler = Squid.Users.User.GetFillerFeed();

                    return PartialView("FillerFeed", filler);
                }

                return PartialView("Feed", model);
            }
            else
            {
                List<Milkshake.Product> filler = Squid.Users.User.GetFillerFeed();

                return PartialView("FillerFeed", filler);
            }
        }

        [Authorize]
        public ActionResult FeedPage(int page)
        {
            List<Squid.Users.FeedItem> model = Squid.Users.User.GetFollowingWishes(GetCurrentUserId(),page);

            // New user or empty feed
            if (model.Count == 0)
            {
                List<Milkshake.Product> filler = Squid.Users.User.GetFillerFeed();

                return PartialView("FillerFeed", filler);
            }

            return PartialView("Feed", model);
        }

        [Authorize]
        public ActionResult Wishlus()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IEnumerable<Squid.Wishes.WishluWishes> model = Squid.Wishes.Wishlu.GetUsersWishLusWishes(GetCurrentUserId());
            model = model.OrderBy(x => x.Wishlu.CreatedOn);

            sw.Stop();
            Logger.Log("GetUsersWishLusWishes() took " + sw.ElapsedMilliseconds + "ms");

            return PartialView("Wishlus", model);
        }

        [AllowAnonymous]
        public ActionResult OtherWishlus(Guid id)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IEnumerable<Squid.Wishes.WishluWishes> model;            
            if (id == GetCurrentUserId())
            {
                model = Squid.Wishes.Wishlu.GetUsersWishLusWishes(GetCurrentUserId());
            }
            else
            {
                if (Request.IsAuthenticated && GetCurrentUser().IsFriend(id))
                {
                    model = Squid.Wishes.Wishlu.GetFriendsWishLusWishes(GetCurrentUserId(), id);
                }
                else
                {
                    model = Squid.Wishes.Wishlu.GetUsersPublicWishlusWishes(id);
                }
            }

            model = model.OrderBy(x => x.Wishlu.CreatedOn);
            //model.RemoveAll(x => x.Wishes.Count <= 0);
            
            sw.Stop();
            Logger.Log("GetFriendsWishlusWishes() took " + sw.ElapsedMilliseconds + "ms");

            return PartialView("OtherWishlus", model);
        }
        
        [AllowAnonymous]
        public ActionResult PublicWishlus(Guid id)
        {
            IEnumerable<Squid.Wishes.WishluWishes> model;

            model = Squid.Wishes.Wishlu.GetUsersPublicWishlusWishes(id);

            return PartialView("OtherWishlus", model);
        }

        [Authorize]
        public ActionResult Wishloops()
        {
            List<Squid.Wishes.Wishloop> model = Squid.Wishes.Wishloop.GetUsersWishloops(GetCurrentUserId());

            return PartialView("Wishloops", model);
        }

        [Authorize]
        public ActionResult Wishes()
        {
            List<Squid.Wishes.Wish> model = Squid.Users.User.GetUsersWishes(GetCurrentUserId());

            return PartialView("Wishes", model);
        }

        [Authorize]
        new public ActionResult Profile()
        {
            var model = GetCurrentUser();

            return PartialView("Profile", model);
        }
    }    
}