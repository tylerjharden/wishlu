using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

namespace Squid.Scout
{
    public class ScoutInfo
    {
        public string Name;
        public string Address;
        public string City;
        public string State;
        public string ZIPCode;
        public string Price;
    }

    public static class BestBuy
    {
        private static string apikey = "rhhnge84zb7zvcw5gr7quxd9";

        public static List<ScoutInfo> Scout(string upc, string zipcode, string radius)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.remix.bestbuy.com/v1/stores(area(" + zipcode + "," + radius + "))+products(upc=" + upc + ")?apiKey=" + apikey);
            request.Method = "GET";
            request.ContentType = "text/xml;charset=utf-8";

            StreamReader rdr = new StreamReader(request.GetResponse().GetResponseStream());

            XmlDocument doc = new XmlDocument();
            doc.Load(rdr);
            rdr.Close();

            List<ScoutInfo> returnlist = new List<ScoutInfo>();

            foreach (XmlNode store in doc.GetElementsByTagName("store"))
            {
                ScoutInfo info = new ScoutInfo();
                info.Name = store["longName"].InnerText;
                info.Address = store["address"].InnerText;
                info.City = store["city"].InnerText;
                info.State = store["region"].InnerText;
                info.ZIPCode = store["fullPostalCode"].InnerText;
                info.Price = "$" + store["products"]["product"]["salePrice"].InnerText;

                returnlist.Add(info);
            }
                        
            return returnlist;
        }
    }
}
