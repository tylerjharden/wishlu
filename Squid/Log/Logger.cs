using Squid.Database;
using Squid.Mail;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;

namespace Squid.Log
{
    public enum LogType
    {
        Message,
        Error
    }

    public static class Logger
    {
        static Logger()
        {
        }

        // this utility method was grabbed from the internet (TODO: Cite source)
        public static void WriteTrace(bool enteringFunction)
        {
            string callingFunctionName = "Undetermined method";
            string action = enteringFunction ? "Entering" : "Exiting";

            try
            {
                //Determine the name of the calling function. 
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
                callingFunctionName = stackTrace.GetFrame(1).GetMethod().Name;

            }
            catch { }

            Trace.Write(action, callingFunctionName);
        }

        private static bool Log(LogType type, string message)
        {
            string final = "";

            switch (type)
            {
                case LogType.Error:
                    final = "(Error) " + message;
                    break;

                case LogType.Message:
                    final = message;
                    break;
            }

            // if the log is being written to from an authenticated session, we want that User ID
            if (GenericPrincipal.Current.Identity.Name != String.Empty)
                final = "(Context: " + System.Security.Principal.GenericPrincipal.Current.Identity.Name + ") " + final;

            try
            {
                Redis.PushLogMessage(final);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Log(string message)
        {
            Log(LogType.Message, message);
        }

        public static void Error(string message)
        {
            Log(LogType.Error, message);
        }

        // when logging an Exception, attempt to e-mail us (developers) directly with the stack trace
        public static void Error(Exception ex)
        {            
            Log(LogType.Error, "Exception: " + ex.Message + " Source: " + ex.Source + " Stack Trace: " + ex.StackTrace);

            try
            {
                new MailController().ErrorEmail(ex).Deliver();
            }
            catch (Exception e)
            {
                Log(LogType.Error, "Logger was unable to send an exception email. Exception: " + e.Message + " Source: " + e.Source + " Stack Trace: " + e.StackTrace);
            }

            if (ex.GetType() == typeof(AggregateException))
            {
                foreach (Exception e in ((AggregateException)ex).InnerExceptions)
                    Error(e);
            }
        }
    }    
}