using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.ViewModels
{
    [Serializable]
    public class TwoTapCheckout
    {
        public Guid UniqueId { get; set; }

        public string CallbackUrl { get; set; }


        public TwoTapCheckout()
        {
            CallbackUrl = "http://www.wishlu.com/purchase/confirm";
        }
    }
}