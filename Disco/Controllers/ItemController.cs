using Milkshake;
using Squid;
using Squid.Log;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Xml;

namespace Disco.Controllers
{
    public class ItemController : BaseController
    {
        [Authorize]
        public ActionResult Index(int sort = 0)
        {
            List<Squid.Wishes.Wish> model = Squid.Users.User.GetUsersWishes(GetCurrentUserId());

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

            return View("Index", model);
        }
        
        [Authorize]
        public ActionResult Hunt()
        {
            return base.View("Add");
        }
        
        [Authorize]
        public ActionResult JustJot(string name = "")
        {            
            ViewBag.ItemName = WebUtility.HtmlDecode(name);

            return base.View("JustJot");
        }
        
        [AllowAnonymous]
        public ActionResult ViewOther(Guid id)
        {            
            Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(id);

            if (Request.IsAuthenticated)
            {
                if (wish.UserId == GetCurrentUserId())
                    return RedirectToAction("view", "item", new { @id = id });
                else
                    return View("ViewOther", wish);
            }
            else
                return View("ViewOther", wish);
        }
        
        [AllowAnonymous]
        public ActionResult View(Guid id)
        {
            Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(id);
                                   
            if (Request.IsAuthenticated)
            {                
                if (wish.UserId == GetCurrentUserId())
                    return View("View", wish);
                else
                    return View("ViewOther", wish);
            }
            else
            {
                return View("ViewOther", wish);
            }
        }
        
        [Authorize]
        public ActionResult Button(string url)
        {            
            if (!Site.SiteExists(new Uri(url)))
            {
                TempData["ErrorMessage"] = "The site you have attempted to add from is not yet supported by wishlu, but we have taken notice and will add it to our platform.";
                return RedirectToAction("index", "wishlu");
            }

            System.Net.WebClient client = new System.Net.WebClient();

            System.IO.Stream webhtml = client.OpenRead(url);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(webhtml);

            Site site = Site.GetSite(new Uri(url));

            RawProduct pn = site.Parse(doc, new Uri(url));

            if (pn == null)
            {
                TempData["ErrorMessage"] = "The page you attempted to add from did not contain valid product data.";
                return RedirectToAction("index", "wishlu");
            }

            decimal price = 0;
            Squid.Wishes.Wish addWish = new Squid.Wishes.Wish();
            addWish.Name = pn.Name;
            addWish.UserId = GetCurrentUserId();
            addWish.WishUrl = pn.Url;
            addWish.GtinCode = pn.UPC;
            addWish.Quantity = 1;
            //addWish.Shop = pn.Store;
            addWish.Description = pn.Description;

            addWish.WishStatus = Squid.Wishes.WishStatus.Requested;
                        
            if (Decimal.TryParse(pn.Price.Replace("$", "").Replace(",", ""), out price))
            {
                addWish.Price = price;
            }

            addWish.IsCustom = false;
            addWish.Create();
            addWish.SetImage(new Uri(pn.Image));

            return RedirectToAction("view", new { @id = addWish.Id });
        }

