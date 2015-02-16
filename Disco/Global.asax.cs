// SignalR
using Microsoft.Owin;
using Owin;
// Wishlu
using Squid.Log;
using System;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
//using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

[assembly: OwinStartup(typeof(Disco.Startup))]
namespace Disco
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);            
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType<System.Web.Mvc.JsonValueProviderFactory>().FirstOrDefault());
            ValueProviderFactories.Factories.Add(new LargeJsonValueProviderFactory());

            // BUG: This fixes an InvalidOperation exception everywhere that we use Html.AntiForgeryToken()
            // We need to integrate our login system with the default account/membership providers used by ASP.NET
            //AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;         
            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
                return;

            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
            if (ticket == null)
                return;
            
            if (ticket.Expired)
            {
                Logger.Log("Expire FormsAuth Ticket - " + ticket.Name);
                Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
                FormsAuthentication.SignOut();
                return;
            }
            else
            {
                ticket = FormsAuthentication.RenewTicketIfOld(ticket);
            }

            // If we have an anonymous ticket then toss it...there is no reason for them
            if (ticket.Name == "" || ticket.Name == String.Empty || null == ticket.Name)
            {
                Logger.Log("Empty FormsAuth Ticket - ");
                Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
                FormsAuthentication.SignOut();
                return;
            }

            // If it is a proper forms authentication ticket then we will be able to grab a user from the database
            //Squid.Users.UserIdentity identity = new Squid.Users.UserIdentity(ticket.Name);
            //Squid.Users.UserPrincipal principal = new Squid.Users.UserPrincipal(identity);
            string[] userData = ticket.UserData.Split('|');

            GenericIdentity identity = new GenericIdentity(ticket.Name);
            GenericPrincipal principal = new GenericPrincipal(identity, userData);
            Context.User = principal;

            //Logger.Log("AuthenticateRequest - " + ticket.Name);            
        }                
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}