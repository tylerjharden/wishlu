using Newtonsoft.Json;
using Squid.Database;
using Squid.Log;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Squid.Messages
{
    public class Comment : SocialGraphObject
    {
        [JsonProperty("AuthorId")]
        public Guid AuthorId { get; set; }

        [JsonProperty("Body")]
        public String Body { get; set; }

        public Comment()
        {
            Id = Guid.Empty;
            AuthorId = Guid.Empty;
            Body = String.Empty;
            
            Logger.Log("new Comment()");
        }        
    }
}
