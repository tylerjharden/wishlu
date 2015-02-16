using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Squid.Database;
using Squid.Housekeeping;
using Squid.Log;
using Squid.Users;
using Squid.Wishes;

namespace Fantine
{
    public static class EmailManager
    {
        public static string SmtpServer { get; set; }
        public static string SmtpUsername { get; set; }
        public static string SmtpPassword { get; set; }
        private static SmtpClient Smtp { get; set; }

        public static List<EmailTask> Tasks { get; set; }
        private static DateTime LastFetch { get; set; }

        static EmailManager()
        {
            SmtpServer = "smtpout.secureserver.net";
            SmtpUsername = "no-reply@wishlu.com";
            SmtpPassword = "ballsOfSteel#34";

            try
            {
                Smtp = new SmtpClient(SmtpServer);
                NetworkCredential cred = new NetworkCredential(SmtpUsername, SmtpPassword);
                Smtp.UseDefaultCredentials = false;
                Smtp.Credentials = cred;
            }
            catch { }

            Tasks = new List<EmailTask>();

            try
            {
                FetchTasks();
            }
            catch { }
        }

        public static void Poll()
        {
            if (ShouldFetch())
                FetchTasks();

            lock (Tasks)
            {
                foreach (EmailTask em in Tasks)
                {
                    try
                    {
                        Logger.Log("Fantine is sending an email! (Id: " + em.Id + ")");

                        // send a basic e-mail notification
                        if (em.EmailNotification == EmailNotification.None)
                        {
                            // Send e-mail                    
                            MailMessage msg = new MailMessage();
                            msg.Subject = em.Subject;
                            msg.Body = em.Body;
                            msg.IsBodyHtml = true;
                            msg.To.Add(em.To);
                            msg.From = new MailAddress("no-reply@wishlu.com", "wishlu", Encoding.Unicode);

                            msg.Priority = MailPriority.High;

                            Smtp.Send(msg);
                        }
                        else // send an e-mail notification with a Razor template
                        {                            
                            new Squid.Mail.MailController().NewUserEmail(User.GetUserById(em.Id), (int)em.EmailNotification).Deliver();                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                    finally
                    {
                        // Delete tasks
                        Graph.Instance.Cypher
                            .Match("(task:EmailTask" + DateTime.Now.ToString("MMddyy") + ")")
                            .Where((EmailTask task) => task.Id == em.Id)
                            .Delete("task")
                            .ExecuteWithoutResultsAsync();
                    }
                }

                Tasks.Clear();
            }
        }

        public static bool ShouldFetch()
        {
            try
            {
                return (DateTime.Now - LastFetch).Seconds >= 5;
            }
            catch { return false; }
        }

        public static void FetchTasks()
        {
            lock (Tasks)
            {
                try
                {

                    List<EmailTask> results = Graph.Instance.Cypher
                        .Match("(task:EmailTask" + DateTime.Now.ToString("MMddyy") + ")")
                        .Return(task => task.As<EmailTask>())
                        .Results.ToList();

                    if (results.Count > 0)
                    {
                        Tasks.AddRange(results);
                        Logger.Log("Fantine has retrieved " + results.Count + " email tasks.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                finally { LastFetch = DateTime.Now; }
            }                                    
        }

        /*public static void SendEmail()
        {
            try
            {
                Logger.Log("Sending an email...");

                MailMessage msg = new MailMessage();
                msg.To.Add("tyler@wishlu.com");
                msg.To.Add("patrick@empirestock.com");
                msg.Subject = "Test e-mail";
                msg.From = new MailAddress("no-reply@wishlu.com");
                msg.Body = "First message sent via Fantine and her sweet ass.";

                SmtpClient smtp = new SmtpClient(SmtpServer);
                NetworkCredential cred = new NetworkCredential(SmtpUsername, SmtpPassword);

                smtp.UseDefaultCredentials = false;
                smtp.Credentials = cred;

                smtp.Send(msg);
            }
            catch { }
        }*/
    }       
}
