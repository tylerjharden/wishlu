using Newtonsoft.Json;
using Squid.Database;
using Squid.Log;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Squid.Messages
{
    public enum MessageType
    {
        None = 0, // Invalid message
        UserToUser = 1, // Instant Message / Inbox
        SystemToUser = 2, // Internal WishLu programmed messages sent by the system user
        MerchantToUser = 3, // Messages sent from Merchant accounts on WishLu0
        AdToUser = 4, // Advertisements (?)
        Notification = 5, // Some type of limited scope notification
        Reserved1 = 6, // Cake
        Reserved2 = 7, // Cookies
        Reserved3 = 8, // Soda Pop
        Reserved4 = 9, // Porn
        Broadcast = 10, // A message sent to everyone (OMG!)
        UserToGroup = 11, // A message from an individual user sent to a group chat (IM/Thread)
        UserToWishloop = 12, // Wishloop Comment / Discussion Thread
        UserToWishlu = 13, // Wishlu Comment / Discussion Thread
        UserToWish = 14, // Wish Comment / Discussion Thread
        UserToMerchant = 15
    }
    
    public
    class Message : GraphObject
    {
        [JsonProperty("ScopeId")]
        public Guid ScopeId { get; set; }

        [JsonProperty("SenderId")]
        public Guid? SenderId { get; set; }

        [JsonProperty("BodyText")]
        public String BodyText { get; set; }

        [JsonProperty("SubjectText")]
        public String SubjectText { get; set; }

        [JsonProperty("SendTime")]
        public DateTimeOffset? SendTime { get; set; }

        [JsonProperty("ReadTime")]
        public DateTimeOffset? ReadTime { get; set; }

        [JsonProperty("MessageType")]
        public MessageType MessageType { get; set; }

        [JsonProperty("SourceUserId")]
        public Guid? SourceUserId { get; set; }

        [JsonProperty("SourceIp")]
        public string SourceIp { get; set; }

        [JsonProperty("Read")]
        public bool Read { get; set; }

        [JsonProperty("Archived")]
        public bool Archived { get; set; }

        [JsonProperty("Sent")]
        public bool Sent { get; set; }

        [JsonProperty("Deleted")]
        public bool Deleted { get; set; }

        static
        Message()
        {
        }

        public
        Message()
        {
            Id = Guid.Empty;
            SendTime = DateTimeOffset.MinValue;
            MessageType = MessageType.None;
            Read = false;
            Archived = false;
            Deleted = false;
            Sent = false;                        
        }

        public
        void
        PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            //validationErrors.ValidateNotNull("SubjectText", SubjectText);
            validationErrors.ValidateNotNull("BodyText", BodyText);
        }

        public
        void
        CreateSendMessage(MessageScope scope)
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            this.ScopeId = scope.Id;
                        
            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            Logger.Log("Message:CreateMessage() for scope " + this.ScopeId);

            this.Create();
                        
            scope.AddMessage(this);

            this.Send();

            scope.PushMessage(this);
        }

        public void Send()
        {
            Logger.Log("Message:Send() for " + this.Id);

            this.Sent = true;
            this.SendTime = DateTimeOffset.Now;

            this.Set("Sent", true);
            this.Set("SendTime", DateTimeOffset.Now);
        }

        private
        void
        UpdateMessage()
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

        public void MarkAsUnread()
        {
            Logger.Log("Message:MarkAsUnread() for " + this.Id);

            this.Read = false;
            this.Set("Read", false);

            this.ReadTime = DateTimeOffset.MinValue;
            this.Set("ReadTime", DateTimeOffset.MinValue);
        }

        public void Archive()
        {
            Logger.Log("Message:Archive() for " + this.Id);

            this.Archived = true;
            this.Set("Archived", true);
        }

        public void Unarchive()
        {
            Logger.Log("Message:Unarchive() for " + this.Id);

            this.Archived = false;
            this.Set("Unarchived", false);
        }

        public override void Delete()
        {
            Logger.Log("Message:Delete() [EVIL] for " + this.Id);

            // Evil: we don't actually delete things from the database, but we do hide them from users!

            this.Deleted = true;
            this.Set("Deleted", true);

            // If we ever decide to not be evil anymore then we would call this function here...
            //base.Delete();
        }

        public static Message GetMessageByIdInternal(Guid id)
        {
            Logger.Log("static Message:GetMessageByIdInternal() [Bypass Evil] for " + id);

            try
            {
                return Graph.Instance.Cypher
                   .Match("(msg:Message)")
                   .Where((Message msg) => msg.Id == id)
                   .Return(msg => msg.As<Message>())
                   .Results.First();
            }
            catch
            {
                return null;
            }
        }

        public static Message GetMessageById(Guid id)
        {
            Logger.Log("static Message:GetMessageById() for " + id);

            try
            {
                return Graph.Instance.Cypher
                    .Match("(msg:Message)")
                    .Where((Message msg) => msg.Id == id)
                    .AndWhere((Message msg) => msg.Deleted == false)
                    .Return(msg => msg.As<Message>())
                    .Results.First();
            }
            catch
            {
                return null;
            }
        }

        /*public
        static
        Message
        GetNextMessage(DatabaseInstance databaseInstance)
        {
           QueryStringBuilder queryStringBuilder = RecordHelper.GetQueryStringBuilder("Msg_SendTime < @Now");

           queryStringBuilder.TopCount = 1;

           Message msg = RecordHelper.SearchForRecord(databaseInstance,queryStringBuilder,
                                                      DatabaseHelper.CreateParameter("@Now",DateTime.Now));
           return msg;
        }*/

        //---------------------------------------------------------------------------------------------//
    }
    //================================================================================================//
}

