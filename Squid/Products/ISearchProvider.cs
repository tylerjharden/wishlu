using System.Collections.Generic;
using System.Xml;

namespace Squid.Products
{
    public interface ISearchProvider
    {
        List<Milkshake.Product> Search(string keywords);        
    }
}
