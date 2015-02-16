using Milkshake;
using System;
using System.Web.Mvc;


namespace Disco.Controllers
{
    //================================================================================================//
    public
    class StoreController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View("Index");
        }
                
        [AllowAnonymous]
        public ActionResult View(Guid id)
        {            
            Milkshake.Store s = Milkshake.Store.GetById(id);
            
            return View("View", s);
        }

        [Authorize]
        public ActionResult Follow(Guid id)
        {
            GetCurrentUser().FollowStore(id);

            if (Request.IsAjaxRequest())
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Milkshake.Store s = Milkshake.Store.GetById(id);

                return View("View", s);
            }
        }

        [Authorize]
        public ActionResult Unfollow(Guid id)
        {
            GetCurrentUser().UnfollowStore(id);

            if (Request.IsAjaxRequest())
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Milkshake.Store s = Milkshake.Store.GetById(id);

                return View("View", s);
            }
        }

        //---------------------------------------------------------------------------------------------//
        [AllowAnonymous]
        public
         ActionResult
         Products(Guid id)
        {
            Milkshake.Store s = Milkshake.Store.GetById(id);

            return View("Products", s);
        }
    }
    //================================================================================================//
}
