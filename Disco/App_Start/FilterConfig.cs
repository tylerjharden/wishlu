using Disco.Filters;
using System.Web.Mvc;

namespace Disco
{
   public class FilterConfig
   {
      public static void RegisterGlobalFilters(GlobalFilterCollection filters)
      {
         filters.Add(new HandleErrorAttribute());
         filters.Add(new ValidateAntiForgeryTokenOnAllPosts());
         filters.Add(new RequireHttpsAttribute());
      }
   }
}