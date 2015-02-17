using Disco.ViewModels;
using Squid.Log;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace Disco.Controllers
{    
    public class WishloopController : BaseController
    {        
        [Authorize]
        public ActionResult Index()
        {
            List<Squid.Wishes.MappedWishloop> model = Squid.Wishes.Wishloop.GetUsersMappedWishloops(CurrentUser.Id);
            
            return View("Index", model);
        }
         
        [Authorize]
        public ActionResult View(Guid id)
        {            
            Squid.Wishes.Wishloop wishLoop = Squid.Wishes.Wishloop.GetWishloopById(id);

            if (wishLoop.UserId != CurrentUser.Id)
            {
                TempData["ErrorMessage"] = "You may only view and manage your own wishloops.";
                return RedirectToAction("index", "dash");
            }
                       
            var viewModel = new LoopViewModel
            {
                wishLoop = wishLoop,
                wishLoops = null,
                wishLoopMembers = wishLoop.GetMembers()
            };
            return View("View", viewModel);
        }
       
        [Authorize]
        [HttpPost]
        public ActionResult Create(CreateWishloopModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Name == null || model.Name == "" || model.Name == "null")
                    return Json(new { result = false, message = "Please provide a name for your new wishloop." });

                Squid.Wishes.Wishloop addWishloop = new Squid.Wishes.Wishloop();

                string coltmp = model.Color;
                if (coltmp == "" || coltmp == null || coltmp == String.Empty)
                    return Json(new { result = false, message = "Please select a color to identify your new wishloop." });

                try
                {
                    addWishloop.Name = model.Name;
                    addWishloop.UserId = CurrentUser.Id;
                    addWishloop.Description = "";
                    addWishloop.DisplayColor = Int32.Parse(coltmp.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
                    addWishloop.WishloopType = Squid.Wishes.WishloopType.UserDefined;
                    addWishloop.Create();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return Json(new { result = false, message = "There was an error creating your wishloop." });
                }

                string wishlus = "";
                try
                {
                    if (model.Wishlus != null && model.Wishlus.Count > 0)
                        foreach (Guid value in model.Wishlus)
                            Squid.Wishes.Wishlu.GetWishLuById(value).AddSubscriber(addWishloop.Id);

                    foreach (Guid value in model.Wishlus)
                    {
                        wishlus = wishlus + value + ",";
                    }

                    if (wishlus.Length > 0)
                        wishlus = wishlus.Remove(wishlus.Length - 1, 1);
                }
                catch
                {
                    return Json(new { result = false, message = "Your wishloop has been created, but there was an error assigning it to the selected wishlus." });
                }

                return Json(new {result=true,message="Your new wishloop was created successfully.", id = addWishloop.Id, name = addWishloop.Name, wishlus = wishlus }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }
        
        [Authorize]
        [HttpPost]
        public ActionResult Color(WishloopColorModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide a valid wishloop ID." });

                if (String.IsNullOrEmpty(model.Color))
                    return Json(new { result = false, message = "Please select a valid color." });

                Squid.Wishes.Wishloop wishloop = Squid.Wishes.Wishloop.GetWishloopById(model.Id);

                if (wishloop.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishloops that belong to you." });

                try
                {
                    wishloop.DisplayColor = Int32.Parse(model.Color.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
                    wishloop.Update();
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
        public ActionResult Name(WishloopNameModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide a valid wishloop ID." });

                if (String.IsNullOrEmpty(model.Name))
                    return Json(new { result = false, message = "Please provide a valid name." });

                Squid.Wishes.Wishloop wishloop = Squid.Wishes.Wishloop.GetWishloopById(model.Id);

                if (wishloop.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishloops that belong to you." });

                try
                {
                    if (wishloop.WishloopType == Squid.Wishes.WishloopType.UserDefined)
                    {
                        wishloop.Name = model.Name;
                        wishloop.Update();
                    }
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error setting the name you specified." });
                }

                return Json(new { result = true, message = "The name has been set successfully.", name = wishloop.Name });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Assign(AssignWishloopModel model)
        {
            if (ModelState.IsValid)
            {                
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please select a valid wishloop to update." });

                Squid.Wishes.Wishloop wishloop = null;

                try
                {
                    wishloop = Squid.Wishes.Wishloop.GetWishloopById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "The selected wishloop is invalid." });
                }

                if (wishloop.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishloops that belong to you." });
                                                                
                string wishlus = "";
                try
                {
                    wishloop.UnsubscribeFromAll();

                    if (model.Wishlus != null && model.Wishlus.Count > 0)
                    {
                        foreach (Guid value in model.Wishlus)
                            wishloop.SubscribeTo(value);

                        foreach (Guid value in model.Wishlus)
                        {
                            wishlus = wishlus + value + ",";
                        }

                        if (wishlus.Length > 0)
                            wishlus = wishlus.Remove(wishlus.Length - 1, 1);
                    }
                    else
                    {
                        return Json(new { result = true, message = "Your wishloop has been unassigned from all wishlus.", wishlus = "" });
                    }
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error assigning your wishloop to the selected wishlus." });
                }

                return Json(new { result = true, message = "Your wishloop was assigned to the selected wishlus successfully.", wishlus = wishlus }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Remove(RemoveWishloopMembersModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please select a valid wishloop to update." });

                Squid.Wishes.Wishloop wishloop = null;

                try
                {
                    wishloop = Squid.Wishes.Wishloop.GetWishloopById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "The selected wishloop is invalid." });
                }

                if (wishloop.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishloops that belong to you." });

                if (wishloop.WishloopType == Squid.Wishes.WishloopType.AllFriends)
                    return Json(new { result = false, message = "You cannot remove members from the all friends wishloop." });
                                
                try
                {
                    if (model.Members != null && model.Members.Count > 0)
                    {                        
                        foreach (Guid value in model.Members)
                        {
                            wishloop.RemoveMemberAny(value);
                        }                        
                    }
                    else
                    {
                        return Json(new { result = false, message = "Please select one or more members to remove from this wishloop." });
                    }
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error removing the selected members from this wishloop." });
                }

                return Json(new { result = true, message = "The selected members have been removed successfully." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Add(AddWishloopMembersModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please select a valid wishloop to update." });

                Squid.Wishes.Wishloop wishloop = null;
                try
                {
                    wishloop = Squid.Wishes.Wishloop.GetWishloopById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "The selected wishloop is invalid." });
                }

                if (wishloop.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishloops that belong to you." });

                if (wishloop.WishloopType == Squid.Wishes.WishloopType.AllFriends)
                    return Json(new { result = false, message = "You cannot add members to the all friends wishloop." });

                string members = "";
                try
                {
                    if (model.Members != null && model.Members.Count > 0)
                    {
                        foreach (Guid value in model.Members)
                        {
                            wishloop.AddMember(value);
                            members = members + value + ",";
                        }
                        members = members.Remove(members.Length - 1, 1);
                    }
                    else
                    {
                        return Json(new { result = false, message = "Please select one or more members to add to this wishloop." });
                    }
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error adding the selected members to this wishloop." });
                }

                return Json(new { result = true, message = "The selected users have been added successfully.", members = members }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Copy(CopyWishloopMembersModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Wishloops == null || model.Wishloops.Count == 0)
                    return Json(new { result = false, message = "Please select one or more wishloops to add members to." });

                Squid.Wishes.Wishloop wishloop = null;

                foreach (Guid id in model.Wishloops)
                {
                    try
                    {
                        wishloop = Squid.Wishes.Wishloop.GetWishloopById(id);
                    }
                    catch
                    {
                        return Json(new { result = false, message = "The selected wishloop is invalid." });
                    }

                    if (wishloop.UserId != GetCurrentUserId())
                        return Json(new { result = false, message = "You may only modify wishloops that belong to you." });

                    if (wishloop.WishloopType == Squid.Wishes.WishloopType.AllFriends)
                        return Json(new { result = false, message = "You cannot add members to the all friends wishloop." });
                                        
                    try
                    {
                        if (model.Members != null && model.Members.Count > 0)
                        {
                            foreach (Guid value in model.Members)
                            {
                                wishloop.AddMember(value);                                
                            }                            
                        }
                        else
                        {
                            return Json(new { result = false, message = "Please select one or more members to copy to the wishloops." });
                        }
                    }
                    catch
                    {
                        return Json(new { result = false, message = "There was an error adding the selected members to the wishloops." });
                    }
                }
                
                return Json(new { result = true, message = "The selected users have been copied successfully." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Move(MoveWishloopMembersModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Wishloops == null || model.Wishloops.Count == 0)
                    return Json(new { result = false, message = "Please select one or more wishloops to add members to." });

                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "The wishloop you selected is invalid." });

                Squid.Wishes.Wishloop currentloop = null;
                try
                {
                    currentloop = Squid.Wishes.Wishloop.GetWishloopById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error getting the current wishloop." });
                }

                Squid.Wishes.Wishloop wishloop = null;

                if (currentloop.WishloopType == Squid.Wishes.WishloopType.UserDefined)
                    if (model.Members != null && model.Members.Count > 0)
                        currentloop.RemoveMembers(model.Members);
                                
                foreach (Guid id in model.Wishloops)
                {
                    try
                    {
                        wishloop = Squid.Wishes.Wishloop.GetWishloopById(id);
                    }
                    catch
                    {
                        return Json(new { result = false, message = "The selected wishloop is invalid." });
                    }

                    if (wishloop.UserId != GetCurrentUserId())
                        return Json(new { result = false, message = "You may only modify wishloops that belong to you." });

                    if (wishloop.WishloopType == Squid.Wishes.WishloopType.AllFriends)
                        return Json(new { result = false, message = "You cannot add members to the all friends wishloop." });

                    try
                    {
                        if (model.Members != null && model.Members.Count > 0)
                        {
                            foreach (Guid value in model.Members)
                            {
                                wishloop.AddMember(value);
                            }
                        }
                        else
                        {
                            return Json(new { result = false, message = "Please select one or more members to move to the wishloops." });
                        }
                    }
                    catch
                    {
                        return Json(new { result = false, message = "There was an error moving the selected members to the wishloops." });
                    }
                }

                return Json(new { result = true, message = "The selected users have been moved successfully." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Update(UpdateWishloopModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Name == null || model.Name == "" || model.Name == "null")
                    return Json(new { result = false, message = "Please provide a name for your new wishloop." });

                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please select a valid wishloop to update." });

                Squid.Wishes.Wishloop wishloop = null;

                try
                {
                    wishloop = Squid.Wishes.Wishloop.GetWishloopById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "The selected wishloop is invalid." });
                }

                if (wishloop.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only modify wishloops that belong to you." });

                string coltmp = model.Color;
                if (coltmp == "" || coltmp == null || coltmp == String.Empty)
                    return Json(new { result = false, message = "Please select a color to identify your new wishloop." });

                try
                {
                    if (wishloop.WishloopType == Squid.Wishes.WishloopType.UserDefined)
                        wishloop.Name = model.Name;

                    wishloop.DisplayColor = Int32.Parse(coltmp.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
                    wishloop.Update();
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error updating your wishloop." });
                }

                string wishlus = "";
                try
                {
                    wishloop.UnsubscribeFromAll();

                    if (model.Wishlus != null && model.Wishlus.Count > 0)
                        foreach (Guid value in model.Wishlus)
                            wishloop.SubscribeTo(value);

                    foreach (Guid value in model.Wishlus)
                    {
                        wishlus = wishlus + value + ",";
                    }

                    if (wishlus.Length > 0)
                        wishlus = wishlus.Remove(wishlus.Length - 1, 1);
                }
                catch
                {
                    return Json(new { result = false, message = "Your wishloop has been updated, but there was an error assigning it to the selected wishlus." });
                }

                return Json(new { result = true, message = "Your wishloop was updated successfully.", id = wishloop.Id, name = wishloop.Name, wishlus = wishlus }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }
                                        
        [Authorize]
        [HttpPost]
        public
        ActionResult
        Delete(DeleteWishloopModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "The selected wishloop is invalid." });

                Squid.Wishes.Wishloop wishLoop = null;
                try
                {
                    wishLoop = Squid.Wishes.Wishloop.GetWishloopById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "Please select a valid wishloop to delete." });
                }

                if (wishLoop.WishloopType == Squid.Wishes.WishloopType.AllFriends)
                {
                    return Json(new { result = false, message = "The friends wishloop is systematically generated and can not be deleted." });
                }

                if (wishLoop.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may only delete wishloops that belong to you." });

                wishLoop.DeleteWishloop();

                return Json(new { result = true, message = "Your wishloop has been successfully deleted." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }        
    }
    
    [Serializable]
    public class CreateWishloopModel
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public List<Guid> Wishlus { get; set; }
    }

    [Serializable]
    public class UpdateWishloopModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public List<Guid> Wishlus { get; set; }
    }

    [Serializable]
    public class DeleteWishloopModel
    {
        public Guid Id { get; set; }
    }

    [Serializable]
    public class AssignWishloopModel
    {
        public Guid Id { get; set; }
        public List<Guid> Wishlus { get; set; }
    }

    [Serializable]
    public class RemoveWishloopMembersModel
    {
        public Guid Id { get; set; }
        public List<Guid> Members { get; set; }
    }

    [Serializable]
    public class AddWishloopMembersModel
    {
        public Guid Id { get; set; }
        public List<Guid> Members { get; set; }
    }

    [Serializable]
    public class CopyWishloopMembersModel
    {
        public List<Guid> Wishloops { get; set; }
        public List<Guid> Members { get; set; }
    }

    [Serializable]
    public class MoveWishloopMembersModel
    {
        public List<Guid> Wishloops { get; set; }
        public List<Guid> Members { get; set; }
        public Guid Id { get; set; }
    }

    [Serializable]
    public class WishloopColorModel
    {
        public string Color { get; set; }
        public Guid Id { get; set; }
    }

    [Serializable]
    public class WishloopNameModel
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }

    [Serializable]
    public class WishloopInputModel
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public string Wishlus { get; set; }
    }

    [Serializable]
    public class WishloopEditModel
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string Color { get; set; }
        public string Wishlus { get; set; }
    }
}