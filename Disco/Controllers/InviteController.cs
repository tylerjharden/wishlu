using Facebook;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;


namespace Disco.Controllers
{
    //================================================================================================//
    public
    class InviteController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            List<Squid.Users.User> here = new List<Squid.Users.User>();
            //List<dynamic> missing = new List<dynamic>();

            var user = GetCurrentUser();

            if (user.IsFacebookSynced)
            {
                var accessToken = user.FacebookAccessToken;
                var client = new FacebookClient(accessToken);

                dynamic result = client.Get("me", new { fields = "friends" });

                List<String> ids = new List<string>();
                foreach (dynamic friend in result.friends.data)
                {
                    ids.Add(friend.id);
                }

                here.AddRange(Squid.Users.User.GetFacebookUsers(ids));

                here.RemoveAll(x => x.IsFriend(user.Id));
                //here.RemoveAll(x => x.FriendRequestExists(user.Id));

                /*foreach (dynamic friend in result.friends.data)
                {
                    string id = friend.id;

                    if (here.Exists(x => x.FacebookAccessToken == id))
                        continue;

                    missing.Add(friend);
                }*/
            }

            dynamic res = new ExpandoObject();
            res.here = here;
            //res.missing = missing;
            res.user = GetCurrentUser();

            return View("Index", res);
        }
                
        [Authorize]
        [HttpPost]
        public ActionResult Invite(InviteUsersModel model)
        {
            if (ModelState.IsValid)
            {
                if (model == null || String.IsNullOrEmpty(model.Emails))
                    return JsonResponse(false, "Please specify atleast one person's e-mail address to invite.");

                string inv = model.Emails;

                Squid.Users.User user = GetCurrentUser();

                //if (Regex.IsMatch(inv.ToUpper(), "^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                //{               
                //    // One e-mail address
                //    user.Invite(inv);
                //}
                //else
                //{
                List<string> emails = inv.Split(',').ToList();

                if (emails.Count == 0)
                    emails.Add(inv);

                bool foundUsers = false;

                foreach (string email in emails)
                {
                    string em = email.Trim();

                    if (!Regex.IsMatch(em, "^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}$", RegexOptions.IgnoreCase))
                        continue;

                    if (user.LoginId == email)
                        return JsonResponse(false, "You cannot send yourself an invite to wishlu. You're already here.");

                    if (Squid.Users.User.LoginIdExists(email))
                    {
                        var friend = Squid.Users.User.GetUserByLoginId(email);

                        if (user.IsFriend(friend.Id))
                            return JsonResponse(false, friend.FullName + "(" + friend.LoginId + ") is already your friend.");

                        foundUsers = true;
                        user.AddFriend(friend.Id);
                    }
                    else {
                        // Process e-mail
                        user.Invite(em);
                    }
                }
                //}

                if (foundUsers)
                    return JsonResponse(true, "One or more of the people you attempted to invite are already on wishlu. We sent friend requests on your behalf to those users. The remaining users were invited via email.");
                else if (emails.Count == 1)
                    return JsonResponse(true, "Your invite has been sent successfully.");
                else
                    return JsonResponse(true, emails.Count + " invites have been sent successfully.");                
            }
            else
            {
                return JsonResponse(false, "The server received an invalid model.");
            }            
        }              
    }

    [Serializable]
    public class InviteUsersModel
    {
        public string Emails { get; set; }
    }
}
