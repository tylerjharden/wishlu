using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WishLuWebApp.ViewModels
{
	 public class WishLoopShareViewModel
	 {
		  public List<WishLuWebServiceShared.Wishes.Wishloop> wishLoops { get; set; }
		  public WishLuWebServiceShared.Wishes.WishLu wishLu { get; set; }
		  public List<WishLuWebServiceShared.Wishes.WishLuSubscriber> wishLuSubscribers { get; set; }
	 }
}