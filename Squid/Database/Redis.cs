using Schloss.Data.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squid.Database
{
    public static class Redis
    {
        private static ConnectionMultiplexer redis;
        private static ISubscriber sub;
        private static IDatabase db;

        static Redis()
        {
            // our Redis instance is hosted on the milkshake server, which is always IP 10.0.0.40, redis default port 6379
            redis = ConnectionMultiplexer.Connect("10.0.0.40:6379");
            redis.PreserveAsyncOrder = true; // we want the speed of async, but for something like logs, which are time critical, we want things in order
                        
            sub = redis.GetSubscriber();
            db = redis.GetDatabase();
        }

        // Pushes a logging message to a redis pub/sub channel that Bubble is configured to listen on
        public static void PushLogMessage(string message)
        {
            sub.Publish("squid_log", message, CommandFlags.FireAndForget);
        }
    }
}
