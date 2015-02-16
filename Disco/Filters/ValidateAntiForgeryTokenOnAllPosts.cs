using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Disco.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ValidateAntiForgeryTokenOnAllPosts : AuthorizeAttribute
    {
        private static List<Uri> trustedReferrers = new List<Uri>
        {
            new Uri ("https://apps.facebook.com/wishludev"),
            new Uri ("https://apps.facebook.com/wishludev/")
        };

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            if (!request.IsAuthenticated)
                return; // we only care about loggedin requests

            //  Only validate POSTs
            if (request.HttpMethod == WebRequestMethods.Http.Post && filterContext.Controller.GetType() != typeof(Disco.Controllers.CJController) && filterContext.Controller.GetType() != typeof(Disco.Controllers.PurchaseController))
            {
                //  Ajax POSTs and normal form posts have to be treated differently when it comes
                //  to validating the AntiForgeryToken
                if (request.IsAjaxRequest())
                {
                    var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];

                    var cookieValue = antiForgeryCookie != null
                        ? antiForgeryCookie.Value
                        : null;

                    AntiForgery.Validate(cookieValue, request.Headers["__RequestVerificationToken"]);
                }
                else
                {
                    // ditch the query string
                    string originalUrl = request.UrlReferrer.AbsoluteUri;
                    if (request.UrlReferrer.Query.Length > 0)
                        originalUrl = originalUrl.Replace(request.UrlReferrer.Query, string.Empty);

                    if (trustedReferrers.Contains(new Uri(originalUrl)) || trustedReferrers.Contains(request.UrlReferrer))
                        return;
                    
                    new ValidateAntiForgeryTokenAttribute()
                        .OnAuthorization(filterContext);
                }
            }
        }
    }
}