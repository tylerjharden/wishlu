using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Butler.Models;

namespace Butler.Controllers
{
    [Authorize]
    public class WishlusController : ApiController
    {
        // GET v1.0/wishlus
        public IEnumerable<Squid.Wishes.Wishlu> Get()
        {
            string id = User.Identity.GetUserId();
                        
            return Squid.Wishes.Wishlu.GetUsersWishLus(Guid.Parse(id)).AsEnumerable();
        }

        // GET v1.0/wishlus/{guid}
        public Squid.Wishes.Wishlu Get(int id)
        {
            return null;
        }

        // POST api/wishlus
        public void Post([FromBody]Squid.Wishes.Wishlu value)
        {
        }

        // PUT api/wishlus/{guid}
        public void Put(int id, [FromBody]Squid.Wishes.Wishlu value)
        {
        }

        // DELETE api/wishlus/{guid}
        public void Delete(int id)
        {
        }
    }
}
