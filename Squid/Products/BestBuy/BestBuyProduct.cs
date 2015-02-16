using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squid.Products.BestBuy
{    
    public class BestBuyProduct
    {
        [JsonProperty("sku")]
        public string SKU { get; set; }

        [JsonProperty("productID")]
        public string ProductID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("new")]
        public bool New { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("lowPriceGuarantee")]
        public bool LowPriceGuarantee { get; set; }

        [JsonProperty("activeUpdateDate")]
        public DateTime ActiveUpdateDate { get; set; }

        [JsonProperty("regularPrice")]
        public decimal RegularPrice { get; set; }

        [JsonProperty("salePrice")]
        public decimal SalePrice { get; set; }

        [JsonProperty("onSale")]
        public bool OnSale { get; set; }

        //planPrice : Decimal (Cell-Phone 2 Year Plan Price)
        //priceWithPlan : List 

        [JsonProperty("priceRestriction")]
        public string PriceRestriction { get; set; }

        [JsonProperty("priceUpdateDate")]
        public DateTime PriceUpdateDate { get; set; }

        [JsonProperty("digital")]
        public bool Digital { get; set; }

        [JsonProperty("preowned")]
        public string PreOwned { get; set; }

        // carrierPlans : List (List of Carrier's and Cell Phone Plans)

        // technologyCode
        // carrierModelNumber
        // earlyTerminationFees : List

        [JsonProperty("outletCenter")]
        public string OutlerCenter { get; set; }

        [JsonProperty("secondaryMarket")]
        public string SecondaryMarket { get; set; }

        [JsonProperty("frequentlyPurchasedWith")]
        public List<Tuple<string,string>> FrequentlyPurchasedWith{ get; set; }

        [JsonProperty("accessories")]
        public List<Tuple<string, string>> Accessories { get; set; }

        [JsonProperty("relatedProducts")]
        public List<Tuple<string, string>> RelatedProducts { get; set; }

        [JsonProperty("techSupportPlans")]
        public List<Tuple<string, string>> TechSupportPlans { get; set; }

        // salesRankShortTerm : Integer
        // salesRankMediumTerm: Integer
        // SalesRankLongTerm : Integer
        // bestSellkingRank : Integer

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("spin360Url")]
        public string Spin360Url { get; set; }

        [JsonProperty("mobileUrl")]
        public string MobileUrl { get; set; }

        [JsonProperty("affiliateUrl")]
        public string AffiliateUrl { get; set; }

        [JsonProperty("addToCartUrl")]
        public string AddToCartUrl { get; set; }

        [JsonProperty("affiliateAddToCartUrl")]
        public string AffiliateAddToCartUrl { get; set; }

        [JsonProperty("linkshareAffiliateUrl")]
        public string LinkshareAffiliateUrl { get; set; }

        [JsonProperty("linkshareAffiliateAddToCartUrl")]
        public string LinkshareAffiliateAddToCartUrl { get; set; }

        [JsonProperty("upc")]
        public string UPC { get; set; }

        [JsonProperty("productTemplate")]
        public string ProductTemplate { get; set; }

        // category : List

        // lists : List

        [JsonProperty("customerReviewCount")]
        public int? CustomerReviewCount { get; set; }

        [JsonProperty("customerReviewAverage")]
        public decimal? CustomerReviewAverage { get; set; }

        [JsonProperty("customerTopRated")]
        public bool CustomerTopRated { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("freeShipping")]
        public bool FreeShipping { get; set; }

        [JsonProperty("freeShippingEligible")]
        public bool FreeShippingEligible { get; set; }

        [JsonProperty("inStoreAvailability")]
        public bool InStoreAvailability { get; set; }

        // inStoreAvailabilityText : String
        // inStoreAvailabilityUpdateDate : DateTime

        [JsonProperty("itemUpdateDate")]
        public DateTime ItemUpdateDate { get; set; }

        [JsonProperty("onlineAvailability")]
        public bool OnlineAvailability { get; set; }

        [JsonProperty("onlineAvailabilityText")]
        public string OnlineAvailabilityText { get; set; }

        // onlineAvailabilityUpdateDate : DateTime

        [JsonProperty("releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [JsonProperty("shippingCost")]
        public decimal shippingCost { get; set; }

        // shipping : List<Tuple<string,decimal>>

        [JsonProperty("specialOrder")]
        public bool SpecialOrder { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }

        // classId: Integer

        [JsonProperty("subclass")]
        public string Subclass { get; set; }

        // subclassId: Integer

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("departmentId")]
        public int DepartmentId { get; set; }

        // buybackPlans : List

        // protectionPlans: List

        // productFamilies: List

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonProperty("modelNumber")]
        public string ModelNumber { get; set; }

        ////////////
        // Images //
        ////////////
        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("largeFrontImage")]
        public string LargeFrontImage { get; set; }

        [JsonProperty("mediumImage")]
        public string MediumImage { get; set; }

        [JsonProperty("thumbnailImage")]
        public string ThumbnailImage { get; set; }

        [JsonProperty("largeImage")]
        public string LargeImage { get; set; }

        [JsonProperty("alternateViewsImage")]
        public string AlternateViewsImage { get; set; }

        [JsonProperty("angleImage")]
        public string AngleImage { get; set; }

        [JsonProperty("backViewImage")]
        public string BackViewImage { get; set; }

        [JsonProperty("energyGuideImage")]
        public string EnergyGuideImage { get; set; }

        [JsonProperty("leftViewImage")]
        public string LeftViewImage { get; set; }

        [JsonProperty("accessoriesImage")]
        public string AccessoriesImage { get; set; }

        [JsonProperty("remoteControlImage")]
        public string RemoteControlImage { get; set; }

        [JsonProperty("rightViewImage")]
        public string RightViewImage { get; set; }

        [JsonProperty("topViewImage")]
        public string TopViewImage { get; set; }
        // End Images

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("inStorePickup")]
        public bool InStorePickup { get; set; }

        [JsonProperty("friendsAndFamilyPickup")]
        public bool FriendsAndFamilyPickup { get; set; }

        [JsonProperty("homeDelivery")]
        public bool HomeDelivery { get; set; }

        [JsonProperty("quantityLimit")]
        public int QuantityLimit { get; set; }

        [JsonProperty("fulfilledBy")]
        public string FulfilledBy { get; set; }

        // bundleIn : List

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("depth")]
        public string Depth { get; set; }

        [JsonProperty("dollarSavings")]
        public decimal DollarSavings { get; set; }

        [JsonProperty("percentSavings")]
        public decimal PercentSavings { get; set; }

        [JsonProperty("tradeInValue")]
        public decimal? TradeInValue { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("orderable")]
        public string Orderable { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("shippingWeight")]
        public decimal ShippingWeight { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("warrantyLabor")]
        public string WarrantyLabor { get; set; }

        [JsonProperty("warrantyParts")]
        public string WarrantyParts { get; set; }

        [JsonProperty("longDescription")]
        public string LongDescription { get; set; }

        [JsonProperty("includedItemList")]
        public List<Tuple<string,string>> IncludedItemList { get; set; }

        [JsonProperty("marketplace")]
        public bool Marketplace { get; set; }

        [JsonProperty("listingId")]
        public string ListingId { get; set; }

        [JsonProperty("sellerId")]
        public string SellerId { get; set; }

        [JsonProperty("shippingRestrictions")]
        public string ShippingRestrictions { get; set; }
    }
}
