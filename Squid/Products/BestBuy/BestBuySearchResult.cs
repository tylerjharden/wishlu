using System;

namespace Squid.Products.BestBuy
{
    public class BestBuySearchResult : ISearchResult
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string ImageUrl { get; set; }
        public string ImageWidth { get; set; }
        public string ImageHeight { get; set; }
        public string UPC { get; set; }
        public string Availability { get; set; }
        public int Score { get; set; }
        public bool IsMilkshake { get; set; }
        public bool Exclusive { get; set; }
        public string Store { get; set; }
        public Guid Id { get; set; }
        public string SKU { get; set; }

        public BestBuySearchResult()
        {
            IsMilkshake = false;
            Store = "BestBuy";
            Id = Guid.Empty;
        }
    }
}
