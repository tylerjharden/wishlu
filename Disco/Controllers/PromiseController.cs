using System.Web.Mvc;

namespace Disco.Controllers
{
    //================================================================================================//
    public
    class PromiseController : BaseController
    {
        [Authorize]
        public
        ActionResult
        Index()
        {            
            return View();
        }

        [Authorize]
        public
        ActionResult
        Create()
        {            
            return View();
        }

        [Authorize]
        public
        ActionResult
        Cancel()
        {            
            return View();
        }

        [Authorize]
        public
        ActionResult
        Confirm()
        {            
            return View();
        }         
        //---------------------------------------------------------------------------------------------//
    }   
    //================================================================================================//
}
