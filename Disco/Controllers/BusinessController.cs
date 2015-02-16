using System.Web.Mvc;

namespace Disco.Controllers
{
public
class BusinessController : BaseController
{    
    //---------------------------------------------------------------------------------------------//
    [AllowAnonymous]
   public
   ActionResult
   Index()
   {       
       RedirectToAction("Create");

      return View();
   }	 
	//---------------------------------------------------------------------------------------------//
    [AllowAnonymous]
    public ActionResult
    Create()
    {
        return View("Create");
    }
    //---------------------------------------------------------------------------------------------//
   
}
//================================================================================================//
}

