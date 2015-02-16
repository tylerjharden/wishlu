using Disco.ViewModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Controllers
{    
    public class InboxController : BaseController
    {
        [Authorize]
        public ActionResult Index()
        {
            /*List<Squid.Messages.Message> messagesList = Squid.Users.User.GetUsersMessages(GetCurrentUserId());
            
            var viewModel = new InboxViewModel
            {
                messages = messagesList
            };*/

            //dynamic model = new ExpandoObject();
            var model = Squid.Messages.IM.GetConversations(GetCurrentUserId());

            return View("Index", model);
        }

        // Starts a new conversation
        [Authorize]
        [HttpPost]
        public ActionResult Create(CreateConversationModel model)
        {
            // Check if a conversation already exists
            var convos = Squid.Messages.IM.GetConversations(GetCurrentUserId());
            if (convos.Count(x => x.User2 == model.UserId) > 0)
            {
                // Grab the IM thread
                var c = convos.Where(x => x.User2 == model.UserId).Single();

                // Create the reply message
                Squid.Messages.Message m = new Squid.Messages.Message();
                m.SenderId = GetCurrentUserId();
                m.SourceUserId = GetCurrentUserId();
                m.BodyText = model.Message;
                m.MessageType = Squid.Messages.MessageType.UserToUser;
                m.SendTime = DateTime.Now;
                m.CreateSendMessage(c);

                return Json(new { result = true, id = c.Id });
            }

            // Create the IM conversation
            var convo = new Squid.Messages.IM();
            convo.Id = Guid.NewGuid();
            convo.User1 = GetCurrentUserId();
            convo.User2 = model.UserId;
            convo.Create();            
            convo.AddReceiver(GetCurrentUserId());
            convo.AddReceiver(model.UserId);

            // Create the first message
            var msg = new Squid.Messages.Message();                        
            msg.SenderId = GetCurrentUserId();
            msg.SourceUserId = GetCurrentUserId();            
            msg.BodyText = model.Message;
            msg.MessageType = Squid.Messages.MessageType.UserToUser;            
            msg.SendTime = DateTime.Now;
            msg.CreateSendMessage(convo);

            return Json(new { result = true, id = convo.Id });
        }

        [Authorize]
        public ActionResult Conversation(Guid id)
        {
            Squid.Messages.IM model = Squid.Messages.IM.GetConversationById(id);

            model.MarkAsRead(GetCurrentUserId()); // the user has loaded the conversation, mark all messages as read

            return PartialView("Conversation", model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Reply(ConversationReplyModel model)
        {
            // Grab the IM thread
            var convo = Squid.Messages.IM.GetConversationById(model.Id);

            // Create the reply message
            Squid.Messages.Message msg = new Squid.Messages.Message();
            msg.SenderId = GetCurrentUserId();
            msg.SourceUserId = GetCurrentUserId();
            msg.BodyText = model.Message;
            msg.MessageType = Squid.Messages.MessageType.UserToUser;
            msg.SendTime = DateTime.Now;
            msg.CreateSendMessage(convo);

            return Json(new { result = true, id = convo.Id });
        }

        [Authorize]
        public
        ActionResult
        Send()
        {
            return View("Send");
        }
                
        //---------------------------------------------------------------------------------------------//
        [Authorize]
        [HttpPost]
        public
        ActionResult
        SendMessage(SendMessageModel message)
        {            
            Guid to = Guid.Parse(message.To);
            String subject = message.Subject;
            String body = message.Body;

            Squid.Messages.MessageManager.SendUserToUserMessage(to, GetCurrentUserId(), subject, body);
                        
            if (Request.IsAjaxRequest())
            {
                return Json(true);
            }

            List<Squid.Messages.Message> messagesList = Squid.Users.User.GetUsersMessages(GetCurrentUserId());

            var viewModel = new InboxViewModel
            {
                messages = messagesList
            };

            Squid.Users.User touser = Squid.Users.User.GetUserById(to);

            TempData["SuccessMessage"] = "Your message to " + touser.FullName + " has been successfully sent!";

            return View("Index", viewModel);
        }
        //---------------------------------------------------------------------------------------------//
        [Authorize]
        public
        ActionResult
        Delete(Guid id)
        {
            Squid.Messages.Message msg = Squid.Messages.Message.GetMessageById(id);

            msg.DeleteAll();

            TempData["SuccessMessage"] = "Message has been deleted.";

            return RedirectToAction("Index", "Inbox");
        }
        //---------------------------------------------------------------------------------------------//
        [Authorize]
        public
        ActionResult
        Archive(Guid id)
        {            
            Squid.Messages.Message msg = Squid.Messages.Message.GetMessageById(id);

            msg.Archive();

            TempData["SuccessMessage"] = "Message has been archived.";

            List<Squid.Messages.Message> messagesList = Squid.Users.User.GetUsersMessages(GetCurrentUserId());
            var viewModel = new InboxViewModel
            {
                messages = messagesList
            };
            return View("Index", viewModel);
        }
        //---------------------------------------------------------------------------------------------//
        [Authorize]
        public
        ActionResult
        Unarchive(Guid id)
        {
            Squid.Messages.Message msg = Squid.Messages.Message.GetMessageById(id);

            msg.Unarchive();

            TempData["SuccessMessage"] = "Message has been unarchived and moved back to the inbox.";

            List<Squid.Messages.Message> messagesList = Squid.Users.User.GetUsersMessages(GetCurrentUserId());
            var viewModel = new InboxViewModel
            {
                messages = messagesList
            };
            return View("Index", viewModel);
        }
        //---------------------------------------------------------------------------------------------//
        [Authorize]
        public
        ActionResult
        Unread(Guid id)
        {            
            Squid.Messages.Message msg = Squid.Messages.Message.GetMessageById(id);

            msg.MarkAsUnread();

            TempData["SuccessMessage"] = "Message has been marked as unread.";

            List<Squid.Messages.Message> messagesList = Squid.Users.User.GetUsersMessages(GetCurrentUserId());
            var viewModel = new InboxViewModel
            {
                messages = messagesList
            };
            return View("Index", viewModel);
        }
        //---------------------------------------------------------------------------------------------//
        [Authorize]        
        public
        ActionResult
        Read(Guid id)
        {
            List<Squid.Messages.Message> messagesList = Squid.Users.User.GetUsersMessages(GetCurrentUserId());

            Squid.Messages.Message msg = messagesList.First(x => x.Id == id);

            if (msg == null)
            {
                TempData["ErrorMessage"] = "Could not find specified message!";

                var errModel = new InboxViewModel
                {
                    messages = messagesList
                };
                return View("Index", errModel);
            }

            msg.MarkAsRead();

            var viewModel = new MessageViewModel
            {                
                Body = msg.BodyText,
                Subject = msg.SubjectText,
                SendTime = msg.SendTime
            };

            try
            {
                if (msg.MessageType == Squid.Messages.MessageType.UserToUser)
                    viewModel.Sender = Squid.Users.User.GetUserById(msg.SenderId ?? Guid.Empty);
            }
            catch { }

            return View("View", viewModel);
        }
    }

    [Serializable]
    public class CreateConversationModel
    {
        public Guid UserId { get; set; }
        public string Message { get; set; }
    }

    [Serializable]
    public class ConversationReplyModel
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
    }
}