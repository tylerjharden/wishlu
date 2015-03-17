using ActionMailerNext.Standalone;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Squid.Users;
using Squid.Wishes;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Squid.Mail
{
    public class MailController : RazorMailerBase
    {
        private readonly string _viewPath;
        public override string ViewPath
        {
            get { return _viewPath; }
        }
        
        public MailController()
        {            
            // view templates path
            if (HostingEnvironment.IsHosted)
            {
                var workingDirectory = HostingEnvironment.MapPath("~");
                _viewPath = Path.Combine(workingDirectory, "bin", "Templates");
            }
            else
            {
                _viewPath = Path.Combine("C:\\", "templates");
            }
                        
            // razor engine initialization
            TemplateBaseType = typeof(RazorEngine.Templating.HtmlTemplateBase<User>);

            var config = new TemplateServiceConfiguration
            {
                BaseTemplateType = typeof(HtmlTemplateBase<>)
            };
            Razor.SetTemplateService(new TemplateService(config));

            // SMTP email sender
            SmtpClient smtp = new SmtpClient("smtpout.secureserver.net");            
            NetworkCredential cred = new NetworkCredential("no-reply@wishlu.com", "ballsOfSteel#34");
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = cred;

            // Sending mail via SMTP, this may change to Mandrill in the future
            MailSender = new ActionMailerNext.Implementations.SMTP.SmtpMailSender(smtp);            
        }

        // Template Email prompting user to confirm their e-mail address (this is the first e-mail wishlu ever sends a new user)
        public RazorEmailResult ConfirmEmail(User model)
        {            
            // mail properties
            MailAttributes.From = new MailAddress("no-reply@wishlu.com", "wishlu");
            MailAttributes.To.Add(new MailAddress(model.LoginId));
            MailAttributes.Subject = "Please confirm your email.";
            MailAttributes.Priority = MailPriority.High;
                        
            return Email<User>("ConfirmEmail", model);
        }

        // Template Email for an invitation to join wishlu
        public RazorEmailResult InviteEmail(User user, string email, string code)
        {
            // mail properties
            MailAttributes.From = new MailAddress("no-reply@wishlu.com", "wishlu");
            MailAttributes.To.Add(new MailAddress(email));
            MailAttributes.Subject = user.FullName + " has invited you to join wishlu."; ;
            MailAttributes.Priority = MailPriority.High;

            dynamic model = new ExpandoObject();
            model.User = user;
            model.Code = code;

            return Email<dynamic>("InviteEmail", model);            
        }

        // Template Email corresponding to the Golden Ratio of Days Notifications
        public RazorEmailResult NewUserEmail(User model, int email)
        {
            // mail properties
            MailAttributes.From = new MailAddress("no-reply@wishlu.com", "wishlu");
            MailAttributes.To.Add(new MailAddress(model.LoginId));            
            MailAttributes.Priority = MailPriority.High;

            switch (email)
            {
                default:
                case 1: // Day 1
                    MailAttributes.Subject = "Welcome to wishlu.";
                    return Email<User>("Day1", model);

                case 2: // Day 2
                    MailAttributes.Subject = "A few smart ways to use wishlu.";
                    return Email<User>("Day2", model);

                case 3: // Day 3
                    MailAttributes.Subject = "The secret to buying the most perfect present for anyone.";
                    return Email<User>("Day3", model);

                case 5: // Day 5
                    MailAttributes.Subject = "The new way to give (and to get).";
                    return Email<User>("Day5", model);

                case 8: // Day 8
                    MailAttributes.Subject = "gifting & wishloops 101";
                    return Email<User>("Day8", model);
                                   
                case 13: // Day 13
                    MailAttributes.Subject = "Get gifted.";
                    return Email<User>("Day13", model);
            }
        }

        // Template of the email sent when a gift is revealed
        public RazorEmailResult RevealEmail(Gift model)
        {
            // mail properties
            MailAttributes.From = new MailAddress("no-reply@wishlu.com", "wishlu");
            MailAttributes.To.Add(new MailAddress(model.GetReceiver().LoginId));
            MailAttributes.Priority = MailPriority.High;
            MailAttributes.Subject = model.GetGiver().FullName + " has gifted you one of your items!";

            return Email<Gift>("RevealEmail", model);
        }

        // Template Email sent when a user requests to reset their password
        public RazorEmailResult PasswordResetEmail(User model)
        {
            // mail properties
            MailAttributes.From = new MailAddress("no-reply@wishlu.com", "wishlu");
            MailAttributes.To.Add(new MailAddress(model.LoginId));
            MailAttributes.Priority = MailPriority.High;
            MailAttributes.Subject = "wishlu - Password Reset Notification";

            return Email<User>("PasswordReset", model);
        }

        // Internal Template email sent when a logged exception occurs in the code
        public RazorEmailResult ErrorEmail(Exception model)
        {
            // mail properties
            MailAttributes.From = new MailAddress("no-reply@wishlu.com", "wishlu - error occurred");
            MailAttributes.To.Add(new MailAddress("log@wishlu.com"));
            MailAttributes.Subject = DateTime.Now.ToString("(MM/dd/yyyy hh:mm:ss tt)") + " Exception: " + model.Message;
            MailAttributes.Priority = MailPriority.High;

            return Email<Exception>("ErrorEmail", model);
        }
    }
}