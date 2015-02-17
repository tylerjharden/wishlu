using Squid.Users;
using Squid.Wishes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Linq;

namespace Squid.Messages
{
    public class WishHub : WishLuHub
    {        
        public void NewComment(Guid id, string comment)
        {
            Guid userId = Guid.Parse(Context.User.Identity.Name);
            User user = User.GetUserById(userId);
                        
            Wish wish = Wish.GetWishById(id);

            wish.AddComment(userId, comment);
        }

        public void Like(Guid id)
        {
            Guid userId = Guid.Parse(Context.User.Identity.Name);
            User user = User.GetUserById(userId);
                        
            Wish wish = Wish.GetWishById(id);

            wish.Like(userId);

            // Like string for everyone else
            LikeInfo li = new LikeInfo();
            li.Id = wish.Id.ToString();
            li.LikeString = CalculateLikeString(wish, Guid.Empty);
            li.Like = "o";

            Clients.AllExcept(Context.ConnectionId).onLike(li);

            // Like string for user who sent it
            li.LikeString = CalculateLikeString(wish, userId);
            li.Like = "t";

            Clients.Client(Context.ConnectionId).onLike(li);
        }

        public void Unlike(Guid id)
        {
            Guid userId = Guid.Parse(Context.User.Identity.Name);

            User user = User.GetUserById(userId);
                        
            Wish wish = Wish.GetWishById(id);

            wish.Unlike(userId);

            LikeInfo li = new LikeInfo();
            li.Id = wish.Id.ToString();
            li.LikeString = CalculateLikeString(wish, Guid.Empty);
            li.Like = "o";

            Clients.AllExcept(Context.ConnectionId).onLike(li);

            li.Like = "t";
            li.LikeString = CalculateLikeString(wish, userId);
            
            Clients.Client(Context.ConnectionId).onLike(li);           
        }

        private string CalculateLikeString(Wish wish, Guid userId)
        {
            List<User> likes = wish.GetLikes();

            if (likes.Count == 1)
            {
                if (likes.First().Id == userId)
                {
                    return "You like this post";
                }
                else
                {
                    return likes.First().FullName + " likes this post.";
                }

            }
            else if (likes.Count == 2)
            {
                return likes.First().FullName + " and " + likes.ElementAt(1).FullName + " like this post.";
            }
            else if (likes.Count == 3)
            {
                return likes.First().FullName + ", " + likes.ElementAt(1).FullName + ", and " + likes.ElementAt(2).FullName + " like this post.";
            }
            else if (likes.Count > 3)
            {
                string list = "";
                int likescount = likes.Count - 2;
                string firstname = likes.First().FullName;
                string secondname = likes.ElementAt(1).FullName;

                likes.RemoveRange(0, 2);
                foreach (Squid.Users.User u in likes)
                {
                    list = list + u.FullName + "<br>";
                }

                using (StringWriter stringWriter = new StringWriter())
                {
                    HtmlTextWriter writer = new HtmlTextWriter(stringWriter);
                    writer.AddAttribute("class", "tooltip");
                    writer.AddAttribute("title", list);
                    writer.RenderBeginTag("span");                    
                    writer.Write(likescount + " other people like this post.");
                    writer.RenderEndTag();

                    return firstname + ", " + secondname + ", and " + stringWriter.ToString();
                }
                //return firstname + ", " + secondname + ", and " + "<span class=\"tooltip\" title=\"" + list + "\">" + likescount + " other people like this post.</span>";
            }
            else
            {
                return "No one currently likes this post.";
            }
        }
    }
    
    public class LikeInfo
    {
        public string Id { get; set; }
        public string LikeString { get; set; }
        public string Like { get; set; }
    }
}
