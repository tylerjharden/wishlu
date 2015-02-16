using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Butler.Models
{
    public class Wishlu
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string EventDateTime { get; set; }
    }
}