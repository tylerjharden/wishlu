using Squid.Log;
using Squid.Users;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace Disco.Controllers
{
	public abstract class BaseController : Controller
	{
		public System.Web.Caching.Cache Cache
		{
			get { return HttpContext.Cache; }
		}

		//---------------------------------------------------------------------------------------------//
		/// <summary>
		///      Has the object been disposed of yet?  This flag insures that the actual dispose logic
		/// is not run twice even if the dispose procedure is called twice.
		/// </summary>
		private bool disposed = false;
		private Squid.Users.User _currentUser = null;

		public SelectList colorlist = new SelectList(new[]
		{
			new {ID="#95D5E1", Name = "Morning Glory"},            
			new {ID="#D0D543", Name = "Wattle"},
			new {ID="#DD838F", Name = "New York Pink"},
			new {ID="#F2B244", Name = "Tulip Tree"},
			new {ID="#DE3F15", Name = "Tia Maria"},
			new {ID="#C5ACCA", Name = "London Hue"},
			new {ID="#FDF39C", Name = "Picasso"},
			new {ID="#A49689", Name = "Zorba"},
			new {ID="#B3CEE1", Name = "Periwinkle Gray"},
			new {ID="#CBEBC4", Name = "Tea Green"},
		},
		 "ID", "Name", 1);

		public SelectList ratings = new SelectList(new[]
		{
			new {ID="1", Name=""},
			new {ID="2", Name=""},
			new {ID="3", Name = ""},
			new {ID="4", Name = ""},
				new {ID="5", Name = ""},
		},
		 "ID", "Name", 1);

		public SelectList states = new SelectList(new[]
		{				
			new {ID="Alabama", Name="Alabama"},
			new {ID="Alaska", Name="Alaska"},
			new {ID="Arizona", Name="Arizona"},
			new {ID="Arkansas", Name="Arkansas"},
			new {ID="California", Name="California"},
			new {ID="Colorado", Name="Colorado"},
			new {ID="Connecticut", Name="Connecticut"},
			new {ID="Delaware", Name="Delaware"},
			new {ID="District of Columbia", Name="District of Columbia"},
			new {ID="Florida", Name="Florida"},
			new {ID="Georgia", Name="Georgia"},
			new {ID="Guam", Name="Guam"},
			new {ID="Hawaii", Name="Hawaii"},
			new {ID="Idaho", Name="Idaho"},
			new {ID="Illinois", Name="Illinois"},
			new {ID="Indiana", Name="Indiana"},
			new {ID="Iowa", Name="Iowa"},
			new {ID="Kansas", Name="Kansas"},
			new {ID="Kentucky", Name="Kentucky"},
			new {ID="Louisiana", Name="Louisiana"},
			new {ID="Maine", Name="Maine"},
			new {ID="Maryland", Name="Maryland"},
			new {ID="Massachusetts", Name="Massachusetts"},
			new {ID="Michigan", Name="Michigan"},
			new {ID="Minnesota", Name="Minnesota"},
			new {ID="Mississippi", Name="Mississippi"},
			new {ID="Missouri", Name="Missouri"},
			new {ID="Montana", Name="Montana"},
			new {ID="Nebraska", Name="Nebraska"},
			new {ID="Nevada", Name="Nevada"},
			new {ID="New Hampshire", Name="New Hampshire"},
			new {ID="New Jersey", Name="New Jersey"},
			new {ID="New Mexico", Name="New Mexico"},
			new {ID="New York", Name="New York"},
			new {ID="North Carolina", Name="North Carolina"},
			new {ID="North Dakota", Name="North Dakota"},
			new {ID="Ohio", Name="Ohio"},
			new {ID="Oklahoma", Name="Oklahoma"},
			new {ID="Oregon", Name="Oregon"},
			new {ID="Pennsylvania", Name="Pennsylvania"},
			new {ID="Puerto Rico", Name="Puerto Rico"},
			new {ID="Rhode Island", Name="Rhode Island"},
			new {ID="South Carolina", Name="South Carolina"},
			new {ID="South Dakota", Name="South Dakota"},
			new {ID="Tennessee", Name="Tennessee"},
			new {ID="Texas", Name="Texas"},
			new {ID="Utah", Name="Utah"},
			new {ID="Vermont", Name="Vermont"},
			new {ID="U.S. Virgin Islands", Name="U.S. Virgin Islands"},
			new {ID="Virginia", Name="Virginia"},
			new {ID="Washington", Name="Washington"},
			new {ID="West Virginia", Name="West Virginia"},
			new {ID="Wisconsin", Name="Wisconsin"},
			new {ID="Wyoming", Name="Wyoming"},
			new {ID="Armed Forces Americas", Name="Armed Forces Americas"},
			new {ID="Armed Forces Europe", Name="Armed Forces Europe"},
			new {ID="Armed Forces Pacific", Name="Armed Forces Pacific"},
		},
			  "ID", "Name", 1);

		public BaseController()
		{
			ViewData["colorlist"] = colorlist;
			ViewData["states"] = states;            
		}

		protected override void OnException(ExceptionContext filterContext)
		{
			// e-mail wishlu developers when an exception occurs on a controller
			Logger.Error(filterContext.Exception);

			// Output a nice error page
			if (filterContext.HttpContext.IsCustomErrorEnabled)
			{
				filterContext.ExceptionHandled = true;
				this.View("Error").ExecuteResult(this.ControllerContext);
			}
		}

		protected override IAsyncResult BeginExecute(System.Web.Routing.RequestContext requestContext, AsyncCallback callback, object state)
		{
			requestContext.HttpContext.Response.AddHeader("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");

			return base.BeginExecute(requestContext, callback, state);
		}
				
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (Request.IsAuthenticated || HttpContext.User.Identity.IsAuthenticated)
			{
				if (Session == null || Session["FirstName"] == null || Session["LastName"] == null)
				{
					if (_currentUser == null)
					{
						_currentUser = GetCurrentUser();
					}

					PopulateSession(_currentUser);
					Logger.Log("Rebuilt empty session data for logged in user.");
				}
			}
			
			base.OnActionExecuting(filterContext);
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;
				if (disposing)
				{
					_currentUser = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
		{
			return new JsonResult()
			{
				Data = data,
				ContentType = contentType,
				ContentEncoding = contentEncoding,
				JsonRequestBehavior = behavior,
				MaxJsonLength = Int32.MaxValue
			};
		}

		public JsonResult JsonResponse(bool result, string message, bool allowGet = true)
		{
			return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
		}
				
		public void ValidateAdmin()
		{
			if (Session["IsAdmin"] == null || (bool)Session["IsAdmin"] == false)
			{
				Logger.Log("Non-admin tried to enter admin area!");
				if (!Response.IsRequestBeingRedirected)
				{                    
					Response.Redirect("/signin");
				}
			}
		}

		public Guid GetCurrentUserId()
		{
			try
			{
				return Guid.Parse(HttpContext.User.Identity.Name);
			}
			catch
			{
				return Guid.Empty;
			}
		}

		public User CurrentUser
		{
			get 
			{
				if (_currentUser == null)
					_currentUser = GetCurrentUser();

				return _currentUser; 
			}
			private set
			{
				if (value == null)
					return;

				_currentUser = value;
			}
		}

		public User GetCurrentUser()
		{
			try
			{
				return Squid.Users.User.GetUserById(GetCurrentUserId());
			}
			catch (Squid.ItemNotFoundException)
			{
				return null;
			}
		}

		public void PopulateSession(User user)
		{
			//Session["WUSID"] = user.SessionId.ToString();
			Session["UID"] = user.Id;
			Session["FirstName"] = user.FirstName;
			Session["LastName"] = user.LastName;
			Session["Email"] = user.Email;
			Session["DOB"] = user.DateOfBirth;
			Session["ImageURL"] = user.ImageUrl;
			Session["LANGUAGE"] = "en-us";
			Session["IsAdmin"] = user.IsAdminUser;
			Session["TutorialMode"] = user.TutorialMode;
			Session["TutorialStep"] = user.TutorialStep;

			Session.Timeout = 1440;
		}

		public void CreateAuthTicket(User user)
		{
			var authTicket = new FormsAuthenticationTicket(1, user.Id.ToString(), DateTime.Now,
														  DateTime.Now.AddMinutes(1440), true, "");

			string cookieContents = FormsAuthentication.Encrypt(authTicket);
			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieContents)
			{
				Expires = authTicket.Expiration,
				Path = FormsAuthentication.FormsCookiePath
			};
			Response.Cookies.Add(cookie);
		}
	}
}