using Squid.Log;
using Squid.Users;
using System;

namespace Squid.Messages
{
    public class MessageManager
    {
        static MessageManager() { }

        public MessageManager() { }

        public static void
        SendMessage(Message msg, MessageScope scope)
        {
            Logger.Log("static MessageManager:SendMessage()");

            msg.SendTime = DateTime.Now;
            msg.CreateSendMessage(scope);
        }

        public static void SendUserToUserMessage(Guid userId, Guid senderId, String subjectText, String bodyText)
        {
            Logger.Log("static MessageManager:SendUserToUserMessage() - Sender: " + senderId + " Receiver: " + userId);

            Message msg = new Message();
            User user = User.GetUserById(userId);
            MessageScope scope = user.GetInbox();

            //msg.UserId = userId; // No longer use a destination UserId as we pipe through a MessageScope between Source and Destination User
            msg.SenderId = senderId;
            msg.SourceUserId = senderId;
            msg.SubjectText = subjectText;
            msg.BodyText = bodyText;
            msg.MessageType = MessageType.UserToUser;

            SendMessage(msg, scope);
        }

        public static void SendSystemToUserMessage(Guid userId, String subjectText, String bodyText)
        {
            Logger.Log("static MessageManager:SendSystemToUserMessage() - Sender: " + "(System)" + " Receiver: " + userId);

            Message msg = new Message();

            User user = User.GetUserById(userId);
            MessageScope scope = user.GetInbox();

            //msg.UserId = userId;
            msg.SenderId = new Guid("ffa6c36a-cb7f-41f6-bb3d-a5a44f1ef5cd"); // "WishLu System" User ID
            msg.SubjectText = subjectText;
            msg.BodyText = bodyText;
            msg.MessageType = MessageType.SystemToUser;

            SendMessage(msg, scope);
        }

        // TODO: Methods for sending other message types!
    }
}