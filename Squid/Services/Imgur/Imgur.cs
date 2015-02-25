using Squid.Log;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Squid.Services.Imgur
{
    // spec implementation of Imgur service / API for hosting our item images, profile images, etc.
    public static class Imgur
    {
        private static string client_id = "6cf4392ae517a26";
        //private static string client_secret = "66b33055c409ea94f78857b2d8a5b4b48e39f95b";
        
        public static string UploadImage(string image)
        {
            WebClient w = new WebClient();
            w.Headers.Add("Authorization", "Client-ID " + client_id);

            System.Collections.Specialized.NameValueCollection keys = new System.Collections.Specialized.NameValueCollection();

            try
            {
                keys.Add("image", Convert.ToBase64String(File.ReadAllBytes(image)));

                byte[] responseArray = w.UploadValues("https://api.imgur.com/3/image", keys);
                dynamic result = Encoding.ASCII.GetString(responseArray);
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("link\":\"(.*?)\"");
                System.Text.RegularExpressions.Match match = reg.Match(result);
                string url = match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/");
                url = url.Replace("http://", "https://");
                return url;
            }
            catch (Exception s)
            {
                Logger.Error("Imgur Exception: " + s.ToString());

                return "//assets.wishlu.com/images/DefaultWish.jpg";
            }
        }

        public static string UploadImage(byte[] image)
        {
            WebClient w = new WebClient();
            w.Headers.Add("Authorization", "Client-ID " + client_id);
            
            System.Collections.Specialized.NameValueCollection keys = new System.Collections.Specialized.NameValueCollection();

            try
            {
                keys.Add("image", Convert.ToBase64String(image));

                byte[] responseArray = w.UploadValues("https://api.imgur.com/3/image", keys);
                dynamic result = Encoding.ASCII.GetString(responseArray);
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("link\":\"(.*?)\"");
                System.Text.RegularExpressions.Match match = reg.Match(result);
                string url = match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/");
                url = url.Replace("http://", "https://");
                return url;
            }
            catch (Exception s)
            {
                Logger.Error(s);

                return "//assets.wishlu.com/images/DefaultWish.jpg";
            }
        }
    }
}
