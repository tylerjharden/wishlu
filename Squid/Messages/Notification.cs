using Newtonsoft.Json;
using Squid.Database;
using Squid.Log;
using Squid.Users;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Squid.Messages
{
    public enum NotificationType
    {
        Invalid = 0, // Invalid message
        Info = 1, // Simple informational notification
        FriendRequest = 2, // Notification with a friend request        
        SuggestedFriend = 3, // Notification with option to send friend request
        SuggestedWish = 4, // Cookies
        SuggestedProduct = 5, // Soda Pop
        Reserved4 = 6, // Porn
        Broadcast = 10 // A notification sent to everyone (OMG!)        
    }

    public class MappedNotification
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; }

        [JsonProperty("SenderId")]
        public Guid SenderId { get; set; }

        [JsonProperty("UserId")]
        public Guid UserId { get; set; }

        [JsonProperty("Content")]
        public String Content { get; set; }

        [JsonProperty("SendTime")]
        public DateTimeOffset SendTime { get; set; }

        [JsonProperty("ReadTime")]
        public DateTimeOffset ReadTime { get; set; }

        [JsonProperty("NotificationType")]
        public NotificationType NotificationType { get; set; }

        [JsonProperty("Read")]
        public bool Read { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("SenderProfileImage")]
        public string SenderProfileImage { get; set; }

        [JsonProperty("SenderFullName")]
        public string SenderFullName { get; set; }
    }

    public class Notification : GraphObject
    {
        [JsonProperty("SenderId")]
        public Guid SenderId { get; set; }

        [JsonProperty("UserId")]
        public Guid UserId { get; set; }

        [JsonProperty("Content")]
        public String Content { get; set; }

        [JsonProperty("SendTime")]
        public DateTimeOffset SendTime { get; set; }

        [JsonProperty("ReadTime")]
        public DateTimeOffset ReadTime { get; set; }

        [JsonProperty("NotificationType")]
        public NotificationType NotificationType { get; set; }

        [JsonProperty("Read")]
        public bool Read { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        static Notification() { }

        public Notification()
        {
            Id = Guid.Empty;
            SenderId = Guid.Empty;
            SendTime = DateTimeOffset.MinValue;
            NotificationType = NotificationType.Invalid;
            Read = false;
            Url = "#";
        }

        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            validationErrors.ValidateNotNull("Content", Content);
        }

        public void CreateNotification()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            if (NotificationType == NotificationType.Invalid)
                throw new Exception("Invalid notifications cannot be sent!");

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            Logger.Log("Notification:CreateNotification() for user " + this.UserId);

            this.Create();

            this.Push();
        }

        public void Push()
        {
            Logger.Log("Notifications:Push() for " + this.Id);

            this.SendTime = DateTimeOffset.Now;

            this.Set("SendTime", DateTimeOffset.Now);

            User.PushNotification(this);
        }

        private void UpdateMessage()
        {
            this.Update();
        }

        public void MarkAsRead()
        {
            Logger.Log("Message:MarkAsRead() for " + this.Id);

            this.Read = true;
            this.Set("Read", true);

            this.ReadTime = DateTimeOffset.Now;
            this.Set("ReadTime", DateTimeOffset.Now);
        }

        public static Notification GetNotificationById(Guid id)
        {
            Logger.Log("static Notification:GetNotificationById() for " + id);

            try
            {
                return Graph.Instance.Cypher
                    .Match("(n:Notification)")
                    .Where((Notification n) => n.Id == id)
                    .Return(n => n.As<Notification>())
                    .Results.Single();
            }
            catch
            {
                return null;
            }
        }

        public static bool DeleteNotification(Guid userId, Guid senderId)
        {
            try
            {
                Graph.Instance.Cypher
                    .Match("(n:Notification)")
                    .Where((Notification n) => n.SenderId == senderId)
                    .AndWhere((Notification n) => n.UserId == userId)
                    .OptionalMatch("(n)-[r]-()")
                    .Delete("n,r")
                    .ExecuteWithoutResults();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}