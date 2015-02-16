// DEPRECATED AND SAVED
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Squid.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squid.Messages
{
    // Extend the core SignalR Hub functionality to suit our needs at WishLu
    [HubName("wishluHub")]
    public class WishLuHub : Hub
    {
        public override Task OnConnected()
        {
            /*Guid userId = Guid.Parse(Context.User.Identity.Name);
            string connectionId = Context.ConnectionId;

            User user = User.GetUserById(userId);

            lock (user.ConnectionIds)
            {
                // Add this connection to the user's connection id list
                user.ConnectionIds.Add(connectionId);

                // Commit connection Id to the graph
                user.Update();

                // If this is the first connection of a user to the WishLu App...
                if (user.ConnectionIds.Count == 1)
                {
                    // Tell user's friends but not the user (all connections by the same user), that said user has connected
                    // Clients.Others.userConnected(user.FullName);
                    List<string> ids = User.GetUsersFriendsConnectionIds(userId);

                    Clients.Clients(ids).userConnected(user.FullName);
                }
            */
                return base.OnConnected();
            //}
        }
                        
        public override Task OnDisconnected(bool stopCalled)
        {
            /*Guid userId = Guid.Parse(Context.User.Identity.Name);
            string connectionId = Context.ConnectionId;

            User user = User.GetUserById(userId);

            if (user != null)
            {
                lock (user.ConnectionIds)
                {
                    // Remove this connection id
                    user.ConnectionIds.RemoveWhere(cid => cid.Equals(connectionId));

                    // Commit new list to graph
                    user.Update();

                    if (!user.ConnectionIds.Any())
                    {
                        // The user has disconnected all sessions
                        // The user is now "offline"
                        //Clients.Others.userDisconnected(user.FullName);

                        List<string> ids = User.GetUsersFriendsConnectionIds(userId);

                        Clients.Clients(ids).userDisconnected(user.FullName);
                    }
                }
            }*/

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
    }
}
