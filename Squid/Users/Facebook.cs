using Facebook;

namespace Squid.Users
{
    public class FacebookManager
    {
        public static FacebookClient Client;

        public static string AppId { get { return "296645670486904"; } }
        public static string AppAccessToken { get { return "296645670486904|KK_Jly0DUrHVxOXYaqOfQw_AT4A"; } }

        static FacebookManager()
        {
            Client = new FacebookClient();
            Client.AppId = "296645670486904";
            Client.AppSecret = "d03805fbc429d0a9bc47f63ad6dade74";
            Client.AccessToken = "296645670486904|KK_Jly0DUrHVxOXYaqOfQw_AT4A";
        }
        
        public static string GetAccessToken()
        {
            dynamic result = Client.Get("oauth/access_token", new
            {
                client_id = Client.AppId,
                client_secret = Client.AppSecret,
                grant_type = "client_credentials"
            });

            return result.access_token;
        }
                
        public static string GetOauthToken(string code, string redirecturl)
        {
            //Dictionary<string, string> tokens = new Dictionary<string, string>();

            //string clientId = "296645670486904";
            //string redirectUrl = "http://localhost:3000/Facebook/SignIn";
            //string clientSecret = "d03805fbc429d0a9bc47f63ad6dade74";
            //string scope = "read_friendlists,user_status,email,offline_access,photo_upload,publish_actions,publish_checkins,publish_stream,read_friendlists,read_insights,read_mailbox";
                        
            JsonObject result = (JsonObject)Client.Get("oauth/access_token", new
            {
                client_id = Client.AppId,
                client_secret = Client.AppSecret,
                redirect_uri = redirecturl,
                code = code//,
                //scope = scope
            });

            //string url = string.Format("https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&scope={4}",
            //                clientId, redirectUrl, clientSecret, code, scope);
            
            //HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            //using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            //{
            //    StreamReader reader = new StreamReader(response.GetResponseStream());

            //    string retVal = reader.ReadToEnd();
                
            //    foreach (string token in retVal.Split('&'))
            //    {
            //        tokens.Add(token.Substring(0, token.IndexOf("=")),
            //            token.Substring(token.IndexOf("=") + 1, token.Length - token.IndexOf("=") - 1));
            //    }
            //}

            Client.AccessToken = result["access_token"].ToString(); //tokens["access_token"];

            return result["access_token"].ToString(); // tokens;
        }

        // exchange short-lived access token for an extended token (up to 60 days)
        public static string GetExtendedAccessToken(string shortToken)
        {
            string extendedToken = "";
            //string scope = "read_friendlists,user_status,email,offline_access,photo_upload,publish_actions,publish_checkins,publish_stream,read_friendlists,read_insights,read_mailbox";

            try
            {
                dynamic result = Client.Get("/oauth/access_token", new
                {
                    grant_type = "fb_exchange_token",
                    client_id = "296645670486904",
                    client_secret = "d03805fbc429d0a9bc47f63ad6dade74",
                    fb_exchange_token = shortToken//,
                    //scope = scope
                });
                extendedToken = result.access_token;
            }
            catch
            {
                extendedToken = shortToken;
            }
            return extendedToken;
        }

        public void OGPost()
        {
            
        }
    }
}
