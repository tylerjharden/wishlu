using Squid.Log;
using System;
using System.Web.Mvc;

namespace Disco.Controllers
{
    [AllowAnonymous]
    public class PasswordController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("index", "dash");
            else
                return RedirectToAction("index", "home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Forgot()
        {
            return View("Forgot");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Forgot(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.Email))
                    return Json(new { result = false, message = "Please specify a valid e-mail address to reset your password." });

                if (!Squid.Users.User.LoginIdExists(model.Email))
                    return Json(new { result = false, message = "The specified e-mail address does not belong to any registered users." });

                try
                {
                    Squid.Users.User.InitiatePasswordReset(model.Email);

                    return Json(new { result = true, message = "Password reset notification was sent successfully." });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return Json(new { result = false, message = "An unexpected error occurred while attempting to send you a password reset link." });
                }

            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }            
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Reset(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (String.IsNullOrEmpty(model.Password))
                        return Json(new { result = false, message = "Please specify a new password." });

                    if (String.IsNullOrEmpty(model.PasswordRepeat))
                        return Json(new { result = false, message = "Please confirm your new password." });

                    if (model.Password != model.PasswordRepeat)
                        return Json(new { result = false, message = "Your passwords must match." });

                    if (model.Password.Length < 6)
                        return Json(new { result = false, message = "Your password must be atleast 6 characters long." });

                    if (model.UserId == null || model.UserId == Guid.Empty)                        
                        return Json(new { result = false, message = "Please specify a valid user ID." });

                    if (model.UserId == null || model.UserId == Guid.Empty)
                        return Json(new { result = false, message = "Please specify a password reset token." });
                                        
                    Squid.Users.User.ResetPassword(model.UserId, model.Token, model.Password, model.PasswordRepeat);
                    return Json(new { result = true, message = "Your password has been changed successfully. You will now be redirected to sign in." });
                    
                }
                catch (ApplicationException ex)
                {
                    return Json(new { result = false, message = ex.Message });
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Reset()
        {
            string[] keys = Request.QueryString["token"].Split('!');
            ViewBag.UserID = keys[0];
            ViewBag.Token = keys[1];
            return View("Reset");
        }
    }

    [Serializable]
    public class ForgotPasswordModel
    {
        public string Email { get; set; }
    }

    [Serializable]
    public class ResetPasswordModel
    {
        public Guid UserId { get; set; }
        public Guid Token { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
    }
}