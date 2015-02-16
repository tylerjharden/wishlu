
using System;
namespace Squid.Products.Amazon
{
    public class AmazonSearchResult : ISearchResult
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
        public string ASIN { get; set; }

        public AmazonSearchResult()
        {
            IsMilkshake = false;
            Store = "Amazon";
            Id = Guid.Empty;
        }
    }
}
