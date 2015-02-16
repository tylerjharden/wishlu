using Squid.Products.TwoTap;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;


namespace Disco.Controllers
{
    //================================================================================================//
    public
    class ProductController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return RedirectToActionPermanent("index", "stores");
        }

        [AllowAnonymous]
        public ActionResult Search()
        {
            ViewBag.Query = Request.QueryString["query"];

            return View("Search");
        }

        [AllowAnonymous]
        public ActionResult View(Guid id)
        {
            Milkshake.Product p = Milkshake.Search.ProductId(id);//Milkshake.ProductManager.GetProduct(id);
            Milkshake.Search.ProductView(id);

            List<string> products = new List<string>();

            string affiliateUrl = p.Offers[0].Url;
            string product_url = affiliateUrl;

            try
            {
                NameValueCollection qs = HttpUtility.ParseQueryString((affiliateUrl.IndexOf('?') < affiliateUrl.Length - 1) ? affiliateUrl.Substring(affiliateUrl.IndexOf('?') + 1) : string.Empty);

                product_url = qs.Get("url");  // actual product URL is a url= param in the affiliate link              

                if (!String.IsNullOrEmpty(qs.Get("murl")))
                {
                    product_url = qs.Get("murl");
                }
            }
            catch { }

            products.Add(product_url);

            //var response = Client.GetProductEstimates(products);

            dynamic model = new ExpandoObject();
            //model.Estimates = response;
            model.Product = p;

            return View("View", model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Suggest(SuggestWishModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please select a product to suggest to your friends." });

                if (model.Friends == null || model.Friends.Count == 0)
                    return Json(new { result = false, message = "Please select at least one friend to suggest to." });

                var current = GetCurrentUser();
                foreach (Guid friend in model.Friends)
                {
                    current.SuggestProduct(model.Id, friend);
                }

                return Json(new { result = true, message = "You have suggested this product to " + model.Friends.Count + " friend(s)." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [AllowAnonymous]
        public ActionResult BBY(string id)
        {
            Squid.Products.BestBuy.BestBuyProduct p = Squid.Products.BestBuy.BestBuyProvider.Lookup(id);

            return View("BBY", p);
        }

        [AllowAnonymous]
        public ActionResult Amazon(string id)
        {
            Squid.Products.Amazon.AmazonProduct p = Squid.Products.Amazon.AmazonProvider.Lookup(id);

            return View("Amazon", p);
        }

        //---------------------------------------------------------------------------------------------//              
    }
    //================================================================================================//
}
