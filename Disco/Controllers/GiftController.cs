using Squid;
using Squid.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Controllers
{
    [Authorize]
    public class GiftController : BaseController
    {
        public ActionResult Index(int sort_others = 0, int sort_me = 0)
        {
            var current = GetCurrentUser();

            List<Squid.Wishes.Gift> others = current.GetGiftsGiven();

            switch (sort_others)
            {
                case 0:
                default:
                    others = others.OrderByDescending(x => x.CreatedOn).ToList();
                    break;

                case 1:
                    others = others.OrderBy(x => x.CreatedOn).ToList();
                    break;
            }
                        
            List<Squid.Wishes.Gift> my = current.GetGiftsReceived();

            switch (sort_others)
            {
                case 0:
                default:
                    my = my.OrderByDescending(x => x.CreatedOn).ToList();
                    break;

                case 1:
                    my = my.OrderBy(x => x.CreatedOn).ToList();
                    break;
            }

            return View(new GiftsModel { Others = others, Me = my, SortOthers = sort_others, SortMe = sort_me });
        }

        public ActionResult Other()
        {
            return View();
        }

        public ActionResult My()
        {
            return View();
        }

        public ActionResult View(Guid id)
        {
            return View();
        }
                
        [HttpPost]
        public ActionResult Give(GiveGiftModel model)
        {
            if (model.Id == null || model.Id == Guid.Empty)
                return JsonResponse(false, "Please provide an item to gift.");

            Squid.Wishes.Wish wish = null;

            try
            {
                wish = Squid.Wishes.Wish.GetWishById(model.Id);
            }
            catch
            {
                return JsonResponse(false, "The item you selected does not exist.");
            }

            if (CurrentUser.HasGifted(wish))
                return JsonResponse(false, "You have already gifted this item.");

            if (!wish.IsGiftable)
            {
                return JsonResponse(false, "This item has already been confirmed as gifted.");
            }

            try
            {
                Squid.Wishes.Gift gift = wish.Gift(GetCurrentUserId(), DateTimeOffset.MinValue);

                if (model.Purchased)
                {
                    gift.Purchase();
                    return JsonResponse(true, "Your gift has been recorded as purchased.");
                }

                return JsonResponse(true, "You have chosen to buy this gift later. You can buy in one click on wishlu from your <a href='/gifts'>gifts page</a>, or from this page directly via the buy now button. If you wish to buy from another site or in-store, use the same options to mark your gift as purchased. We won't notify the user of your gift until it has been marked as purchased.");
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return JsonResponse(false, "There was an error gifting this item.");
            }
        }

        [HttpPost]
        public ActionResult Reveal(RevealGiftModel model)
        {
            if (model.Id == null || model.Id == Guid.Empty)
                return JsonResponse(false, "Please specify a gift to reveal.");

            Squid.Wishes.Gift g = null;
            try
            {
                g = Squid.Wishes.Gift.GetGiftById(model.Id);
            }
            catch (ItemNotFoundException)
            {
                return JsonResponse(false, "The specified gift does not exist.");
            }

            if (g.Status == Squid.Wishes.GiftStatus.Revealed || g.Status == Squid.Wishes.GiftStatus.Confirmed)
                return JsonResponse(false, "This gift has already been revealed.");
                        
            if (g.Status != Squid.Wishes.GiftStatus.Purchased)
                return JsonResponse(false, "You can only reveal gifts that have been purchased.");
                        
            if (g.GiverId != GetCurrentUserId())
                return JsonResponse(false, "You can only reveal gifts you've given.");

            g.Reveal();

            return JsonResponse(true, "The specified gift has been revealed.");
        }

        [HttpPost]
        public ActionResult Purchase (PurchaseGiftModel model)
        {
            if (model.Id == null || model.Id == Guid.Empty)
                return JsonResponse(false, "Please specify a gift to confirm.");

            Squid.Wishes.Gift g = null;
            try
            {
                g = Squid.Wishes.Gift.GetGiftById(model.Id);
            }
            catch (ItemNotFoundException)
            {
                return JsonResponse(false, "The specified gift does not exist.");
            }

            if (g.Status == Squid.Wishes.GiftStatus.Revealed || g.Status == Squid.Wishes.GiftStatus.Confirmed)
                return JsonResponse(false, "This gift has already been revealed.");

            if (g.Status == Squid.Wishes.GiftStatus.Purchased)
                return JsonResponse(false, "You can only reveal gifts that have been reserved.");

            if (g.GiverId != GetCurrentUserId())
                return JsonResponse(false, "You can only reveal gifts you've given.");

            g.Purchase();

            return JsonResponse(true, "The specified gift has been recorded as purchased.");
        }
                
        [HttpPost]
        public ActionResult Cancel(CancelGiftModel model)
        {
            if (model.Id == null || model.Id == Guid.Empty)
                return JsonResponse(false, "Please specify a gift to cancel.");

            Squid.Wishes.Wish wish = null;

            try { wish = Squid.Wishes.Wish.GetWishById(model.Id); }
            catch
            {
                return JsonResponse(false, "The specified item does not exist.");
            }

            if (wish.UserId == GetCurrentUserId())
                return JsonResponse(false, "You may only cancel gifts you have given.");
                        
            Squid.Wishes.Gift gift = null;
            try
            {
                gift = wish.GetGift(GetCurrentUserId());

                if (gift.Status == Squid.Wishes.GiftStatus.Confirmed)
                    return JsonResponse(false, "You cannot cancel a gift that has been confirmed.");

                try
                {
                    gift.Cancel();

                    return JsonResponse(true, "Your gift for this item has been canceled.");
                }
                catch
                {
                    return JsonResponse(false, "There was an error canceling your gift.");
                }
            }
            catch
            {
                return JsonResponse(false, "You have not gifted this item.");
            }
        }
                
        [HttpPost]
        public ActionResult Confirm(ConfirmGiftModel model)
        {
            if (model.Id == null || model.Id == Guid.Empty)
                return JsonResponse(false, "Please specify a gift to confirm.");

            Squid.Wishes.Gift g = null;
            try
            {
                g = Squid.Wishes.Gift.GetGiftById(model.Id);
            }
            catch (ItemNotFoundException)
            {
                return JsonResponse(false, "The specified gift no longer exists.");
            }

            Squid.Wishes.Wish wish = g.GetWish();

            if (wish.UserId != GetCurrentUserId())
                return JsonResponse(false, "You can only mark your own items as received.");

            g.Confirm();

            return JsonResponse(true, "You have confirmed this gift.");
        }
    }

    [Serializable]
    public class GiftsModel
    {
        public List<Squid.Wishes.Gift> Others { get; set; }
        public List<Squid.Wishes.Gift> Me { get; set; }
        public int SortOthers { get; set; }
        public int SortMe { get; set; }
    }

    [Serializable]
    public class GiveGiftModel
    {
        public Guid Id { get; set; }
        public bool Purchased { get; set; }
        public string Date { get; set; }
    }

    [Serializable]
    public class RevealGiftModel
    {
        public Guid Id { get; set; }
    }

    [Serializable]
    public class PurchaseGiftModel
    {
        public Guid Id { get; set; }
    }

    [Serializable]
    public class CancelGiftModel
    {
        public Guid Id { get; set; }
    }

    [Serializable]
    public class ConfirmGiftModel
    {
        public Guid Id { get; set; }
    }    
}