        [Authorize]
        [HttpPost]        
        public JsonResult Add(AddWishModel model)
        {
            if (ModelState.IsValid)
            {                
                // universal attributes                
                if (model.Quantity <= 0)
                    model.Quantity = 1; // prevent null or less than zero quantity

                // wishlu
                if (model.Wishlu == null || model.Wishlu == Guid.Empty)
                {
                    if (String.IsNullOrEmpty(model.WishluName))
                    {
                        try
                        {
                            model.Wishlu = Squid.Wishes.Wishlu.GetUsersJustMeWishLu(GetCurrentUserId()).Id;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            return Json(new { result = false, message = "Please make sure you selected an existing wishlu or provided a name to create a new one. The specified wishlu ID was invalid." });
                        }
                    }
                    else
                    {
                        try
                        {
                            Squid.Wishes.Wishlu wishlu = new Squid.Wishes.Wishlu();
                            wishlu.Name = model.WishluName;
                            wishlu.UserId = GetCurrentUserId();
                            wishlu.EventDateTime = null;
                            wishlu.Visibility = Squid.Wishes.WishluVisibility.Friends;
                            wishlu.WishLuType = Squid.Wishes.WishluType.UserDefined;
                            wishlu.Create();
                            wishlu.AddSubscriber(Squid.Wishes.Wishloop.GetAllFriendsWishloopByUserId(GetCurrentUserId()).Id);

                            model.Wishlu = wishlu.Id;
                        }
                        catch
                        {
                            return Json(new { result = false, message = "A new wishlu was unable to be created for this item." });
                        }
                    }
                }

                if (model.ProductType == ProductType.milkshake)
                {
                    if (model.Id == null || model.Id == Guid.Empty)
                        return Json(new { result = false, message = "The given product ID is either invalid or the associated product no longer exists." });
                    
                    try
                    {
                        Squid.Wishes.Wish wish = new Squid.Wishes.Wish();

                        Milkshake.Product p;
                        Milkshake.Offer o;

                        p = Milkshake.Search.ProductId(model.Id); //Milkshake.ProductManager.GetProduct(model.Id);

                        if (p == null || p.Id == Guid.Empty)
                            return Json(new { result = false, message = "This product either no longer exists, or the product ID provided was invalid." });

                        o = p.GetLowestOffer();

                        if (!String.IsNullOrEmpty(model.Notes))                        
                            wish.Notes = model.Notes;                            
                        
                        wish.CreateFromMilkshake(p, o, GetCurrentUserId(), model.Wishlu, model.Quantity);

                        Milkshake.Search.ProductSave(p.Id);

                        return Json(new { result = true, message = "This item has been added successfully." });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        return Json(new { result = false, message = "There was an unexpected error adding this item. We have been notified. Please try again shortly. We apologize for the inconvenience." });
                    }
                }
                else if (model.ProductType == ProductType.amazon)
                {
                    Squid.Wishes.Wish wish = new Squid.Wishes.Wish();

                    Squid.Products.Amazon.AmazonProduct ap;

                    decimal price = 0;
                    ap = Squid.Products.Amazon.AmazonProvider.Lookup(model.ASIN);

                    if (ap == null || ap.ASIN != model.ASIN)
                        return Json(new { result = false, message = "This product either no longer exists, or the product ASIN provided was invalid." });
                                        
                    if (!String.IsNullOrEmpty(model.Notes))
                        wish.Notes = model.Notes;

                    wish.Name = ap.Name;
                    wish.UserId = GetCurrentUserId();
                    wish.WishUrl = ap.Url;
                    wish.Quantity = model.Quantity;
                    wish.Description = ap.LongDescription;

                    if (Decimal.TryParse(ap.Price.Replace("$", "").Replace(",", ""), out price))
                        wish.Price = price;
                    else
                        return Json(new { result = false, message = "The specified product has an invalid price." });

                    wish.IsAmazon = true;

                    wish.Create();
                                        
                    wish.SetImage(new Uri(ap.Image));
                    
                    if (model.Wishlu != null && model.Wishlu != Guid.Empty)
                        wish.AssignToWishlu(model.Wishlu);

                    return Json(new { result = true, message = "This item has been added successfully." });
                }
                else if (model.ProductType == ProductType.bestbuy)
                {
                    Squid.Wishes.Wish wish = new Squid.Wishes.Wish();

                    Squid.Products.BestBuy.BestBuyProduct bp;

                    bp = Squid.Products.BestBuy.BestBuyProvider.Lookup(model.SKU);

                    if (bp == null || bp.SKU != model.SKU)
                        return Json(new { result = false, message = "This product either no longer exists, or the product SKU provided was invalid." });
                    
                    if (!String.IsNullOrEmpty(model.Notes))
                        wish.Notes = model.Notes;

                    wish.Name = bp.Name;
                    wish.UserId = GetCurrentUserId();
                    wish.WishUrl = bp.Url;
                    wish.Quantity = model.Quantity;
                    wish.Description = bp.LongDescription;

                    wish.Price = bp.SalePrice;

                    wish.IsBestBuy = true;

                    wish.Create();

                    wish.SetImage(new Uri(bp.LargeImage));

                    if (model.Wishlu != null && model.Wishlu != Guid.Empty)
                        wish.AssignToWishlu(model.Wishlu);

                    return Json(new { result = true, message = "This item has been added successfully." });
                }
                else if (model.ProductType == ProductType.etsy)
                {
                    // ENHANCEMENT: Implement Etsy product lookup and wish creation
                    Squid.Wishes.Wish wish = new Squid.Wishes.Wish();

                    return Json(new { result = false, message = "Adding Etsy products has not yet been implemented, and will be enabled once finished." });
                }
                else if (model.ProductType == ProductType.custom)
                {
                    if (String.IsNullOrEmpty(model.Name))
                        return Json(new { result = false, message = "Please specify a name for your new item." });

                    //if (String.IsNullOrEmpty(model.Description))                        
                    //    return Json(new { result = false, message = "Please add a description for your new item." });

                    //if (String.IsNullOrEmpty(model.Price))
                    //    return Json(new { result = false, message = "Please give your new item a price." });

                    //if (String.IsNullOrEmpty(model.Image))
                    //    return Json(new { result = false, message = "Please upload an image to identify your new item." });
                                        
                    try
                    {
                        Squid.Wishes.Wish wish = new Squid.Wishes.Wish();

                        decimal price = 0;

                        wish.Name = model.Name;
                        wish.UserId = GetCurrentUserId();

                        if (!String.IsNullOrEmpty(model.Url))
                        {
                            if (!Uri.IsWellFormedUriString(model.Url, UriKind.Absolute))
                                return Json(new { result = false, message = "The website you entered is not a validly formed URL. A valid URL starts with \"http://\", \"https://\", or \"www.\"." });

                            wish.WishUrl = model.Url;
                        }
                        
                        wish.Quantity = model.Quantity;

                        if (!String.IsNullOrEmpty(model.Description))
                            wish.Description = model.Description;

                        if (!String.IsNullOrEmpty(model.Price) && Decimal.TryParse(model.Price.Replace("$", "").Replace(",", ""), out price))
                            wish.Price = price;
                        else if (!String.IsNullOrEmpty(model.Price))
                            return Json(new { result = false, message = "The price you provided was invalid or not properly formatted. Please enter using only digits, the dollar sign, commas, and a period. Punctuation is optional." });

                        wish.IsCustom = true;

                        wish.Create();

                        if (!String.IsNullOrEmpty(model.Image))
                        {
                            model.Image = WebUtility.HtmlDecode(model.Image);
                            model.Image = WebUtility.UrlDecode(model.Image);
                            model.Image = model.Image.Replace("&amp;", "&").Trim();

                            wish.SetImage(new Uri(model.Image));
                        }
                        
                        if (model.Wishlu != null && model.Wishlu != Guid.Empty)
                            wish.AssignToWishlu(model.Wishlu);

                        // ENHANCEMENT: Post action to Facebook
                        //Squid.Users.FacebookManager.Client.AccessToken = "296645670486904|KK_Jly0DUrHVxOXYaqOfQw_AT4A";
                        //Squid.Users.FacebookManager.Client.Post("/" + GetCurrentUser().FacebookPageId + "/wishlu:wish", new { wish = "http://www.wishlu.com/i/" + addWish.Id });

                        return Json(new { result = true, message = "Your new item has been saved successfully.", id = wish.Id });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        throw ex;
                        //return Json(new { result = false, message = "An unexpected error occurred while attempting to add your item." });
                    }
                }
                else
                {
                    return Json(new { result = false, message = "The specified product type does not exist." });
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }
           
        [Authorize]
        [HttpPost]
       public ActionResult Like(WishLikeModel model)
        {
            if (model.Id == null || model.Id == Guid.Empty)
                return JsonResponse(false, "Please provide an item to like.");

            Squid.Wishes.Wish wish = null;

            try
            {
                wish = Squid.Wishes.Wish.GetWishById(model.Id);

                wish.Like(model.UserId);

                return JsonResponse(true, "The item has been liked successfully.");
            }
            catch
            {
                return JsonResponse(false,"The item you selected does not exist.");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Unlike(WishLikeModel model)
        {
            if (model.Id == null || model.Id == Guid.Empty)
                return JsonResponse(false, "Please provide an item to like.");

            Squid.Wishes.Wish wish = null;

            try
            {
                wish = Squid.Wishes.Wish.GetWishById(model.Id);

                wish.Unlike(model.UserId);

                return JsonResponse(true, "The item has been liked successfully.");
            }
            catch
            {
                return JsonResponse(false, "The item you selected does not exist.");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Promise(PromiseWishModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please provide an item to promise / give." });

                Squid.Wishes.Wish wish = null;

                try
                {
                    wish = Squid.Wishes.Wish.GetWishById(model.Id);
                }
                catch
                {
                    return Json(new { result = false, message = "The item you selected does not exist." });
                }

                if (wish.WishStatus == Squid.Wishes.WishStatus.Revealed && wish.GetRevealedPromises().Count >= wish.Quantity)
                {
                    return Json(new { result = false, message = "This item has already been promised and revealed by another user." });
                }

                if (wish.WishStatus == Squid.Wishes.WishStatus.Confirmed && wish.GetConfirmedPromises().Count >= wish.Quantity)
                {
                    return Json(new { result = false, message = "This item has already been fulfilled and confirmed." });
                }

                try
                {
                    DateTimeOffset date = DateTimeOffset.Now;
                    try
                    {
                        date = DateTimeOffset.Parse(model.Date);
                    }
                    catch
                    {
                        return Json(new { result = false, message = "The date provided was not in a proper format or was not a valid date." });
                    }
                    
                    Squid.Wishes.Promise promise = wish.Promise(GetCurrentUserId(), date);
                    string name = Squid.Users.User.GetUserFullName(wish.UserId).Split(' ')[0];

                    // reveal
                    if (date.Date <= DateTimeOffset.Now.Date)
                    {
                        wish.Reveal(promise);
                        
                        return Json(new { result = true, message = name + " has been notified of your gift." });
                    }

                    return Json(new { result = true, message = name + " will be notified of your gift on " + model.Date + "." });
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return Json(new { result = false, message = "There was an error promising this item." });
                }                         
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }             
        }

        [Authorize]
        [HttpPost]
        public
        ActionResult Cancel(CancelPromiseModel model)
        {
            if (ModelState.IsValid)
            {
                Squid.Wishes.Wish wish = null;

                try { wish = Squid.Wishes.Wish.GetWishById(model.Id); }
                catch
                {
                    return Json(new { result = false, message = "The specified item does not exist." });
                }

                Squid.Wishes.Promise promise = null;
                try
                {
                    promise = wish.GetPromise(GetCurrentUserId());
                }
                catch
                {
                    return Json(new { result = false, message = "You have not promised to give this item." });
                }


                try
                {
                    wish.Cancel(promise);

                    return Json(new { result = true, message = "Your promise to gift this item has been canceled." });
                }
                catch
                {
                    return Json(new { result = false, message = "There was an error canceling your promise." });
                }                
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }
                
        // Individual gifting confirm button is clicked
        [Authorize]        
        public ActionResult Confirm(Guid id)
        {
            Squid.Wishes.Promise p = Squid.Wishes.Promise.GetPromiseById(id);

            Squid.Wishes.Wish wish = p.GetWish();

            if (wish.UserId != GetCurrentUserId())
                return Json(new { result = false, message = "You can only mark your own items as received." });

            wish.Confirm(p);

            TempData["SuccessMessage"] = "You have successfully confirmed receipt of this gift.";

            return RedirectToAction("view", new { @id = wish.Id });
        }

        // Item's "confirm as received" is clicked
        [Authorize]
        [HttpPost]
        public ActionResult Mark(Guid id)
        {
            try
            {
                if (id == null || id == Guid.Empty)
                    return JsonResponse(false, "Please provide an item to confirm receipt of a gift.");

                Squid.Wishes.Wish wish;

                try
                {
                    wish = Squid.Wishes.Wish.GetWishById(id);
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item could not be found.");
                }
                
                if (wish.UserId != GetCurrentUserId())
                    return JsonResponse(false, "You can only mark your own items as received.");
                                
                List<Squid.Wishes.Promise> all = wish.GetAllPromises();

                if (wish.Purchased >= wish.Quantity && wish.WishStatus == Squid.Wishes.WishStatus.Confirmed)
                    return JsonResponse(false, "You already confirmed this item. The number of confirmed gifts matches the item's desired quantity.");
                
                List<Squid.Wishes.Promise> gifted = all.Where(x => (x.PromiseStatus == Squid.Wishes.PromiseStatus.Promised || x.PromiseStatus == Squid.Wishes.PromiseStatus.Revealed)).ToList();
                
                // There is only 1 gift that is not yet confirmed
                if (gifted.Count >= 1)
                {
                    // confirm the item's only gift
                    wish.Confirm(gifted.First());

                    return JsonResponse(true, "You have successfully confirmed receipt of this gift. If more than one person has gifted this item, please confirm their gift as well.");
                }
                                
                return JsonResponse(false, "The item you provided cannot be confirmed.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return JsonResponse(false, "An unexpected error occurred while attempting to confirm receipt of your item.");
            }
        }
        
        [Authorize]
        [HttpPost]
        public ActionResult Delete(DeleteWishesModel model)
        {
            try
            {
                if (model == null)
                    return Json(new { result = false, message = "The server has received an invalid model." });

                if (model.Wishes.Count < 1)
                    return Json(new { result = false, message = "Please select one or more items to delete." });
                                
                foreach (Guid wid in model.Wishes)
                {
                    Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(wid);

                    if (wish.UserId != GetCurrentUserId())
                        return Json(new { result = false, message = "You may not delete an item that does not belong to you." });

                    wish.DeleteWish();
                }

                return Json(new { result = true, message = model.Wishes.Count + " item(s) successfully deleted." });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "An unhandled server error has occurred. We have been notified of the error and are working to fix it." });
            }
        }

        [Authorize]
        [HttpPost]     
        public ActionResult Name(WishNameModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select an item to update.");

                if (String.IsNullOrEmpty(model.Name))
                    return JsonResponse(false, "Please provide a name for your item.");

                try
                {
                    var wish = Squid.Wishes.Wish.GetWishById(model.Id);

                    if (wish.UserId != GetCurrentUserId())
                        return JsonResponse(false, "You may only update items that belong to you.");

                    wish.Name = model.Name;
                    wish.Update();

                    return JsonResponse(true, "Your item's name has been updated successfully.");
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item does not exist.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return JsonResponse(false, "An unexpected error occurred while updating your item. We have been notified. Please try again shortly.");
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult WishUrl(WishUrlModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select an item to update.");

                if (String.IsNullOrEmpty(model.Url))
                    return JsonResponse(false, "Please provide a URL for your item.");

                try
                {
                    if (!model.Url.StartsWith("http://") && !model.Url.StartsWith("https://"))
                        model.Url = "http://" + model.Url;
                                        
                    if (!Uri.IsWellFormedUriString(model.Url, UriKind.Absolute))
                        return JsonResponse(false, "The URL you provided is not in a valid format.");

                    /*try
                    {
                        using (var client = new HeadWebClient())
                        {
                            client.HeadOnly = true;
                            client.DownloadString(model.Url);
                        }
                    }
                    catch
                    {
                        return JsonResponse(false, "The URL you provided does not exist.");
                    }*/

                    var wish = Squid.Wishes.Wish.GetWishById(model.Id);

                    if (wish.UserId != GetCurrentUserId())
                        return JsonResponse(false, "You may only update items that belong to you.");

                    wish.WishUrl = model.Url;
                    wish.Update();

                    return JsonResponse(true, "Your item's URL has been updated successfully.");
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item does not exist.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return JsonResponse(false, "An unexpected error occurred while updating your item. We have been notified. Please try again shortly.");
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]     
        public ActionResult Quantity(WishQuantityModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select an item to update.");

                if (model.Quantity <= 0)
                    model.Quantity = 1;
                    
                try
                {
                    var wish = Squid.Wishes.Wish.GetWishById(model.Id);

                    if (wish.UserId != GetCurrentUserId())
                        return JsonResponse(false, "You may only update items that belong to you.");

                    wish.Quantity = model.Quantity;
                    wish.Update();

                    return JsonResponse(true, "Your requested quantity has been updated successfully.");
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item does not exist.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return JsonResponse(false, "An unexpected error occurred while updating your item. We have been notified. Please try again shortly.");
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Purchased(WishPurchasedModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select an item to update.");

                if (model.Purchased < 0)
                    model.Purchased = 0;

                try
                {
                    var wish = Squid.Wishes.Wish.GetWishById(model.Id);

                    if (wish.UserId != GetCurrentUserId())
                        return JsonResponse(false, "You may only update items that belong to you.");

                    wish.Purchased = model.Purchased;
                    if (wish.Purchased == 0) // if the user has reset the gift's purchased count, mark it as requested again
                        wish.WishStatus = Squid.Wishes.WishStatus.Requested;
                    else if (wish.Purchased >= wish.Quantity) // if the user is manually confirming a wish, or logging a gift they received outside of wishlu's gifting system
                        wish.WishStatus = Squid.Wishes.WishStatus.Confirmed;

                    wish.Update();
                                        
                    return JsonResponse(true, "This item's total purchased count has been updated successfully.");
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item does not exist.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return JsonResponse(false, "An unexpected error occurred while updating your item. We have been notified. Please try again shortly.");
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]     
        public ActionResult Price(WishPriceModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select an item to update.");
                                
                try
                {
                    var wish = Squid.Wishes.Wish.GetWishById(model.Id);

                    if (wish.UserId != GetCurrentUserId())
                        return JsonResponse(false, "You may only update items that belong to you.");

                    wish.Price = model.Price;
                    wish.Update();

                    return JsonResponse(true, "Your item's price has been updated successfully.");
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item does not exist.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return JsonResponse(false, "An unexpected error occurred while updating your item. We have been notified. Please try again shortly.");
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]     
        public ActionResult Description(WishDescriptionModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select an item to update.");
                               
                try
                {
                    var wish = Squid.Wishes.Wish.GetWishById(model.Id);

                    if (wish.UserId != GetCurrentUserId())
                        return JsonResponse(false, "You may only update items that belong to you.");

                    wish.Description = model.Description;
                    wish.Update();

                    return JsonResponse(true, "Your item's description has been updated successfully.");
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item does not exist.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return JsonResponse(false, "An unexpected error occurred while updating your item. We have been notified. Please try again shortly.");
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]     
        public ActionResult Notes(WishNotesModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select a item to update.");

                try
                {
                    var wish = Squid.Wishes.Wish.GetWishById(model.Id);

                    if (wish.UserId != GetCurrentUserId())
                        return JsonResponse(false, "You may only update items that belong to you.");

                    wish.Notes = model.Notes;
                    wish.Update();

                    return JsonResponse(true, "Your item's notes have been updated successfully.");
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item does not exist.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return JsonResponse(false, "An unexpected error occurred while updating your item. We have been notified. Please try again shortly.");
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]     
        public ActionResult Image(WishImageModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select an item to update.");

                if (String.IsNullOrEmpty(model.Image))
                    return JsonResponse(false, "Please upload a valid image or provide an image URL.");

                try
                {
                    var wish = Squid.Wishes.Wish.GetWishById(model.Id);

                    if (wish.UserId != GetCurrentUserId())
                        return JsonResponse(false, "You may only update items that belong to you.");

                    if (Uri.IsWellFormedUriString(model.Image, UriKind.RelativeOrAbsolute))
                    {
                        wish.SetImage(new Uri(model.Image));
                    }
                    else
                    {
                        wish.SetImage(model.Image);
                    }
                    
                    return JsonResponse(true, "Your item's image has been updated successfully.");
                }
                catch (ItemNotFoundException)
                {
                    return JsonResponse(false, "The specified item does not exist.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return JsonResponse(false, "An unexpected error occurred while updating your item. We have been notified. Please try again shortly.");
                }
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Rate(RateWishModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return JsonResponse(false, "Please select an item to rate.");

                if (model.Rating < 0)
                    model.Rating = 0;

                if (model.Rating > 5)
                    model.Rating = 5;

                Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(model.Id);

                if (wish.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may not rate an item that does not belong to you." });

                // TODO: This averaging model should be used for product ratings.
                //if (wish.RatingCount > 0)
                //    wish.Rating = (wish.Rating + model.Rating) / 2; // average
                //else
                    wish.Rating = model.Rating;

                wish.RatingCount++;

                wish.Update();

                return Json(new { result = true, message = "Your rating has been recorded successfully.", rating = wish.Rating, count = wish.RatingCount });
            }
            else
            {
                return JsonResponse(false, "The server has received an invalid model.");
            }
        }

        [Authorize]
        [HttpPost]  
        public ActionResult Wishlu(AssignWishModel model)
        {
            try
            {
                if (model == null)
                    return Json(new { result = false, message = "The server has received an invalid model." });

                if (model.Wish == null || model.Wish == Guid.Empty)
                    return Json(new { result = false, message = "Please select an item to assign to a wishlu." });

                Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(model.Wish);

                if (wish.UserId != GetCurrentUserId())
                    return Json(new { result = false, message = "You may not assign an item that does not belong to you." });

                if (String.IsNullOrEmpty(model.WishluName) && (model.Wishlu == null || model.Wishlu == Guid.Empty))
                    return Json(new { result = false, message = "Please make sure you selected an existing wishlu or provided a name to create a new one. The specified wishlu ID was invalid." });
                else if (Squid.Wishes.Wishlu.GetWishLuById(model.Wishlu) != null)
                    wish.AssignToWishlu(model.Wishlu);
                else if (model.Wishlu == null || model.Wishlu == Guid.Empty)
                {
                    try
                    {
                        if (String.IsNullOrEmpty(model.WishluName))
                            return JsonResponse(false, "Empty names are not permitted for new wishlus.");
                        
                        Squid.Wishes.Wishlu wishlu = new Squid.Wishes.Wishlu();
                        wishlu.Name = model.WishluName;
                        wishlu.UserId = GetCurrentUserId();
                        wishlu.EventDateTime = null;
                        wishlu.Visibility = Squid.Wishes.WishluVisibility.Friends;
                        wishlu.WishLuType = Squid.Wishes.WishluType.UserDefined;
                        wishlu.Create();
                        wishlu.AddSubscriber(Squid.Wishes.Wishloop.GetAllFriendsWishloopByUserId(GetCurrentUserId()).Id);

                        model.Wishlu = wishlu.Id;
                    }
                    catch
                    {
                        return Json(new { result = false, message = "A new wishlu was unable to be created for this item." });
                    }
                }
                                
                wish.AssignToWishlu(model.Wishlu);
                
                return Json(new { result = true, message = "Your item was successfully assigned." });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "An unhandled server error has occurred. We have been notified of the error and are working to fix it." });
            }
        }

        [Authorize]
        [HttpPost]        
        public ActionResult Assign(AssignWishesModel model)
        {
            try
            {
                if (model == null)
                    return Json(new { result = false, message = "The server has received an invalid model." });

                if (model.Wishes.Count < 1)
                    return Json(new {result=false,message="Please select one or more items to assign to a wishlu."});

                if (model.Wishlu == null || model.Wishlu == Guid.Empty)
                    return Json(new {result=false,message="Please select a valid wishlu to assign the select item(s) to." });
                                
                if (String.IsNullOrEmpty(model.WishluName) && (model.Wishlu == null || model.Wishlu == Guid.Empty))                    
                    return Json(new { result = false, message = "Please make sure you selected an existing wishlu or provided a name to create a new one. The specified wishlu ID was invalid." });
                else if (model.Wishlu == null || model.Wishlu == Guid.Empty)
                {
                    try
                    {
                        if (String.IsNullOrEmpty(model.WishluName))
                            return JsonResponse(false, "Empty names are not permitted for new wishlus.");

                        Squid.Wishes.Wishlu wishlu = new Squid.Wishes.Wishlu();
                        wishlu.Name = model.WishluName;
                        wishlu.UserId = GetCurrentUserId();
                        wishlu.EventDateTime = null;
                        wishlu.Visibility = Squid.Wishes.WishluVisibility.Friends;
                        wishlu.WishLuType = Squid.Wishes.WishluType.UserDefined;
                        wishlu.Create();

                        model.Wishlu = wishlu.Id;
                    }
                    catch
                    {
                        return Json(new { result = false, message = "A new wishlu was unable to be created for this item." });
                    }
                }

                foreach (Guid wid in model.Wishes)
                {
                    Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(wid);

                    if (wish.UserId != GetCurrentUserId())
                        return Json(new {result=false,message="You may not assign an item that does not belong to you." });

                    wish.AssignToWishlu(model.Wishlu);
                }

                return Json(new { result = true, message = model.Wishes.Count + " item(s) successfully assigned." });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "An unexpected server error has occurred. We have been notified of the error and are working to fix it." });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Copy(CopyWishesModel model)
        {
            try
            {
                if (model == null)
                    return Json(new { result = false, message = "The server has received an invalid model." });

                if (model.Wishes.Count < 1)
                    return Json(new { result = false, message = "Please select one or more items to copy." });

                if (model.Wishlu == null || model.Wishlu == Guid.Empty)
                    return Json(new { result = false, message = "Please select a valid wishlu to copy the selected item(s) to." });

                if (String.IsNullOrEmpty(model.WishluName) && (model.Wishlu == null || model.Wishlu == Guid.Empty))
                    return Json(new { result = false, message = "Please make sure you selected an existing wishlu or provided a name to create a new one. The specified wishlu ID was invalid." });
                else if (model.Wishlu == null || model.Wishlu == Guid.Empty)
                {
                    try
                    {
                        if (String.IsNullOrEmpty(model.WishluName))
                            return JsonResponse(false, "Empty names are not permitted for new wishlus.");

                        Squid.Wishes.Wishlu wishlu = new Squid.Wishes.Wishlu();
                        wishlu.Name = model.WishluName;
                        wishlu.UserId = GetCurrentUserId();
                        wishlu.EventDateTime = null;
                        wishlu.Visibility = Squid.Wishes.WishluVisibility.Friends;
                        wishlu.WishLuType = Squid.Wishes.WishluType.UserDefined;
                        wishlu.Create();

                        model.Wishlu = wishlu.Id;
                    }
                    catch
                    {
                        return Json(new { result = false, message = "A new wishlu was unable to be created for this item." });
                    }
                }

                foreach (Guid wid in model.Wishes)
                {
                    Squid.Wishes.Wish.GetWishById(wid).Copy(model.Wishlu);
                }

                return Json(new { result = true, message = model.Wishes.Count + " item(s) successfully copied." });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "An unexpected server error has occurred. We have been notified of the error and are working to fix it." });
            }
        }
        
        [Authorize]
        [HttpPost]
        public ActionResult Steal(Guid id)
        {            
            Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(id);

            if (wish.UserId == GetCurrentUserId())
            {
                return Json(new { result = false, message = "You can't steal your own item." });
            }

            try
            {
                Squid.Wishes.Wish newWish = wish.Steal(GetCurrentUserId());
                return Json(new { result = true, message = "This item has been stolen successfully. You can view it in your just me wishlu." });
            }
            catch
            {
                return Json(new { result = false, message = "An error occurred while attempting to steal this item." });
            }
        }
        
        [Authorize]
        [HttpPost]
        public ActionResult Grab(Guid id)
        {            
            Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(id);
                        
            if (wish.UserId == GetCurrentUserId())
            {
                return Json(new { result = false, message = "You can't grab your own item." });
            }

            try
            {
                Squid.Wishes.Wish newWish = wish.Grab(GetCurrentUserId());
                return Json(new { result = true, message = "This item has been copied successfully. You can view it in your just me wishlu." });
            }
            catch
            {
                return Json(new{result=false,message="An error occurred while attempting to copy this item."});
            }                        
        }

        [Authorize]
        [HttpPost]
        public ActionResult Phone(Guid id)
        {
            Squid.Wishes.Wish wish = Squid.Wishes.Wish.GetWishById(id);
            string name = Squid.Users.User.GetUserFullName(wish.UserId);
                        
            User current = GetCurrentUser();

            try
            {
                if (String.IsNullOrEmpty(current.PhoneNumber))
                {
                    TempData["ErrorMessage"] = "You must first add and verify a mobile number to send text links.";
                    return Json(new { result = false, message = "You must first add and verify a mobile number to send text links.", redirect=true });
                }

                if (wish.UserId == GetCurrentUserId())                
                    current.SendText("Your item: " + wish.Name + ". http://www.wishlu.com/i/" + wish.Id);                
                else                
                    current.SendText(name + "'s item: " + wish.Name + ". http://www.wishlu.com/i/" + wish.Id);
                                
                return Json(new { result = true, message = "A text with a link to this item has been sent to your phone." });
            }
            catch
            {
                return Json(new { result = false, message = "An error occurred while attempting to text your phone." });
            }
        }
                
        [Authorize]
        [HttpPost]
        public ActionResult Suggest(SuggestWishModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == null || model.Id == Guid.Empty)
                    return Json(new { result = false, message = "Please select a wish to suggest to your friends." });

                if (model.Friends == null || model.Friends.Count == 0)
                    return Json(new { result = false, message = "Please select at least one friend to suggest to." });
                                                                
                User current = GetCurrentUser();
                foreach (Guid friend in model.Friends)
                {
                    current.SuggestWish(model.Id, friend);
                }

                return Json(new { result = true, message = "You have suggested this wish to " + model.Friends.Count + " friend(s)." });
            }
            else
            {
                return Json(new { result = false, message = "The server received an invalid model." });
            }                        
        }
        
        public string ScoutGeo(FormCollection col)
        {
            string final = "";

            string upc = col["upc"];
            string latlon = col["latlon"];
            string zipcode;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latlon + "&sensor=false");
            request.Method = "GET";
            request.ContentType = "text/xml;charset=utf-8";

            StreamReader rdr = new StreamReader(request.GetResponse().GetResponseStream());

            XmlDocument doc = new XmlDocument();
            doc.Load(rdr);
            rdr.Close();

            XmlNode zipnode = null;

            foreach (XmlNode result in doc.GetElementsByTagName("result"))
            {
                if (result["type"].InnerText == "postal_code")
                {
                    zipnode = result;
                    break;
                }
            }

            if (zipnode != null)
            {
                zipcode = zipnode.ChildNodes[2]["long_name"].InnerText;

                foreach (Squid.Scout.ScoutInfo info in Squid.Scout.BestBuy.Scout(upc, zipcode, "50"))
                {
                    final += "<ul>";

                    final += "<li>" + info.Name + "</li>";
                    final += "<li>" + info.Address + "</li>";
                    final += "<li>" + info.City + ", " + info.State + " " + info.ZIPCode + "</li>";
                    final += "<li>Price:" + info.Price + "</li>";

                    final += "</ul>";
                }

                return final;
            }

            return "";
        }
    }

    public enum ProductType
    {
        milkshake,
        amazon,
        bestbuy,
        custom,
        etsy
    }
    
    [Serializable]
    public class AddWishModel
    {
        // Universal
        public ProductType ProductType { get; set; }
        public Guid Wishlu { get; set; }
        public string WishluName { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }

        // Milkshake Item
        public Guid Id { get; set; }

        // Amazon Product
        public string ASIN { get; set; }

        // BestBuy Product
        public string SKU { get; set; }

        // Etsy Product
        // ENHANCEMENT: Etsy API integration

        // Custom Item
        public string Url { get; set; }                        
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Image { get; set; }
    }

    [Serializable]
    public class WishInputModel
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
        public string Description { get; set; }
        public string UPC { get; set; }

        public string Date { get; set; }
        public string Wishloops { get; set; }
    }

    [Serializable]
    public class WishNameModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    public class WishUrlModel
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
    }

    [Serializable]
    public class WishQuantityModel
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
    }

    [Serializable]
    public class WishPurchasedModel
    {
        public Guid Id { get; set; }
        public int Purchased { get; set; }
    }

    [Serializable]
    public class WishPriceModel
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
    }

    [Serializable]
    public class WishDescriptionModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
    }

    [Serializable]
    public class WishNotesModel
    {
        public Guid Id { get; set; }
        public string Notes { get; set; }
    }

    [Serializable]
    public class WishImageModel
    {
        public Guid Id { get; set; }
        public string Image { get; set; }
    }

    [Serializable]
    public class WishLikeModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }

    [Serializable]
    public class AssignWishModel
    {
        public Guid Wish { get; set; }
        public Guid Wishlu { get; set; }
        public string WishluName { get; set; }
    }
        
    [Serializable]
    public class AssignWishesModel
    {
        public List<Guid> Wishes { get; set; }
        public Guid Wishlu { get; set; }
        public string WishluName { get; set; }
    }

    [Serializable]
    public class CopyWishesModel
    {
        public List<Guid> Wishes { get; set; }
        public Guid Wishlu { get; set; }
        public string WishluName { get; set; }
    }

    [Serializable]
    public class DeleteWishesModel
    {
        public List<Guid> Wishes { get; set; }        
    }

    [Serializable]
    public class PromiseWishModel
    {
        public Guid Id { get; set; }
        public string Date { get; set; }
    }
    
    [Serializable]
    public class CancelPromiseModel
    {
        public Guid Id { get; set; }        
    }

    [Serializable]
    public class SuggestWishModel
    {
        public Guid Id { get; set; }
        public List<Guid> Friends { get; set; }
    }

    [Serializable]
    public class RateWishModel
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
    }
}