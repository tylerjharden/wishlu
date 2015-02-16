using Squid.Users;
using Squid.Wishes;
using System;
using System.Collections.Generic;

namespace Disco.ViewModels
{    
    public class InboxViewModel
    {
        public List<Squid.Messages.Message> messages { get; set; }
    }

    public class MessageViewModel
    {
        public User Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTimeOffset? SendTime { get; set; }
    }

    public class SendMessageModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
        
    public class LoopViewModel
    {
        public List<Wishloop> wishLoops { get; set; }
        public Wishloop wishLoop { get; set; }
        public List<User> wishLoopMembers { get; set; }
    }

    public class WishLuShareViewModel
    {
        public List<Wishlu> wishLuList { get; set; }
        public List<Wish> wishList { get; set; }
    }
}