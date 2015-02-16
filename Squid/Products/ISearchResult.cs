using System;

namespace Squid.Products
{
    public interface ISearchResult
    {
        string Url { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        string Price { get; set; }
        string ImageUrl { get; set; }
        string ImageWidth { get; set; }
        string ImageHeight { get; set; }
        string UPC { get; set; }
        string Availability { get; set; }
        int Score { get; set; }
        string Store { get; set; }
        bool Exclusive { get; set; }
        bool IsMilkshake { get; }
        Guid Id { get; set; }
    }
}
