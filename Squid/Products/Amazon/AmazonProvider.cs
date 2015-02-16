using Squid.Amazon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Squid.Products.Amazon
{
    public static class AmazonProvider
    {
        public static AmazonProduct Lookup(string asin)
        {
            // Instantiate Amazon ProductAdvertisingAPI client
            AWSECommerceServicePortTypeClient amazonClient = new AWSECommerceServicePortTypeClient();

            // ItemLookup request
            ItemLookupRequest request = new ItemLookupRequest();
            request.IdType = ItemLookupRequestIdType.ASIN;
            request.ItemId = new string[] { asin };
            request.ResponseGroup = new string[] { "Images", "ItemAttributes", "ItemIds", "OfferFull", "Offers", "VariationImages", "Variations" };

            ItemLookup lookup = new ItemLookup();
            lookup.Request = new ItemLookupRequest[] { request };
            lookup.AWSAccessKeyId = ConfigurationManager.AppSettings["accessKeyId"];
            lookup.AssociateTag = ConfigurationManager.AppSettings["trackingID"];

            ItemLookupResponse response = amazonClient.ItemLookup(lookup);

            var item = response.Items[0].Item[0]; // should only be getting one item
            
            AmazonProduct ritem = new AmazonProduct();
           
            try { ritem.ASIN = item.ASIN; }
            catch { return null; }
            try { ritem.Url = item.DetailPageURL; }
            catch { return null; }
            try { ritem.Name = item.ItemAttributes.Title; }
            catch { return null; }
            try { ritem.SKU = item.ItemAttributes.SKU; }
            catch { }
            try { ritem.Manufacturer = item.ItemAttributes.Manufacturer; }
            catch { }
            try { ritem.Color = item.ItemAttributes.Color; }
            catch { }
            try { ritem.UPC = item.ItemAttributes.UPC; }
            catch { }
            try { ritem.Availability = item.Offers.Offer[0].OfferListing[0].Availability; }
            catch { }
            try { ritem.Price = item.ItemAttributes.ListPrice.FormattedPrice; }
            catch { return null; }

            try { ritem.Image = item.LargeImage.URL; }
            catch
            {
                try { ritem.Image = item.MediumImage.URL; }
                catch
                {
                    try { ritem.Image = item.SmallImage.URL; }
                    catch
                    {
                        ritem.Image = "//assets.wishlu.com/images/DefaultWish.jpg";
                    }
                }
            }

            if (String.IsNullOrEmpty(ritem.Image))
                return null;

            return ritem;
        }

        public static List<Milkshake.Product> Search(string keywords)
        {
            return Search(keywords, 0);
        }

        public static List<Milkshake.Product> Search(string keywords, int page = 0) 
        {
            List<Milkshake.Product> ret = new List<Milkshake.Product>();
            page++;
            // Instantiate Amazon ProductAdvertisingAPI client
            AWSECommerceServicePortTypeClient amazonClient = new AWSECommerceServicePortTypeClient();

            // prepare an ItemSearch request
            ItemSearchRequest request = new ItemSearchRequest();
            request.SearchIndex = "All";
            request.Keywords = keywords;
            request.ResponseGroup = new string[] { "Images", "ItemAttributes", "ItemIds", "Offers" };
            
            // DEBUG SEARCH REFINING
            //request.Availability = ItemSearchRequestAvailability.Available;
            //request.MerchantId = "Amazon";
            request.ItemPage = (page).ToString();

            ItemSearch itemSearch = new ItemSearch();
            itemSearch.Request = new ItemSearchRequest[] { request };
            itemSearch.AWSAccessKeyId = ConfigurationManager.AppSettings["accessKeyId"];
            itemSearch.AssociateTag = ConfigurationManager.AppSettings["trackingID"];
            
            // send the ItemSearch request
            ItemSearchResponse response = amazonClient.ItemSearch(itemSearch);

            // check for nulls, return empty list
            if (response == null)
                return new List<Milkshake.Product>();

            if (response.Items == null)
                return new List<Milkshake.Product>();

            if (response.Items.Length == 0)
                return new List<Milkshake.Product>();

            if (response.Items[0] == null)
                return new List<Milkshake.Product>();

            if (response.Items[0].Item.Length == 0)
                return new List<Milkshake.Product>();

            // write out the results from the ItemSearch request
            Parallel.ForEach(response.Items[0].Item, item =>
            {                
                Milkshake.Product ritem = new Milkshake.Product();
                ritem.IsMilkshake = false;
                ritem.Store = "Amazon";

                try { ritem.ASIN = item.ASIN; }
                catch { return; }
                //ritem.Url = item.DetailPageURL;
                try { ritem.Name = item.ItemAttributes.Title; }
                catch { return;  }
                //ritem.Description = "";
                try { ritem.UPC = item.ItemAttributes.UPC; }
                catch { }
                try { ritem.Availability = item.Offers.Offer[0].OfferListing[0].Availability; }
                catch { }
                
                try { ritem.Price = item.ItemAttributes.ListPrice.FormattedPrice.Replace("$", ""); }
                catch { return; }

                try { ritem.Image = item.LargeImage.URL; }
                catch 
                {
                    try { ritem.Image = item.MediumImage.URL; }
                    catch
                    {
                        try { ritem.Image = item.SmallImage.URL; }
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
            return ret;
        }
    }
}
