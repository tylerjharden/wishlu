using Newtonsoft.Json;
using Squid.Database;
using Squid.Log;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Squid.Messages
{
    public enum CommentType
    {
        comment,
        image,
        wish,
        link
    }

    public class Comment : SocialGraphObject
    {
        [JsonProperty("commenter")]
        public Commenter Commenter { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("deletable_by_me")]
        public bool DeletableByMe
        {
            get
            {
                return this.Commenter.Id == User.CurrentUserId;
            }
        }

        public Comment()
        {
            Id = Guid.Empty;
            Commenter = new Messages.Commenter();
            Text = String.Empty;            
        }       
 
        public Comment(Guid userId, string text)
        {
            Id = Guid.NewGuid();
            Commenter = new Commenter(userId);
            Text = text;
        }
    }

    public enum CommenterType
    {
        user,
        store,
        wishlu
    }

    public class Commenter
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("type")]
        public CommenterType Type { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        public Commenter()
        {
            Id = Guid.Empty;
            Username = string.Empty;
            FullName = string.Empty;
            Type = CommenterType.user;
            ImageUrl = string.Empty;
        }

        public Commenter(Guid userId)
        {
            var user = User.GetUserById(userId);

            Id = user.Id;
            Username = user.Handle;
            FullName = user.FullName;
            Type = CommenterType.user;
            ImageUrl = user.Image;
        }

        public Commenter(User user)
        {

        }
    }
}
