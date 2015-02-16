using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace Squid.Products.BestBuy
{
    public static class BestBuyProvider
    {
        private static string apikey = "rhhnge84zb7zvcw5gr7quxd9";
        public static string linksharePublisherID = "BbABvEfAobY";
                
        public static BestBuyProduct Lookup(string sku)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.remix.bestbuy.com/v1/products/" + sku + ".json?apiKey=" + apikey + "&LID=" + Milkshake.Linkshare.Linkshare.SiteID);
            request.Method = "GET";
            request.ContentType = "application/json;charset=utf-8";

            StreamReader response = new StreamReader(request.GetResponse().GetResponseStream());

            string res = response.ReadToEnd();

            //dynamic obj = JsonConvert.DeserializeObject(res);
            //string json = JsonConvert.SerializeObject(obj.products[0]);

            BestBuyProduct p = JsonConvert.DeserializeObject<BestBuyProduct>(res);

            return p;
        }

        public static List<Milkshake.Product> Search(string keywords)
        {
            return Search(keywords, 0);
        }

        // Stream product elements in LinkShare/CJ Catalog XML to an XElement that we can enumerate
        private static IEnumerable<XElement> Products(this XmlReader source)
        {
            while (source.Read())
            {
                source.MoveToContent();

                if (source.NodeType == XmlNodeType.Element &&
                source.Name == "product")
                {
                    yield return (XElement)XElement.ReadFrom(source);
                }
            }
        }

        public static List<Milkshake.Product> Search(string keywords, int page = 0) 
        {
            List<Milkshake.Product> ret = new List<Milkshake.Product>();
            page++;

            HttpWebRequest request;
            Stream stream;

            try
            {
                request = (HttpWebRequest)WebRequest.Create("http://api.remix.bestbuy.com/v1/products(search=" + WebUtility.UrlEncode(keywords) + ")?apiKey=" + apikey + "&page=" + page + "&LID=" + linksharePublisherID);
                request.Method = "GET";
                request.ContentType = "text/xml;charset=utf-8";

                stream = request.GetResponse().GetResponseStream();
            }
            catch
            {
                request = (HttpWebRequest)WebRequest.Create("http://api.remix.bestbuy.com/v1/products(search=" + keywords + ")?apiKey=" + apikey + "&page=" + page + "&LID=" + linksharePublisherID);
                request.Method = "GET";
                request.ContentType = "text/xml;charset=utf-8";

                stream = request.GetResponse().GetResponseStream();
            }
            
            //StreamReader rdr = new StreamReader(request.GetResponse().GetResponseStream());

            //XmlDocument doc = new XmlDocument();
            //doc.Load(rdr);
            //rdr.Close();

            //Parallel.ForEach(doc.GetElementsByTagName("product").Cast<XmlNode>(), pro =>

            using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
            {
                Parallel.ForEach(reader.Products(), pro =>
                {
                    Milkshake.Product ritem = new Milkshake.Product();
                    ritem.IsMilkshake = false;
                    ritem.Store = "BestBuy";

                    try { ritem.Name = pro.Element("name").Value; }
                    catch { return; }
                    try { ritem.Description = pro.Element("longDescription").Value; }
                    catch { }
                    try { ritem.UPC = pro.Element("upc").Value; }
                    catch { }
                    try { ritem.SKU = pro.Element("sku").Value; }
                    catch { return; }
                                        
                    //if (pro["linkShareAffiliateUrl"] != null)
                    //    ritem.Url = pro["linkShareAffiliateUrl"].InnerText;

                    //if (String.IsNullOrEmpty(ritem.Url))
                    //    ritem.Url = pro["url"].InnerText;

                    try { ritem.Price = pro.Element("salePrice").Value.Replace("$", ""); }
                    catch { return; }

                    try { ritem.Image = pro.Element("largeImage").Value; }
                    catch 
                    {
                        try { ritem.Image = pro.Element("mediumImage").Value; }
                        catch 
                        {
                            try { ritem.Image = pro.Element("image").Value; }
                            catch
                            {
                                ritem.Image = "//assets.wishlu.com/images/DefaultWish.jpg";
                            }
                        }                        
                    }

                    if (String.IsNullOrEmpty(ritem.Image))
                        return;
                   
                    ret.Add(ritem);
                });
            }
                        
            return ret;                        
        }
    }
}
