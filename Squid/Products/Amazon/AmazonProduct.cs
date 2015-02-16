using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squid.Products.Amazon
{    
    public class AmazonProduct
    {
        public string ASIN { get; set; }

        public string Name { get; set; }

        public string LongDescription { get; set; }

        public string ShortDescription { get; set; }

        public string Url { get; set; }

        public string UPC { get; set; }

        public string SKU { get; set; }

        public string Manufacturer { get; set; }

        public string Price { get; set; }

        public string Image { get; set; }

        public string Availability { get; set; }

        public string Color { get; set; }
    }
}
