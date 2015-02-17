
namespace Disco.Controllers
{
	/*public class ScrollController : BaseController
	{        
		[Authorize]
		public
		ActionResult
		Index()
		{            
			// RSS Feed
			const string feedUrl = "http://www.amazon.com/gp/rss/movers-and-shakers/beauty/ref=zg_bsms_beauty_rsslink";

			SyndicationFeed feed = null;

			using (XmlReader reader = XmlReader.Create(feedUrl))
			{
				feed = SyndicationFeed.Load(reader);
			}

			if (feed != null)
			{
				SyndicationItem item = feed.Items.First<SyndicationItem>();
				ViewBag.RssItem = item;
			}
						
			return View("Index", AggregateScroll());
		}	

		public ActionResult Chat()
		{
			return View("Chat");
		}
	
		public List<Squid.Scroll.Scribe> AggregateScroll()
		{
			List<Squid.Scroll.Scribe> scroll = Squid.Users.User.GetUserById(GetCurrentUserId()).GetScrollStore().GetScribes(25);

			foreach (Squid.Users.User friend in Squid.Users.User.GetUsersFriends(GetCurrentUserId()))
			{
				Squid.Scroll.ScrollStore s = friend.GetScrollStore();

				scroll.AddRange(s.GetScribes(25));
			}
			
			scroll.Sort((x, y) => y.CreatedOn.CompareTo(x.CreatedOn));

			return scroll;
		}

		public ActionResult LikeScribe(Guid id)
		{
			Squid.Scroll.Scribe scribe = Squid.Scroll.Scribe.GetScribeById(id);

			scribe.Like(GetCurrentUserId());

			return Index();
		}

		public ActionResult UnlikeScribe(Guid id)
		{
			Squid.Scroll.Scribe scribe = Squid.Scroll.Scribe.GetScribeById(id);

			scribe.Unlike(GetCurrentUserId());

			return Index();
		}
		//---------------------------------------------------------------------------------------------//
	}*/
}