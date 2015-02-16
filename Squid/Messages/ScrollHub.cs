// DEPRECATED AND SAVED
namespace Squid.Messages
{
    /*public class ScrollHub : WishLuHub
    {
        public void NewScrollEntry(string message)
        {
            if (String.IsNullOrEmpty(message) || String.IsNullOrWhiteSpace(message))
                return;

            Guid userId = Guid.Parse(Context.User.Identity.Name);

            User user = User.GetUserById(userId);

            Logger.Log("ScrollHub:NewScrollEntry() - UserId: " + user.Id + " FullName: " + user.FullName + " Message: " + message);

            ScrollStore store = user.GetScrollStore();

            Scribe post = new Scribe();
            post.ScribeType = "Status";
            post.Content = message;

            store.Add(post);

            PostInfo pi = new PostInfo();
            pi.Content = message;
            pi.UserName = user.FullName;
            pi.Posted = DateTime.Now.ToString();
            pi.ProfileUrl = user.ImageUrl;
            pi.UserId = userId.ToString();
            pi.ScribeId = post.Id.ToString();

            Clients.All.addNewScrollEntry(pi);            
        }

        public void NewComment(Guid id, string comment)
        {
            Guid userId = Guid.Parse(Context.User.Identity.Name);

            User user = User.GetUserById(userId);

            Logger.Log("ScrollHub:NewComment() - UserId: " + user.Id + " FullName: " + user.FullName + " ScribeId: " + id);

            Scribe post = Scribe.GetScribeById(id);

            post.AddComment(userId, comment);
        }

        public void LikeScribe(Guid id)
        {
            Guid userId = Guid.Parse(Context.User.Identity.Name);

            User user = User.GetUserById(userId);

            Logger.Log("ScrollHub:LikeScribe() - UserId: " + user.Id + " FullName: " + user.FullName + " ScribeId: " + id);

            Scribe post = Scribe.GetScribeById(id);

            post.Like(userId);

            LikeInfo li = new LikeInfo();
            li.ScribeId = post.Id.ToString();
            li.LikeString = CalculateLikeString(post, Guid.Empty);
            li.Like = "o";

            Clients.AllExcept(Context.ConnectionId).onScribeLiked(li);

            li.LikeString = CalculateLikeString(post, userId);
            li.Like = "t";

            Clients.Client(Context.ConnectionId).onScribeLiked(li);
        }

        public void UnlikeScribe(Guid id)
        {
            Guid userId = Guid.Parse(Context.User.Identity.Name);

            User user = User.GetUserById(userId);

            Logger.Log("ScrollHub:UnlikeScribe() - UserId: " + user.Id + " FullName: " + user.FullName + " ScribeId: " + id);

            Scribe post = Scribe.GetScribeById(id);

            post.Unlike(userId);

            LikeInfo li = new LikeInfo();
            li.ScribeId = post.Id.ToString();
            li.LikeString = CalculateLikeString(post, Guid.Empty);
            li.Like = "o";

            Clients.AllExcept(Context.ConnectionId).onScribeLiked(li);

            li.Like = "f";
            li.LikeString = CalculateLikeString(post, userId);
            
            Clients.Client(Context.ConnectionId).onScribeLiked(li);           
        }

        private string CalculateLikeString(Scribe post, Guid userId)
        {
            List<User> likes = post.GetLikes();

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

    public class PostInfo
    {
        public string Content { get; set; }
        public string UserName { get; set; }
        public string Posted { get; set; }

        public string ScribeId { get; set; }

        public string ProfileUrl { get; set; }
        public string UserId { get; set; }
    }

    public class LikeInfo
    {
        public string ScribeId { get; set; }
        public string LikeString { get; set; }
        public string Like { get; set; }
    }*/
}
