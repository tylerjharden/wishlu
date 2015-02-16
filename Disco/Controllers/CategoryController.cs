using System;
using System.Web.Mvc;

namespace Disco.Controllers
{
    public class CategoryController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult View(Guid id)
        {
            Milkshake.Category c = Milkshake.Category.GetCategory(id);

            if (c == null)
            {
                TempData["ErrorMessage"] = "That category is invalid or does not exist.";
                return RedirectToAction("index");
            }

            return View("View", c);
        }

        [AllowAnonymous]
        public ActionResult Subcategories(Guid id)
        {
            return View("Subcategories", id);
        }

        public ActionResult Products(Guid id)
        {
            return View("Products", id);
        }
    }
}