using Milkshake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Squid.Products.Graph
{
    public class GraphProvider : ISearchProvider
    {
        public GraphProvider() { }

        public List<Product> Search(string keywords) 
        {
            return Search(keywords, 0);
        }

        public List<Product> Search(string keywords, int page)
        {
            List<Product> ret = new List<Product>();
            
            try
            {
                ret = Milkshake.ProductManager.GetProducts(keywords, page).ToList();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
                        
            try
            {                 
                ret.AddRange(Amazon.AmazonProvider.Search(keywords, page));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }

            try
            {
                ret.AddRange(BestBuy.BestBuyProvider.Search(keywords, page));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
                        
            return ret;
        }        
    }
}
