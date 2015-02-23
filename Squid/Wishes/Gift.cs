using Newtonsoft.Json;
using Squid.Database;
using Squid.Housekeeping;
using Squid.Messages;
using Squid.Users;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Squid.Wishes
{
    public enum GiftStatus
    {
        Reserved = 1,
        Purchased = 2,
        Revealed = 3,
        Confirmed = 4,
        Canceled = 5
    }

    public class Gift : GraphObject
    {
        [JsonProperty("WishId")]
        public Guid WishId { get; set; }

        [JsonProperty("GiverId")]
        public Guid GiverId { get; set; }

        [JsonProperty("ReceiverId")]
        public Guid ReceiverId { get; set; }

        [JsonProperty("Status")]
        public GiftStatus Status { get; set; }

        [JsonProperty("RevealDate")]
        public DateTimeOffset RevealDate { get; set; }

        static Gift()
        { }

        public Gift()
        {
            Id = Guid.Empty;            
        }

        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            validationErrors.ValidateNotNull("WishId", WishId);
            validationErrors.ValidateNotNull("GiverId", GiverId);
            validationErrors.ValidateNotNull("ReceiverId", ReceiverId);
        }

        public override void Create()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();
            
            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();
                        
            this.CreatedOn = DateTimeOffset.Now;
            this.LastModifiedOn = DateTimeOffset.Now;
                        
            base.Create();

            Graph.Instance.Cypher
                .Match("(g:User)", "(gift:Gift)", "(r:User)", "(w:Wish)")
                .Where((User g) => g.Id == this.GiverId)
                .AndWhere((Gift gift) => gift.Id == this.Id)
                .AndWhere((Wish w) => w.Id == this.WishId)
                .AndWhere((User r) => r.Id == this.ReceiverId)
                .Create("(g)-[:GIVER]->(gift)")
                .Create("(gift)-[:RECEIVER]->(r)")
                .Create("(gift)<-[:GIFT]-(w)")
                .ExecuteWithoutResults();

            /*if (this.RevealDate.Date > DateTimeOffset.Now.Date) // only create reveal tasks for promises not made for today or in the past
            {
                // Reveal Task
                RevealTask rt = new RevealTask();
                rt.PromiseId = this.Id;
                rt.Date = this.RevealDate;

                DateTimeOffset ld = rt.Date;

                if (ld < DateTimeOffset.Now)
                    ld = DateTimeOffset.Now;

                Graph.Instance.Cypher
                    .Create("(n:RevealTask" + ld.ToString("MMddyy") + " {p})")
                    .WithParam("p", rt)
                    .ExecuteWithoutResults();
            }*/
        }

        internal static Gift CreateGiftForWish(Guid wishId, Guid giverId, Guid receiverId, DateTimeOffset revealDate)
        {
            Gift gift = new Gift()
            {
                WishId = wishId,
                GiverId = giverId,
                ReceiverId = receiverId,
                Status = GiftStatus.Reserved,
                RevealDate = revealDate
            };

            gift.Create();
            return gift;
        }
                               
        public static Gift GetGiftById(Guid giftId)
        {
            Gift gift = Graph.Instance.Cypher
                 .Match("(g:Gift)")
                 .Where((Gift g) => g.Id == giftId)
                 .Return(g => g.As<Gift>())
                 .Results.First();

            if (gift != null)
                return gift;

            String message = ("Service.Gift.GiftNotFound");

            throw new ItemNotFoundException(message);
        }
       
        public void Reserve()
        {

        }

        public void Purchase()
        {
            if (Status != GiftStatus.Reserved)
            {
                throw new OperationNotAllowedException("A gift can only be purchased if it is currently reserved.");
            }

            this.Status = GiftStatus.Purchased;

            this.Update();
        }

        public void Reveal()
        {
            if (Status != GiftStatus.Purchased)
            {
                throw new OperationNotAllowedException("A gift can only be revealed if it has been purchased.");
            }

            this.Status = GiftStatus.Revealed;

            this.Update();

            // Push Notification
            Wish wish = this.GetWish();
            Wishlu wishlu = Wishlu.GetWishLuById(wish.GetAssignmentId());
            User giver = this.GetGiver();

            Notification n = new Notification();
            n.UserId = this.ReceiverId;
            n.SenderId = this.GiverId;
            n.NotificationType = NotificationType.Info;
            n.Content = giver.FullName + " has gifted you " + wish.Name + ". When you receive it, go to your " + wishlu.Name + " wishlu to confirm as received, or click this notification.";
            n.Url = "/i/" + this.WishId;
            n.CreateNotification();

            new Mail.MailController().RevealEmail(this).Deliver();
        }

        public void Confirm()
        {
            if (Status != GiftStatus.Revealed)
            {
                throw new OperationNotAllowedException("A gift can only be confirmed if it is currently revealed. Gifts that have not been reserved, purchased, and revealed, or gifts that have been canceled or are already confirmed cannot be confirmed again.");
            }

            this.Status = GiftStatus.Confirmed;                        
            this.Update();

            Wish w = this.GetWish();
            w.Purchased = w.Purchased + 1;
            w.Update();

            // Push Notification
            Notification n = new Notification();
            n.UserId = this.GiverId;
            n.SenderId = this.ReceiverId;
            n.NotificationType = NotificationType.Info;
            n.Content = User.GetUserFullName(this.ReceiverId) + " has confirmed receiving your gift of " + w.Name + ".";
            n.Url = "/i/" + this.WishId;
            n.CreateNotification();
        }

        public void Cancel()
        {
            switch (Status)
            {
                case GiftStatus.Confirmed:
                    throw new OperationNotAllowedException("This gift has already been confirmed. It cannot be canceled."); 
                    
                case GiftStatus.Canceled:
                    throw new OperationNotAllowedException("This gift has already been canceled.");                    
            }

            this.Status = GiftStatus.Canceled;

            this.Update();

            User giver = this.GetGiver();
            Wish w = this.GetWish();

            // Push Notification
            Notification n = new Notification();
            n.UserId = this.ReceiverId;
            n.SenderId = this.GiverId;
            n.NotificationType = NotificationType.Info;
            n.Content = giver.FullName + " has canceled their promise to give you " + w.Name + ".";
            n.Url = "/i/" + this.WishId;
            n.CreateNotification();
        }
                
        public Wish GetWish()
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(g:Gift)-[:GIFT]-(w:Wish)")
                     .Where((Gift g) => g.Id == this.Id)
                     .Return(w => w.As<Wish>())
                     .Results.First();
            }
            catch
            {
                return null;
            }
        }

        public User GetGiver()
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(g:Gift)<-[:GIVER]-(u:User)")
                     .Where((Gift g) => g.Id == this.Id)
                     .Return(u => u.As<User>())
                     .Results.First();
            }
            catch
            {
                return null;
            }
        }

        public User GetReceiver()
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(g:Gift)-[:RECEIVER]->(u:User)")
                     .Where((Gift g) => g.Id == this.Id)
                     .Return(u => u.As<User>())
                     .Results.First();
            }
            catch
            {
                return null;
            }
        }
                                                
        public static List<Gift> GetAllGiftsForWish(Guid wishId)
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(g:Gift)-[:GIFT]->(w:Wish)")
                 .Where((Wish w) => w.Id == wishId)
                 .Return(g => g.As<Gift>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Gift>();
            }
        }
    }
}