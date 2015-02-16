using Milkshake;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;
using System.Xml;

namespace Disco.Controllers
{
    public class LinkshareProduct
    {
        public string MerchantName;
        public string ProductName;
        public string ImageUrl;
        public string Price;
        public string LinkUrl;
    }

    public class CJProduct
    {
        public string MerchantName;
        public string ProductName;
        public string ImageUrl;
        public string Price;
        public string LinkUrl;
    }
        
    public class SearchController : BaseController
    {
        [Authorize]
        public ActionResult Index(string query = "")
        {
            ViewBag.Query = query;

            return View("Index");
        }

        // Internal Searches
        [Authorize]
        [HttpGet]
        public ActionResult Members()
        {            
            return View("Members");
        }

        [Authorize]
        [HttpPost]
        public ActionResult Members(FormCollection col)
        {            
            return View("MembersResults");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Shops()
        {                       
            return View("Shops");
        }

        [Authorize]
        [HttpPost]
        public ActionResult Shops(FormCollection col)
        {                        
            return View("ShopsResults");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Products()
        {                        
            return View("Products");
        }

        //[Authorize]
       // [HttpGet]
        //public JsonResult Tokens(string q)
        //{
        //    var tokens = Squid.Text.Tokens.Get(q);
        //                                       
        //    return Json(tokens, JsonRequestBehavior.AllowGet);
        //}

        [Authorize]
        [HttpPost]
        public ActionResult Products(string txtSearch)
        {            
            IEnumerable<Product> results = new List<Product>();

            int c = Milkshake.ProductManager.GetProductsCount(txtSearch);
            results = Milkshake.ProductManager.GetProducts(txtSearch);

            ViewBag.SearchQuery = txtSearch;
            ViewBag.ProductCount = c;
                       
            return View("ProductsResults", results);
        }

        // AJAX Operation
        [Authorize]
        public ActionResult GetPage(string query, int page)
        {            
            IEnumerable<Product> results = new List<Product>();

            results = Milkshake.ProductManager.GetProducts(query, page);

            return View("GetPage", results);
        }
        
        // External Searches
        /*[Authorize]
        public ActionResult Amazon(string txtSearch) 
        {            
            AmazonProvider ap = new AmazonProvider();
            List<ISearchResult> res = ap.Search(txtSearch);
            return View("Amazon", res);
        }*/

        [Authorize]
        public ActionResult Linkshare()
        {            
            return View("Linkshare");
        }

        [Authorize]
        public ActionResult Graph()
        {            
            return View("Graph");
        }

        [Authorize]
        public ActionResult GraphSearch(string s)
        {            
            IEnumerable<Product> results = new List<Product>();

            results = Milkshake.ProductManager.GetProducts(s);

            ViewBag.SearchQuery = s;

            return View("GraphResults", results);
        }
                
        [Authorize]
        public ActionResult LinkshareSearch(string txtSearch)
        {            
            string token;
            string url;
            string keyword;
            string resturl;
            int maxresults;
            url = "http://productsearch.linksynergy.com/productsearch";
            token = "32f1f9b47d9c05291f9424b0a21a53fa429e1521509d390d3c6110ace39f0692";
            keyword = txtSearch;
            maxresults = 20;

            resturl = url + "?" + "token=" + token + "&" + "keyword=" + keyword + "&max=" + maxresults.ToString();

            Uri objuri;
            WebRequest objrequest;
            WebResponse objresponse;
            Stream objstream;
            
            objuri = new Uri(resturl);
            objrequest = WebRequest.Create(objuri);

            objresponse = objrequest.GetResponse();
            objstream = objresponse.GetResponseStream();

            List<LinkshareProduct> res = new List<LinkshareProduct>();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(objstream);

                XmlNodeList nodes = doc.SelectNodes("/result/item");

                foreach (XmlNode node in nodes)
                {
                    LinkshareProduct p = new LinkshareProduct();

                    p.MerchantName = node["merchantname"].InnerText;
                    p.ProductName = node["productname"].InnerText;
                    p.ImageUrl = node["imageurl"].InnerText;
                    p.Price = node["price"].InnerText;

                    p.LinkUrl = node["linkurl"].InnerText;

                    res.Add(p);
                }
            }
            catch
            {

            }

            return View("LinkshareSearch", res);
        }

        [Authorize]
        public ActionResult CJ()
        {            
            return View("CJ");
        }

        [Authorize]
        public ActionResult CJSearch(FormCollection frm)
        {            
            string token;
            string webid;
            string url;
            string keywords;
            string resturl;
            //int maxresults;
            url = "https://product-search.api.cj.com/v2/product-search";
            token = "00dd90ee2e4cd3865f4f6bc9a3b3affc09efe0e5643ba81db43f929e8bc5b5ab4f3c2cd3d7209fdd5bbdcf4649d09989b6551821a1d80f6981c188f4047d716685/78c5863ef09ba03415ab9056ce1c504aee596d508c801fe8df7a4137b90644a0c15b2d2077799421f6627db4aa0635716ba71ed955e14e7742a9fbd10daff161";
            webid = "7496421";
            keywords = Request.Form["txtSearch"]; // txtSearch;
            //maxresults = 20;

            string adids = "";

            if (Request.Form["carparts_cb"] == "on")
                adids = adids + "3440565,";

            if (Request.Form["creative_cb"] == "on")
                adids = adids + "878483,";

            if (Request.Form["elf_cb"] == "on")
                adids = adids + "2451878,";

            if (Request.Form["hartstrings_cb"] == "on")
                adids = adids + "2509798,";

            if (Request.Form["zagg_cb"] == "on")
                adids = adids + "2218454,";

            if (Request.Form["jellybelly_cb"] == "on")
                adids = adids + "2959067,";

            if (Request.Form["logitech_cb"] == "on")
                adids = adids + "2534397,";

            if (Request.Form["skechers_cb"] == "on")
                adids = adids + "2306831,";

            if (adids.EndsWith(","))
                adids.TrimEnd(',');

            resturl = url + "?" + "website-id=" + webid + "&advertiser-ids=" + adids + "&" + "keywords=" + keywords + "&serviceable-area=US"; // + "&max=" + maxresults.ToString();

            Uri objuri;
            WebRequest objrequest;
            WebResponse objresponse;
            Stream objstream;

            objuri = new Uri(resturl);
            objrequest = WebRequest.Create(objuri);
            objrequest.Headers.Add("authorization", token);
            objrequest.Method = "GET";

            objresponse = objrequest.GetResponse();
            objstream = objresponse.GetResponseStream();

            List<CJProduct> res = new List<CJProduct>();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(objstream);

                XmlNodeList nodes = doc.SelectNodes("/cj-api/products/product");

                foreach (XmlNode node in nodes)
                {
                    CJProduct p = new CJProduct();

                    p.MerchantName = node["advertiser-name"].InnerText;
                    p.ProductName = node["name"].InnerText;
                    p.ImageUrl = node["image-url"].InnerText;
                    p.Price = node["price"].InnerText;

                    p.LinkUrl = node["buy-url"].InnerText;

                    res.Add(p);
                }
            }
            catch
            {

            }

            return View("CJSearch", res);
        }
    }
}