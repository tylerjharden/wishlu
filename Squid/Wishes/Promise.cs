using Newtonsoft.Json;
using Squid.Database;
using Squid.Housekeeping;
using Squid.Users;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Squid.Wishes
{
    public enum PromiseStatus
    {
        Promised = 1,
        Confirmed = 2,
        Revealed = 3,
        Canceled = 4
    }

    public class Promise : GraphAssociation<User, Wish>
    {
        [JsonProperty("WishId")]
        public Guid WishId { get; set; }

        [JsonProperty("UserId")]
        public Guid UserId { get; set; }

        [JsonProperty("PromiseStatus")]
        public PromiseStatus PromiseStatus { get; set; }

        [JsonProperty("RevealDate")]
        public DateTimeOffset RevealDate { get; set; }

        static Promise()
        { }

        public Promise()
        {
            Id = Guid.Empty;
            Type = GraphAssociationType.PROMISED;
        }

        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            Debug.Assert(validationErrors != null);

            validationErrors.ValidateNotNull("WishId", WishId);
            validationErrors.ValidateNotNull("UserId", UserId);
        }

        public void CreatePromise()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            this.Alpha = User.GetUserById(UserId);
            this.Omega = Wish.GetWishById(WishId);

            this.Type = GraphAssociationType.PROMISED;

            this.CreatedOn = DateTimeOffset.Now;
            this.LastModifiedOn = DateTimeOffset.Now;

            this.CreateExclusive();

            if (this.RevealDate.Date > DateTimeOffset.Now.Date) // only create reveal tasks for promises not made for today or in the past
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
            }
        }

        internal static Promise CreatePromiseForWish(Guid wishId, Guid userId, DateTimeOffset revealDate)
        {
            Promise promise = new Promise()
            {
                WishId = wishId,
                UserId = userId,
                PromiseStatus = PromiseStatus.Promised,
                RevealDate = revealDate
            };

            promise.CreatePromise();
            return promise;
        }

        internal static Promise CreateSelfConfirmedPromiseForWish(Guid wishId, Guid userId)
        {
            Promise promise = new Promise()
            {
                WishId = wishId,
                UserId = userId,
                PromiseStatus = PromiseStatus.Promised,
                RevealDate = DateTimeOffset.MinValue
            };

            List<ValidationError> validationErrors = new List<ValidationError>();

            promise.Id = Guid.NewGuid();

            promise.PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            promise.Alpha = User.GetUserById(promise.UserId);
            promise.Omega = Wish.GetWishById(promise.WishId);

            promise.Type = GraphAssociationType.PROMISED;

            promise.CreatedOn = DateTimeOffset.Now;
            promise.LastModifiedOn = DateTimeOffset.Now;

            promise.PromiseStatus = PromiseStatus.Confirmed;

            promise.CreateExclusive();

            return promise;
        }

        public static Promise GetPromiseById(Guid promiseId)
        {
            Promise promise = Graph.Instance.Cypher
                 .Match("(user:User)-[r:PROMISED]->(wish:Wish)")
                 .Where((Promise r) => r.Id == promiseId)
                 .Return(r => r.As<Promise>())
                 .Results.First();

            if (promise != null)
                return promise;

            String message = ("Service.Promise.PromiseNotFound");

            throw new ItemNotFoundException(message);
        }

        private void ThrowPromiseConfirmedException()
        {
            String message = ("Service.Promise.PromiseConfirmedError");

            throw new PromiseConfirmedException(message);
        }

        private void ThrowPromiseCanceledException()
        {
            String message = ("Service.Pledge.PromiseCanceledError");

            throw new PromiseCanceledException(message);
        }

        public void Confirm()
        {
            switch (PromiseStatus)
            {
                case PromiseStatus.Confirmed: ThrowPromiseConfirmedException(); break;
                case PromiseStatus.Canceled: ThrowPromiseCanceledException(); break;
            }

            this.PromiseStatus = PromiseStatus.Confirmed;

            this.Update();
        }

        public void Cancel()
        {
            switch (PromiseStatus)
            {
                case PromiseStatus.Confirmed: ThrowPromiseConfirmedException(); break;
                case PromiseStatus.Canceled: ThrowPromiseCanceledException(); break;
            }

            this.PromiseStatus = PromiseStatus.Canceled;

            this.Update();
        }

        public void Reveal()
        {
            switch (PromiseStatus)
            {
                case PromiseStatus.Confirmed: ThrowPromiseConfirmedException(); break;
                case PromiseStatus.Canceled: ThrowPromiseCanceledException(); break;
            }

            this.PromiseStatus = PromiseStatus.Revealed;

            this.Update();
        }

        public Wish GetWish()
        {
            return Wish.GetWishById(WishId);
        }

        public User GetPromiser()
        {
            return User.GetUserById(UserId);
        }

        public User GetReceiver()
        {
            return User.GetUserById(GetWish().UserId);
        }

        public static List<Promise> GetConfirmedPromisesForWish(Guid wishId)
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(user:User)-[r:PROMISED]->(wish:Wish)")
                 .Where((Promise r) => r.PromiseStatus.ToString() == PromiseStatus.Confirmed.ToString())
                 .AndWhere((Promise r) => r.WishId == wishId)
                 .Return(r => r.As<Promise>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Promise>();
            }
        }

        public static List<Promise> GetPromisesForWish(Guid wishId)
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(user:User)-[r:PROMISED]->(wish:Wish)")
                 .Where((Promise r) => r.PromiseStatus.ToString() == PromiseStatus.Promised.ToString())
                 .AndWhere((Promise r) => r.WishId == wishId)
                 .Return(r => r.As<Promise>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Promise>();
            }
        }

        public static List<Promise> GetRevealedPromisesForWish(Guid wishId)
        {
            try
            {
                return Graph.Instance.Cypher
                     .Match("(user:User)-[r:PROMISED]->(wish:Wish)")
                     .Where((Promise r) => r.PromiseStatus.ToString() == PromiseStatus.Revealed.ToString())
                     .AndWhere((Promise r) => r.WishId == wishId)
                     .Return(r => r.As<Promise>())
                     .Results.ToList();
            }
            catch
            {
                return new List<Promise>();
            }
        }

        public static List<Promise> GetAllPromisesForWish(Guid wishId)
        {
            try
            {
                return Graph.Instance.Cypher
                 .Match("(user:User)-[r:PROMISED]->(wish:Wish)")
                 .Where((Promise r) => r.WishId == wishId)
                 .Return(r => r.As<Promise>())
                 .Results.ToList();
            }
            catch
            {
                return new List<Promise>();
            }
        }
    }
}