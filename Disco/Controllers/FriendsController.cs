using Disco.ViewModels;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Controllers
{
    [Authorize]
    public class FriendsController : BaseController
    {        
        [Authorize]
        public ActionResult Index()
        {
            List<Squid.Users.User> friends = Squid.Users.User.GetUsersFriends(GetCurrentUserId());
                        
            return View("Index", friends);
        }
                
        [Authorize]
        public ActionResult Requests()
        {
            List<User> requests = GetCurrentUser().GetFriendRequestUsers();

            return View("Requests", requests);
        }

        //---------------------------------------------------------------------------------------------//
        // This method accepts a friend request, deletes it, and creates a friendship between the two users
        [Authorize]
        public ActionResult Accept(Guid id)
        {
            if (Request.IsAjaxRequest())
            {
                if (!Squid.Users.User.UserExists(id))
                    return JsonResponse(false, "The specified user does not exist.");

                if (GetCurrentUser().FriendRequestExists(id))
                {
                    if (GetCurrentUser().AcceptFriendRequest(id))
                    {
                        string name = Squid.Users.User.GetUserFullName(id);

                        return JsonResponse(true, "You and " + name + " are now friends.");
                    }
                    else
                        return JsonResponse(false, "Sorry, we were unable to create a friendship between you.");
                }
                else
                {
                    return JsonResponse(false, "The specified friend request no longer exists.");
                }
            }
            else
            {
                if (GetCurrentUser().AcceptFriendRequest(id))
                {
                    string name = Squid.Users.User.GetUserFullName(id);

                    TempData["SuccessMessage"] = "You and " + name + " are now friends.";
                }
                else
                {
                    TempData["ErrorMessage"] = "There was an error accepting the friend request.";
                }

                List<User> requests = GetCurrentUser().GetFriendRequestUsers();

                if (requests.Count > 0)
                {
                    return View("Requests", requests);
                }
                else
                {
                    return View("index", Squid.Users.User.GetUsersFriends(GetCurrentUserId()));
                }                
            }
        }

        //---------------------------------------------------------------------------------------------//
        // This method declines and then deletes a friendship request
        [Authorize]
        public ActionResult Decline(Guid id)
        {
            if (Request.IsAjaxRequest())
            {
                if (GetCurrentUser().DeleteFriendRequest(id))
                    return JsonResponse(true, "Friendship request declined successfully.");
                else
                    return JsonResponse(false, "Unable to decline the friendship request. It may have already been declined.");
            }
            else
            {
                if (GetCurrentUser().DeleteFriendRequest(id))
                {
                    string name = Squid.Users.User.GetUserFullName(id);

                    TempData["SuccessMessage"] = "You have deleted " + name + "'s friend request successfully.";
                }

                List<User> requests = GetCurrentUser().GetFriendRequestUsers();

                if (requests.Count > 0)
                {
                    return View("Requests", requests);
                }
                else
                {
                    return View("index", Squid.Users.User.GetUsersFriends(GetCurrentUserId()));
                }
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Suggest(SuggestFriendsModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select a friend to suggest potential friends to.");

                if (model.Friends == null || model.Friends.Count == 0)
                    return JsonResponse(false, "Please select at least one friend to suggest.");

                if (!Squid.Users.User.UserExists(model.Id))
                    return JsonResponse(false, "The provided user does not exist.");

                Squid.Users.User current = GetCurrentUser();
                foreach (Guid friend in model.Friends)
                {
                    current.SuggestFriend(model.Id, friend);
                }

                return JsonResponse(true, "You have suggested " + model.Friends.Count + " friends.");
            }
            else
            {
                return JsonResponse(false, "The server received an invalid model.");
            }
        }

        //---------------------------------------------------------------------------------------------//
        // This method performs a tokenized query of the user database and returns users who match the given search criteria
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Search(FormCollection formCollection)
        {           
            Squid.Users.User wlUser = GetCurrentUser();            
            List<Squid.Users.User> results = new List<Squid.Users.User>();
                        
            if (formCollection["searchquery"] == String.Empty)
                return View("Index", Squid.Users.User.GetUsersFriends(GetCurrentUserId()));
                        
            return View("Search", results);
        }
        
        [Authorize]
        public ActionResult Add(Guid id)
        {
            User user = GetCurrentUser();
            User friend;

            try
            {
                friend = Squid.Users.User.GetUserById(id);
            }
            catch
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(false, "The specified user could not be found.");

                TempData["ErrorMessage"] = "The specified user could not be found.";
                return RedirectToAction("index");
            }

            // redirect if they are already friends
            if (user.IsFriend(id))
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(false, "You are already friends with this user.");

                TempData["ErrorMessage"] = "You are already friends with this user.";
                return RedirectToAction("index");
            }

            if (user.FriendRequestExists(id))
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(false, "A friendship request already exists for this user.");

                TempData["ErrorMessage"] = "A friendship request already exists for this user.";
                return RedirectToAction("index");
            }

            // privacy
            switch (friend.FriendRequestPermission)
            {
                case Squid.Users.UserPrivacy.FriendsOfFriends:
                    if (!user.IsFriendOfFriend(id))
                    {
                        if (Request.IsAjaxRequest())
                            return JsonResponse(false, "This user only accepts friend requests from friends of friends.");

                        TempData["ErrorMessage"] = "This user only accepts friend requests from friends of friends.";
                        return RedirectToAction("index");
                    }
                    break;
            }

            if (user.AddFriend(id))
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(true, "A friend request has been sent to " + friend.FullName);

                TempData["SuccessMessage"] = "A friend request has been sent to " + friend.FullName;
                return RedirectToAction("index");
            }
            else
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(false, "There was an error sending your friend request.");

                TempData["ErrorMessage"] = "There was an error sending your friend request.";
                return RedirectToAction("index");
            }            
        }
        
        [Authorize]
        public ActionResult Delete(Guid id)
        {
            Squid.Users.User user = GetCurrentUser();
            Squid.Users.User friend;

            try
            {
                friend = Squid.Users.User.GetUserById(id);
            }
            catch
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(false, "The specified user could not be found.");

                TempData["ErrorMessage"] = "The specified user could not be found.";
                return RedirectToAction("index");
            }
            
            if (user.Unfriend(id))
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(true, "You have unfriended " + friend.FullName + ".");

                TempData["SuccessMessage"] = "You have unfriended " + friend.FullName + ".";
                return RedirectToAction("index");
            }
            else if (user.CancelFriendRequest(id))
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(true, "You have deleted your friend request for " + friend.FullName + ".");
                
                TempData["SuccessMessage"] = "You have deleted your friend request for " + friend.FullName + ".";
                return RedirectToAction("index");                
            }
            else 
            {
                if (Request.IsAjaxRequest())
                    return JsonResponse(false, "There was a problem unfriending " + friend.FullName + ".");

                TempData["ErrorMessage"] = "There was a problem unfriending " + friend.FullName + ".";
                return RedirectToAction("index");
            }                        
        }
    }

    [Serializable]
    public class SuggestFriendsModel
    {
        public Guid Id { get; set; }
        public List<Guid> Friends { get; set; }
    }
}