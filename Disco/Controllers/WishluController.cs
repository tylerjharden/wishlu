using Disco.ViewModels;
using Squid.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Disco.Controllers
{    
    public class WishluController : BaseController
    {        
        [Authorize]
        public ActionResult Index()
        {            
            return View("Index");
        }
                
        [Authorize]
        public ActionResult IndexOther(Guid id)
        {
            if (id == GetCurrentUserId())
                return RedirectToAction("index");

            Squid.Users.User model = Squid.Users.User.GetUserById(id);
            
            return View("IndexOther", model);
        }
                
        [Authorize]
        public ActionResult View(Guid id, int sort = 0)
        {
            Squid.Wishes.Wishlu wishLu = Squid.Wishes.Wishlu.GetWishLuById(id);
            List<Squid.Wishes.Wish> model;

            model = wishLu.GetWishes();
            ViewBag.WishCount = model.Count;
            ViewBag.WishLu = wishLu;

            if (wishLu.UserId != GetCurrentUserId())                      
                return View("ViewOther", model);
            
            if (sort == 0)
                sort = wishLu.Sort;

            if (sort != wishLu.Sort && sort > 0 && sort <= 8)
            {
                wishLu.Sort = sort;
                wishLu.Set("Sort", sort);
            }
                        
            ViewBag.EventDate = wishLu.EventDateTime == null ? "" : wishLu.EventDateTime.Value.ToString("M.d.yyyy");
            
            switch (sort)
            {
                case 1:
                    model = model.OrderBy(x => x.Name).ToList();
                    break;

                case 2:
                    model = model.OrderByDescending(x => x.Name).ToList();
                    break;

                case 3:
                    model = model.OrderByDescending(x => x.Price).ToList();
                    break;

                case 4:
                    model = model.OrderBy(x => x.Price).ToList();
                    break;

                case 5:
                    model = model.OrderByDescending(x => x.Rating).ToList();                    
                    break;

                case 6:
                    model = model.OrderBy(x => x.Rating).ToList();                    
                    break;

                case 7:
                default:
                    model = model.OrderByDescending(x => x.CreatedOn).ToList();
                    break;
                                    
                case 8:
                    model = model.OrderBy(x => x.CreatedOn).ToList();
                    break;    
            }
            
            return View("View", model);
        }
                        
        [Authorize]
        [HttpPost]
        public ActionResult Create(CreateWishluModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Name == null || model.Name == "" || model.Name == "null")
                    return Json(new { result = false, message = "Please provide a name for your new wishlu." });
                
                string coltmp = model.Color;
                if (coltmp == "" || coltmp == null || coltmp == String.Empty)
                    return Json(new { result = false, message = "Please select a color to identify your new wishlu." });

                if ((model.Wishloops == null || model.Wishloops.Count < 1) && model.Visibility == Squid.Wishes.WishluVisibility.Friends)
                    return Json(new { result = false, message = "Please select at least one wishloop to share this wishlu with." });
                
                Squid.Wishes.Wishlu wishlu = null;
                string wishloops = "";
                try
                {
                    wishlu = new Squid.Wishes.Wishlu();

                    wishlu.Name = model.Name;
                    wishlu.Id = Guid.NewGuid();
                    wishlu.UserId = GetCurrentUserId();
                    wishlu.Description = "";                    
                    wishlu.DisplayColor = Int32.Parse(coltmp.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
                    wishlu.WishLuType = Squid.Wishes.WishluType.UserDefined;
                    wishlu.Visibility = model.Visibility;

                    // no event / date
                    if (String.IsNullOrEmpty(model.EventDate) || model.EventDate == "select a date" || model.EventDate == "01/01/0001")
                    {
                        wishlu.EventDateTime = null;                        
                    }                    
                    else
                    {
                        try
                        {
                            wishlu.EventDateTime = DateTimeOffset.Parse(model.EventDate);
                        }
                        catch
                        {
                            wishlu.EventDateTime = null;
                        }
                    }

                    wishlu.Create();

                    if (model.Visibility == Squid.Wishes.WishluVisibility.Friends)
                    {
                        try
                        {
                            if (model.Wishloops != null && model.Wishloops.Count > 0)
                                foreach (Guid value in model.Wishloops)
                                {
                                    wishlu.AddSubscriber(value);
                                    wishloops = wishloops + value + ",";
                                }
                            else
                                wishlu.AddSubscriber(Squid.Wishes.Wishloop.GetAllFriendsWishloopByUserId(GetCurrentUserId()).Id);

                            if (wishloops.Length > 0)
                                wishloops = wishloops.Remove(wishloops.Length - 1, 1);
                        }
                        catch
                        {
                            return Json(new { result = false, message = "Your wishlu has been created, but there was an error assigning it to the selected wishloops." });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return Json(new { result = false, message = "There was an error creating your wishlu." });
                }
                                                            
                return Json(new { result = true, message = "Your new wishlu was created successfully.", id = wishlu.Id, name = wishlu.Name, wishloops = wishloops, date = model.EventDate, visibility = wishlu.Visibility.ToString() }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Update(UpdateWishluModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Name == null || model.Name == "" || model.Name == "null")
                    return Json(new { result = false, message = "Please provide a name for your wishlu." });

                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please select a valid wishlu to update." });

                if ((model.Wishloops == null || model.Wishloops.Count < 1) && model.Visibility == Squid.Wishes.WishluVisibility.Friends)
                    return Json(new { result = false, message = "Please select at least one wishloop to share this wishlu with." });

                Squid.Wishes.Wishlu wishlu = null;                
                try
                {
                    wishlu = Squid.Wishes.Wishlu.GetWishLuById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "The selected wishlu is invalid." });
                }

                if (wishlu.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishlus that belong to you." });

                string coltmp = model.Color;
                if (coltmp == "" || coltmp == null || coltmp == String.Empty)
                    return Json(new { result = false, message = "Please select a color to identify your wishlu." });

                try
                {
                    wishlu.DisplayColor = Int32.Parse(coltmp.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);

                    // can only change name / date on user defined wishlus
                    if (wishlu.WishLuType == Squid.Wishes.WishluType.UserDefined)
                    {
                        wishlu.Name = model.Name;

                        // in the event that the wishlu already had an event, remove it if the user wanted to
                        if (String.IsNullOrEmpty(model.EventDate) || model.EventDate == "select a date" || model.EventDate == "01/01/0001")
                        {
                            wishlu.EventDateTime = null;                            
                        }                        
                        else
                        {
                            try
                            {
                                wishlu.EventDateTime = DateTimeOffset.Parse(model.EventDate);
                            }
                            catch
                            {
                                wishlu.EventDateTime = null;
                            }
                        }
                    }

                    // birthday wishlu and user defined wishlus can have their visibility changed
                    if (wishlu.WishLuType != Squid.Wishes.WishluType.JustMe)
                    {
                        wishlu.Visibility = model.Visibility;
                    }

                    wishlu.Update();
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error updating your wishlu." });
                }

                string wishloops = "";
                try
                {
                    wishlu.RemoveAllSubscribers();

                    if (model.Visibility == Squid.Wishes.WishluVisibility.Friends)
                    {
                        if (model.Wishloops != null && model.Wishloops.Count > 0)
                        {
                            foreach (Guid value in model.Wishloops)
                            {
                                wishlu.AddSubscriber(value);
                                wishloops = wishloops + value + ",";
                            }
                        }
                        else
                            wishlu.AddSubscriber(Squid.Wishes.Wishloop.GetAllFriendsWishloopByUserId(GetCurrentUserId()).Id);

                        if (wishloops.Length > 0)
                            wishloops = wishloops.Remove(wishloops.Length - 1, 1);
                    }
                }
                catch
                {
                    return Json(new { result = false, message = "Your wishlu has been updated, but there was an error assigning it to the selected wishloops." });
                }

                return Json(new { result = true, message = "Your wishlu was updated successfully.", id = wishlu.Id, name = wishlu.Name, wishloops = wishloops, date = model.EventDate, visibility = wishlu.Visibility.ToString() }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Name(WishluNameModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide a valid wishlu ID." });

                if (String.IsNullOrEmpty(model.Name))
                    return Json(new { result = false, message = "Please provide a valid name." });

                Squid.Wishes.Wishlu wishlu = Squid.Wishes.Wishlu.GetWishLuById(model.Id);

                if (wishlu.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishlus that belong to you." });

                try
                {
                    if (wishlu.WishLuType == Squid.Wishes.WishluType.UserDefined)
                    {
                        wishlu.Name = model.Name;
                        wishlu.Update();
                    }
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error setting the name you specified." });
                }

                return Json(new { result = true, message = "The name has been set successfully.", name = wishlu.Name });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Color(WishluColorModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide a valid wishlu ID." });

                if (String.IsNullOrEmpty(model.Color))
                    return Json(new { result = false, message = "Please select a valid color." });

                Squid.Wishes.Wishlu wishlu = Squid.Wishes.Wishlu.GetWishLuById(model.Id);

                if (wishlu.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishlus that belong to you." });

                try
                {
                    wishlu.DisplayColor = Int32.Parse(model.Color.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
                    wishlu.Update();
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error setting the color you specified." });
                }

                return Json(new { result = true, message = "The color has been set successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Event(WishluEventModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide a valid wishlu ID." });
                                
                Squid.Wishes.Wishlu wishlu = Squid.Wishes.Wishlu.GetWishLuById(model.Id);

                if (wishlu.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishlus that belong to you." });

                if (wishlu.WishLuType != Squid.Wishes.WishluType.UserDefined)
                    return Json(new { result = false, message = "You can not set a date / event on systematically generated wishlus." });

                try
                {
                    // in the event that the wishlu already had an event, remove it if the user wanted to
                    if (String.IsNullOrEmpty(model.EventDate))
                    {
                        wishlu.EventDateTime = null;                        
                    }                    
                    else
                    {
                        try
                        {
                            wishlu.EventDateTime = DateTimeOffset.Parse(model.EventDate);
                        }
                        catch
                        {
                            return Json(new { result = false, message = "The event date you provided was not in a valid format." });
                        }
                    }

                    wishlu.Update();
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error setting the date / event you specified." });
                }

                return Json(new { result = true, message = "The event / date has been set successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Visibility(WishluVisibilityModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide a valid wishlu ID." });
               
                Squid.Wishes.Wishlu wishlu = Squid.Wishes.Wishlu.GetWishLuById(model.Id);

                if (wishlu.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishlus that belong to you." });

                if (wishlu.WishLuType == Squid.Wishes.WishluType.JustMe)
                    return Json(new { result = false, message = "You cannot adjust visibility on systematically generated wishlus." });

                try
                {
                    wishlu.Visibility = model.Visibility;
                    wishlu.Update();

                    if (model.Visibility != Squid.Wishes.WishluVisibility.Friends)
                    {
                        wishlu.RemoveAllSubscribers();
                        wishlu.AddSubscriber(Squid.Wishes.Wishloop.GetAllFriendsWishloopByUserId(GetCurrentUserId()).Id);
                    }
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error setting the visibility you specified." });
                }

                return Json(new { result = true, message = "The visibility has been set successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }
        
        [Authorize]        
        [HttpPost]
        public ActionResult Assign(AssignWishluModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide a valid wishlu ID." });

                Squid.Wishes.Wishlu wishlu = null;

                try { wishlu = Squid.Wishes.Wishlu.GetWishLuById(model.Id); }
                catch
                {
                    return Json(new { result = false, message = "There was an error retrieving the wishlu you specified, it may not exist." });
                }
                if (wishlu.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishlus that belong to you." });
                                
                if (!wishlu.RemoveAllSubscribers() || !wishlu.DeletePrivateWishloop())
                    return Json(new { result = false, message = "An error occurred while removing the existing wishloops." });

                try
                {
                    if (model.Wishloops != null && model.Wishloops.Count > 0)
                        wishlu.AddSubscribers(model.Wishloops);
                    else
                        return Json(new { result = true, message = "The wishlu has been unassigned from all wishloops successfully." });
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error assigning your wishlu to the selected wishloops." });
                }
                
                return Json(new { result = true, message = "The wishlu has been assigned to the specified wishloops successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public
        ActionResult Friends(WishluFriendsModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide a valid wishlu ID." });

                Squid.Wishes.Wishlu wishlu = null;

                try { wishlu = Squid.Wishes.Wishlu.GetWishLuById(model.Id); }
                catch
                {
                    return Json(new { result = false, message = "There was an error retrieving the wishlu you specified, it may not exist." });
                }
                if (wishlu.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishlus that belong to you." });

                if (!wishlu.RemoveAllSubscribers())
                    return Json(new { result = false, message = "An error occurred while removing existing wishloop assignments." });

                try
                {
                    wishlu.UsePrivateWishloop();

                    if (model.Friends != null && model.Friends.Count > 0)
                        wishlu.GetPrivateWishloop().AddMembers(model.Friends);
                    else
                        return Json(new { result = true, message = "The wishlu has been unassigned from all wishloops / friends successfully." });
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error assigning your wishlu to the selected friends." });
                }

                return Json(new { result = true, message = "Your wishlu has been assigned to the specified friends successfully." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }
                                                
        [Authorize]
        [HttpPost]
        public ActionResult Delete(DeleteWishluModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "The selected wishlu is invalid." });

                Squid.Wishes.Wishlu wishlu = null;
                try
                {
                    wishlu = Squid.Wishes.Wishlu.GetWishLuById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "Please select a valid wishlu to delete." });
                }

                if (wishlu.WishLuType == Squid.Wishes.WishluType.JustMe)
                {
                    return Json(new { result = false, message = "The wishlu you selected is systematically generated and can not be deleted." });
                }

                if (wishlu.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only delete wishlus that belong to you." });

                wishlu.DeleteWishLu(Squid.Wishes.DeleteWishluOptions.DoNotDeleteWishes);

                return Json(new { result = true, message = "Your wishlu has been successfully deleted. Its wishes have been moved to your just me wishlu." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }
        
        [Authorize]
        [HttpPost]
        public ActionResult Follow(Guid id)
        {
            if (id == null || id == Guid.Empty)
                return JsonResponse(false, "The specified wishlu ID is invalid.");

            try
            {
                Squid.Wishes.Wishlu lu = Squid.Wishes.Wishlu.GetWishLuById(id);

                if (lu.UserId == GetCurrentUserId())
                    return JsonResponse(false, "You may not follow your own wishlus.");

                if (lu.Visibility == Squid.Wishes.WishluVisibility.Private)
                    return JsonResponse(false, "You may not follow private wishlus.");

                if (lu.HasFollower(GetCurrentUserId()))
                    return JsonResponse(false, "You are already following the specified wishlu.");

                if (lu.AddFollower(GetCurrentUserId()))
                    return JsonResponse(true, "You have followed the specified wishlu successfully.");
                else
                    return JsonResponse(false, "You are not able to follow the specified wishlu.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return JsonResponse(false, "An unexpected error occurred while attempting to follow the specified wishlu. We have been notified.");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Unfollow(Guid id)
        {
            if (id == null || id == Guid.Empty)
                return JsonResponse(false, "The specified wishlu ID is invalid.");

            try
            {
                Squid.Wishes.Wishlu lu = Squid.Wishes.Wishlu.GetWishLuById(id);
                                
                if (!lu.HasFollower(GetCurrentUserId()))
                    return JsonResponse(false, "You are not following the specified wishlu.");

                if (lu.RemoveFollower(GetCurrentUserId()))
                    return JsonResponse(true, "You have unfollowed the specified wishlu successfully.");
                else
                    return JsonResponse(false, "You are not able to unfollow the specified wishlu.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return JsonResponse(false, "An unexpected error occurred while attempting to unfollow the specified wishlu. We have been notified.");
            }
        }
    }

    public class CreateWishluModel
    {
        public List<Guid> Wishloops { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string EventDate { get; set; }        
        public Squid.Wishes.WishluVisibility Visibility { get; set; }
    }

    public class UpdateWishluModel
    {
        public Guid Id { get; set; }
        public List<Guid> Wishloops { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string EventDate { get; set; }        
        public Squid.Wishes.WishluVisibility Visibility { get; set; }
    }
    
    [Serializable]
    public class WishluNameModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    public class WishluColorModel
    {
        public Guid Id { get; set; }
        public string Color { get; set; }
    }

    [Serializable]
    public class WishluEventModel
    {
        public Guid Id { get; set; }
        public string EventDate { get; set; }        
    }

    [Serializable]
    public class WishluVisibilityModel
    {
        public Guid Id { get; set; }
        public Squid.Wishes.WishluVisibility Visibility { get; set; }
    }

    [Serializable]
    public class AssignWishluModel
    {
        public Guid Id { get; set; }
        public List<Guid> Wishloops { get; set;}
    }

    [Serializable]
    public class WishluFriendsModel
    {
        public Guid Id { get; set; }
        public List<Guid> Friends { get; set; }
    }

    [Serializable]
    public class DeleteWishluModel
    {
        public Guid Id { get; set; }
    }
}