using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco
{
    public abstract class ViewBase : WebViewPage
    {        
        public Guid UserId 
        {
            get
            {
                try
                {
                    return Guid.Parse(Context.User.Identity.Name);
                }
                catch
                {
                    return Guid.Empty;
                }
            } 
        }
    }

    public abstract class ViewBase<T> : WebViewPage<T>
    {
        public Guid UserId
        {
            get
            {
                try
                {
                    return Guid.Parse(Context.User.Identity.Name);
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }
    }
}