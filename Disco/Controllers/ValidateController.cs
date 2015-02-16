using System;
using System.Web.Mvc;

namespace Disco.Controllers
{
[AllowAnonymous]
public class ValidateController : BaseController
{   
	//---------------------------------------------------------------------------------------------//    
	[AllowAnonymous]    
	[OutputCache(Duration = 0, NoStore = true)]
	public
	JsonResult
	Available()
	{
		string email = Request.QueryString["joinEmail"];

		return Json(!Squid.Users.User.LoginIdExists(email), JsonRequestBehavior.AllowGet);		
	}

	[AllowAnonymous]
	[OutputCache(Duration = 0, NoStore = true)]
	public ActionResult Exists()
	{
		string email = Request.QueryString["EMail"];

		return Json(Squid.Users.User.LoginIdExists(email), JsonRequestBehavior.AllowGet);
	}

	[AllowAnonymous]
	[HttpPost]
	public
	ActionResult
	Reset(FormCollection formCol)
	{
		return View("Reset");
	}
 
	//---------------------------------------------------------------------------------------------//
	[AllowAnonymous]
	public
	ActionResult
	ResetPassword()
	{        
		 //ViewBag.LanguageResourceGeneric = new LanguageResource("Web.Generic", Session["LANGUAGE"].ToString());
		 //ViewBag.LanguageResourceHome = new LanguageResource("Web.Home", Session["LANGUAGE"].ToString());
		 return View("ResetPassword");
	}
	//---------------------------------------------------------------------------------------------//
	[AllowAnonymous]
	public
	ActionResult
	ProcessReset(FormCollection formCollection)
	{
		 string returnMessage = "Password reset instructions were sent to the email provided.";
		 ViewBag.SuccessMessage = returnMessage;
		 try
		 {			  
			  //if (Request.QueryString["languageid"] != null)
			  //{
				//	Session["LANGUAGE"] = Request.QueryString["languageid"];
			  //}
			  //else
			  //{
			//		Session["LANGUAGE"] = "en-us";
			  //}
			  //ViewBag.LanguageResourceGeneric = new LanguageResource("Web.Generic", Session["LANGUAGE"].ToString());
			  //ViewBag.LanguageResourceHome = new LanguageResource("Web.Home", Session["LANGUAGE"].ToString());
			  // Send Email 
			  if (formCollection["EMail"] != null)
			  {
					Squid.Users.User.InitiatePasswordReset(formCollection["EMail"]);
			  }
			  if (formCollection["Password"] != null)
			  {
					Squid.Users.User.ResetPassword(new Guid(formCollection["UserID"]), new Guid(formCollection["Token"]), formCollection["Password"], formCollection["Passwordrepeat"]);
					returnMessage = "Password has been changed.";
					ViewBag.SuccessMessage = returnMessage;
			  }
		 }
		 catch
		 {
			  returnMessage = "Username was not found.";
			  ViewBag.ErrorMessage = returnMessage;
			  ViewBag.SuccessMessage = null;
		 }
		 
		 return View("GrantWish");
	}
	//---------------------------------------------------------------------------------------------//
	[AllowAnonymous]
	public
	ActionResult
	DoReset(FormCollection formCollection)
	{
		 //if (Request.QueryString["languageid"] != null)
		 //{
		//	  Session["LANGUAGE"] = Request.QueryString["languageid"];
		 //}
		 //else
		// {
		//	  Session["LANGUAGE"] = "en-us";
		// }
		// ViewBag.LanguageResourceGeneric = new LanguageResource("Web.Generic", Session["LANGUAGE"].ToString());
		// ViewBag.LanguageResourceHome = new LanguageResource("Web.Home", Session["LANGUAGE"].ToString());
		 if (formCollection.Count > 0)
		 {
			  Squid.Users.User.InitiatePasswordReset(formCollection["EMail"]);			  
		 }
		 string[] keys = Request.QueryString["resetkey"].Split('!');
		 ViewBag.UserID = keys[0];
		 ViewBag.Token = keys[1];
		 return View("ResetPassword");
	}


	//---------------------------------------------------------------------------------------------//
   [AllowAnonymous]
	public ActionResult SignIn(string returnurl)
   {
	   //Session["LANGUAGE"] = "en-us";
	   //ViewBag.LanguageResourceGeneric = new LanguageResource("Web.Generic", Session["LANGUAGE"].ToString());
	   //ViewBag.LanguageResourceHome = new LanguageResource("Web.Home", Session["LANGUAGE"].ToString());

	   ViewBag.ReturnUrl = returnurl;

	   return View("SignIn");
   }

	[AllowAnonymous]
	public ActionResult Register(string returnurl)
	{
		return RedirectToAction("register", "join", new { @returnurl = returnurl });
	}

	[AllowAnonymous]
	public ActionResult PrivacyPolicy()
	{
		return View("PrivacyPolicy");
	}
				   
	//---------------------------------------------------------------------------------------------//	   
}
//================================================================================================//
}

