using System;

namespace Squid.Products.Graph
{
    public class GraphSearchResult : ISearchResult
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string ImageUrl { get; set; }
        public string ImageWidth { get; set; }
        public string ImageHeight { get; set; }
        public string UPC { get; set; }
        public string Availability { get; set; }
        public string Store { get; set; }
        public bool Exclusive { get; set; }
        public bool IsMilkshake { get; set; }
        public int Score { get; set; }

        public GraphSearchResult()
        {
            Id = Guid.Empty;
            Url = "";
            Title = "";
            Description = "";
            Price = "0.00";
            ImageUrl = "//assets.wishlu.com/images/defaultWish.jpg";
            UPC = "000000000000";
            Score = 0;
            IsMilkshake = true;
        }
    }
}
