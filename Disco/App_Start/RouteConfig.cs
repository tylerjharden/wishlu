using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Disco
{
   public class RouteConfig
   {
       public static void RegisterRoutes(RouteCollection routes)
       {
           // clear existing routes, insuring only routes defined here are active
           routes.Clear();

           // Turns off the unnecessary file exists check
           routes.RouteExistingFiles = true;

           // Ignore text, html, htm files
           routes.IgnoreRoute("{file}.txt");
           routes.IgnoreRoute("{file}.htm");
           routes.IgnoreRoute("{file}.html");
           routes.IgnoreRoute("{file}.xml");

           // Ignore axd files such as assets, images, sitemaps, etc.           
           routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

           // Ignore the error directory which contains error pages
           routes.IgnoreRoute("ErrorPages/{*pathInfo}");

           // Exclude the favicon (google toolbar requests gif file as fav icon which is weird)
           routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.([iI][cC][oO]|[gG][iI][fF])(/.*)?" });
           
           // wishlu Routes
                      
           // Route "no controller" actions to Home controller
           routes.MapRoute(
             "Root",
             "{action}",
             new { controller = "Home", action = "Index" },
             new { IsRootAction = new IsRootActionConstraint() }  // Route Constraint
            );

           // Route signin shortcut
           routes.MapRoute(
             "Signin",
             "signin",
             new { controller = "Home", action = "SignIn" }             
            );

           // Privacy/TOS Shortcut
           routes.MapRoute(
             "Privacy",
             "privacy",
             new { controller = "Home", action = "Privacy" }
            );
           routes.MapRoute(
             "TOS",
             "tos",
             new { controller = "Home", action = "TOS" }
            );

           // Route usernames to user controller
           routes.MapRoute(
             "Usernames",
             "{username}",
             new { controller = "User", action = "ViewUsername", username = UrlParameter.Optional },
             new { IsUsername = new IsUsernameConstraint() }  // Route Constraint
            );
                                            
           ////////////////
           // Shortcuts  //
           ////////////////

           // Wish/Item           
           routes.MapRoute(
               name: "i",
               url: "i/{id}",
               defaults: new { controller = "Item", action = "View", id = UrlParameter.Optional }
           );

           // Gift           
           routes.MapRoute(
               name: "g",
               url: "g/{id}",
               defaults: new { controller = "Gift", action = "View", id = UrlParameter.Optional }
           );

           // User/Profile           
           routes.MapRoute(
               name: "u",
               url: "u/{id}",
               defaults: new { controller = "User", action = "View", id = UrlParameter.Optional }
           );

           // Wishlu
           routes.MapRoute(
               name: "l",
               url: "l/{id}",
               defaults: new { controller = "WishLu", action = "View", id = UrlParameter.Optional }
           );

           // Wishloop
           routes.MapRoute(
               name: "o",
               url: "o/{id}",
               defaults: new { controller = "WishLoop", action = "View", id = UrlParameter.Optional }
           );

           // Product
           routes.MapRoute(
               name: "p",
               url: "p/{id}",
               defaults: new { controller = "Product", action = "View", id = UrlParameter.Optional }
           );

           // Store/Shop
           routes.MapRoute(
               name: "s",
               url: "s/{id}",
               defaults: new { controller = "Store", action = "View", id = UrlParameter.Optional }
           );

           // Category
           routes.MapRoute(
               name: "c",
               url: "c/{id}",
               defaults: new { controller = "Category", action = "View", id = UrlParameter.Optional }
           );
                      
           ////////////////////
           // Standard Route //
           ////////////////////
           routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
           );

       }
   }

   public class IsRootActionConstraint : IRouteConstraint
   {
       private Dictionary<string, Type> _controllers;
       private Dictionary<string, MethodInfo> _actions;

       public IsRootActionConstraint()
       {
           _controllers = Assembly
                               .GetCallingAssembly()
                               .GetTypes()
                               .Where(type => type.IsSubclassOf(typeof(Controller)))
                               .ToDictionary(key => key.Name.Replace("Controller", "").ToLower());

           _actions = Assembly
               .GetCallingAssembly()
               .GetTypes()
               .Where(type => type.Name == "HomeController")
               .Single()
               .GetMethods()
               .Where(method => method.ReturnType.IsSubclassOf(typeof(ActionResult)))
               .ToDictionary(key => key.Name.ToLower());
       }

       #region IRouteConstraint Members

       public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
       {
           return !_controllers.Keys.Contains((values["action"] as string).ToLower()) && _actions.Keys.Contains((values["action"] as string).ToLower());
       }

       #endregion
   }

   public class IsUsernameConstraint : IRouteConstraint
   {       
       public IsUsernameConstraint()
       {           
       }

       private HashSet<string> banned = new HashSet<string>
       {
           "wishlu",
           "wishloop",
           "wish",
           "item",
           "dash",
           "index",
           "privacy",
           "blocking",
           "notifications",
           "mobile",
           "findfriends",
           "social",
           "twitter",
           "facebook",
           "google",
           "w",
           "l",
           "s",
           "o",
           "p",
           "m",
           "b",
           "u",
           "hunt",
           "friends",
           "calendar",
           "inbox",
           "messages",
           "items",
           "wishlus",
           "wishloops",
           "invite"
       };
              
       public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
       {
           if (!values.ContainsKey("username"))
               return false;
                      
           string username = values["username"] as string;

           if (String.IsNullOrEmpty(username))
               return false;

           if (username.Length < 2)
               return false;

           if (username.Contains('\\'))
               return false;

           if (username.Contains("/"))
               return false;

           username = username.ToLower();

           if (banned.Contains(username))
               return false;

           return !Squid.Users.User.IsHandleAvailable(username);
       }

   }
}