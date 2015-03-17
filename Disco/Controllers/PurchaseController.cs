using Squid.Log;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Squid.Products.TwoTap;
using Milkshake;
using System.Collections.Specialized;
using System.Dynamic;
using System.Net;

namespace Disco.Controllers
{
    public class AddToCartModel
    {
        public Guid ProductId { get; set; }
    }

    public class ValidateShippingModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }
        public string Telephone { get; set; }
    }

    public class ValidateBillingModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }
        public string Telephone { get; set; }

        public string CardName { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string CVV { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
    }

    public class CheckoutModel
    {
        public string ShippingFirstName { get; set; }
        public string ShippingLastName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZIP { get; set; }
        public string ShippingTelephone { get; set; }

        public string BillingFirstName { get; set; }
        public string BillingLastName { get; set; }
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZIP { get; set; }
        public string BillingTelephone { get; set; }

        public string CardName { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string CVV { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }

        public string Email { get; set; }

        public string CartId { get; set; }
        public string SiteId { get; set; }
        public string ProductId { get; set; }        
        public string AffiliateLink { get; set; }

        public string ProductUrl { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
    }

    public class ConfirmModel
    {
        public class SiteData
        {
            public class ProductData
            {
                public string title { get; set; }
                public string price { get; set; }
                public string image { get; set; }
                public string url { get; set; }
            }

            public class PriceData
            {
                public string salex_tax { get; set; }
                public string shipping_price { get; set; }
                public string coupon_value { get; set; }
                public string gift_card_value { get; set; }
                public string final_price { get; set; }
            }

            public Dictionary<string, ProductData> products { get; set; }
            public PriceData prices { get; set; }

            public string status { get; set; }
            public List<string> status_messages { get; set; }
        }

        public string purchase_id { get; set; }
        public string unique_token { get; set; }
        public Dictionary<string,SiteData> sites { get; set; }
    }

    public class FinishedModel
    {
        public class SiteData
        {
            public string order_id { get; set; }
            public string status { get; set; }
            public List<String> status_messages { get; set; }
        }

        public string purchase_id { get; set; }
        public Dictionary<string, SiteData> sites { get; set; }
    }

    public class PurchaseController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {            
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddToCart(AddToCartModel model)
        {
            Product p = Search.ProductId(model.ProductId);
           
            List<string> products = new List<string>();

            string affiliateUrl = p.Offers[0].Url;
            string product_url = affiliateUrl;

            try {
                Uri url = new Uri(affiliateUrl);

                string q = url.Query;

                if (q.Contains("murl=")) // linkshare affiliate URL
                {
                    product_url = q.Substring(q.IndexOf("murl=")).Replace("murl=", "");
                }
                else if (q.Contains("url=")) // CJ affiliate URL
                {
                    product_url = q.Substring(q.IndexOf("url=")).Replace("url=", "");
                }
                //product_url = q.Replace("?murl=", "").Replace("?url=", "");

                 /*               
                NameValueCollection qs = HttpUtility.ParseQueryString((affiliateUrl.IndexOf('?') < affiliateUrl.Length -1) ? WebUtility.UrlEncode(affiliateUrl.Substring(affiliateUrl.IndexOf('?')) + 1) : string.Empty);

                product_url = qs.Get("url");  // actual product URL is a url= param in the affiliate link              
  
                if (!String.IsNullOrEmpty(qs.Get("murl")))
                {
                    product_url = qs.Get("murl");
                }*/

                // build.com hack
                if (product_url.Contains("searchmarketing.com"))
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(product_url);
                    request.Method = "HEAD";

                    var r = request.GetResponse();
                    if (r.ResponseUri.AbsoluteUri != product_url)
                        product_url = r.ResponseUri.AbsoluteUri;
                }
            }
            catch { }

            products.Add(product_url);

            var response = Client.AddToCart(GetCurrentUserId(), products);

            if (response.IsSuccess)
            {
                if (response.Data.Message == "still_processing" || response.Data.Message == "done")
                {
                    return Json(new { result = true, message = "Product added to cart successfully.", cart_id = response.Data.CartId, affiliate_link = affiliateUrl });
                }
                else if (response.Data.Message == "has_failures")
                {
                    return JsonResponse(false, "This product is not supported by wishlu buy now. Please follow the offer link to the seller's website to complete your purchase.");
                }
                else
                {
                    return JsonResponse(false, "wishlu's buy now feature is currently experiencing technical difficulties. Try your purchase again in a minute.");
                }
            }
            else
                return JsonResponse(false, "There was an error adding this product to your cart.");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Unavailable(string product)
        {
            // TODO: TwoTap API calls this when their API is called by us with a product URL that is out of stock or not supported. Useful for out of stock products!

            return View("Unavailable", model: product);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ValidateShipping(ValidateShippingModel model)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.Address))
                    return JsonResponse(false, "Please provide a shipping address.");

                if (String.IsNullOrEmpty(model.City))
                    return JsonResponse(false, "Please provide a city.");

                if (String.IsNullOrEmpty(model.FirstName))
                    return JsonResponse(false, "Please specify the recipient's first name.");

                if (String.IsNullOrEmpty(model.LastName))
                    return JsonResponse(false, "Please specify the recipient's last name.");

                if (String.IsNullOrEmpty(model.State))
                    return JsonResponse(false, "Please specify a state.");
                
                if (String.IsNullOrEmpty(model.Telephone))
                    return JsonResponse(false, "Please specify the recipient's telephone number.");

                if (model.Telephone.Length != 10)
                    return JsonResponse(false, "Please specify a valid 10-digit phone number.");

                if (String.IsNullOrEmpty(model.ZIP))
                    return JsonResponse(false, "Please specify a ZIP Code.");

                if (model.ZIP.Length != 5)
                    return JsonResponse(false, "A ZIP Code should be exactly 5 digits.");

                return JsonResponse(true, "Shipping address is valid.");
            }
            else
            {
                return JsonResponse(false, "The server received an invalid model.");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ValidateBilling(ValidateBillingModel model)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.Address))
                    return JsonResponse(false, "Please provide a billing address.");

                if (String.IsNullOrEmpty(model.City))
                    return JsonResponse(false, "Please provide a city.");

                if (String.IsNullOrEmpty(model.FirstName))
                    return JsonResponse(false, "Please specify the billing first name.");

                if (String.IsNullOrEmpty(model.LastName))
                    return JsonResponse(false, "Please specify the billing last name.");

                if (String.IsNullOrEmpty(model.State))
                    return JsonResponse(false, "Please specify a state.");

                if (String.IsNullOrEmpty(model.Telephone))
                    return JsonResponse(false, "Please specify a billing telephone number.");

                if (model.Telephone.Length != 10)
                    return JsonResponse(false, "Please specify a valid 10-digit phone number.");

                if (String.IsNullOrEmpty(model.ZIP))
                    return JsonResponse(false, "Please specify a ZIP Code.");

                if (model.ZIP.Length != 5)
                    return JsonResponse(false, "A ZIP Code should be exactly 5 digits.");

                if (String.IsNullOrEmpty(model.CardName))
                    return JsonResponse(false, "Please specify the card holder's first and last name.");

                if (String.IsNullOrEmpty(model.CardNumber))
                    return JsonResponse(false, "Please specify the card number.");

                if (String.IsNullOrEmpty(model.CardType))
                    return JsonResponse(false, "Please specify the card's network type (e.g. Visa, Masterard, American Express, Discover).");

                if (String.IsNullOrEmpty(model.CVV))
                    return JsonResponse(false, "Please specify the card's 3-digit or 4-digit CVV code.");

                if (String.IsNullOrEmpty(model.ExpiryMonth))
                    return JsonResponse(false, "Please specify the card's month of expiration.");

                if (String.IsNullOrEmpty(model.ExpiryYear))
                    return JsonResponse(false, "Please specify the card's year of expiration.");
                
                return JsonResponse(true, "Billing and payment information is valid.");
            }
            else
            {
                return JsonResponse(false, "The server received an invalid model.");
            }
        }
        
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Checkout(CheckoutModel model)
        {
            if (ModelState.IsValid)
            {
                var response = Client.GetCartEstimates(model.CartId, model.SiteId, model.ProductId, model.ShippingZIP, model.ShippingCity, model.ShippingState, model.ProductUrl);

                dynamic res = new ExpandoObject();
                res.Estimates = response;
                res.Model = model;

                if (response.IsSuccess)
                {
                    return View("Checkout", res);
                }

                throw new Exception("Estimates response failed!");

                //return View("Checkout", model);
            }
            else
            {
                return JsonResponse(false, "The server received an invalid model.");
            }
        }
        
        [AllowAnonymous]
        public ActionResult Confirm(ConfirmModel model)
        {
            var response = Client.Confirm(model.purchase_id);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            Logger.Log(json);

            return Json(model);
        }

        [AllowAnonymous]
        public ActionResult Finished(FinishedModel model)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            Logger.Log(json);

            return Json(new { });
        }

        [AllowAnonymous]
        public ActionResult Cart(string cart_id)
        {
            var response = Client.GetCartStatus(GetCurrentUserId(), cart_id);

            return View("Cart", response);
        }
                
        [Authorize]
        public ActionResult TwoTap_Checkout(string product)
        {            
            return View("TwoTap", (object)product);
        }

        public ActionResult TwoTap_Confirm(string purchase_id, string test_mode = "")
        {
            var apiURL = "https://api.twotap.com";
           // var public_token = "147b1eed7550448a6d435c98975322";
            var private_token = "3a89a336d7f7e1f63ab8809db94b5e9e4762ff5e546a67c524";

            var callPath = "/v1.0/purchase_confirm?private_token=" + private_token;

            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> t = client.PostAsync(apiURL + callPath, new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new {form = new {@purchase_id=purchase_id, @test_mode=test_mode}}), Encoding.UTF8, "application/json"));
            t.RunSynchronously();
            Task<String> t2 = t.Result.Content.ReadAsStringAsync();
            t2.RunSynchronously();
            string json = t2.Result;

            return View("TwoTap_Confirm");
        }
    }
}

