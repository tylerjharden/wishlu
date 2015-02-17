using Milkshake;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace Disco.Controllers
{            
    public class ContactController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {            
            return View("Index");
        }

        [AllowAnonymous]
        public ActionResult Send(ContactModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Subject))
                    return JsonResponse(false, "Please provide a subject.");

                if (string.IsNullOrEmpty(model.Message))
                    return JsonResponse(false, "Please provide a message.");
                                
                var Smtp = new SmtpClient("smtpout.secureserver.net");
                NetworkCredential cred = new NetworkCredential("no-reply@wishlu.com", "ballsOfSteel#34");
                Smtp.UseDefaultCredentials = false;
                Smtp.Credentials = cred;

                string message = "wishlu Contact Form Message\n";

                if (GetCurrentUser() != null)
                {
                    message += "\nUser: " + GetCurrentUser().FullName;
                    message += "\nE-mail: " + GetCurrentUser().LoginId;
                }
                else
                {
                    message += "\nAnonymous User.";
                }

                message += "\n\n\n\n";

                message += model.Message;

                // Send e-mail                    
                MailMessage msg = new MailMessage();
                msg.Subject = "Contact Form: " + model.Subject;
                msg.Body = message;
                msg.To.Add("contact@wishlu.com");
                msg.From = new MailAddress("no-reply@wishlu.com", "wishlu", Encoding.Unicode);
                                
                Smtp.Send(msg);

                return JsonResponse(true, "Thanks for reaching out to us. We will respond as soon as possible.");
            }
            else
            {
                return JsonResponse(false, "The server received an invalid model.");
            }
        }
    }

    public class ContactModel
    {
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}