// NOTE: This code is outdated, was never working, and requires a new Memcached wrapper to function
// NOTE: This code is being replaced with a Redis-based cache
// TODO: Implement Redis cache

//using Enyim.Caching;
//using Enyim.Caching.Memcached;
//using Enyim.Caching.Configuration;

namespace Squid.Database
{
    public static class Cache
    {
        /*
        private static MemcachedClient memcached;

        static Cache()
        {
            //LogManager.AssignFactory(new EnyimLogger());
            
            //memcached = new MemcachedClient(new MemcachedClientConfiguration());           
            
        }

        public static T Get<T>(string key)
        {
            Logger.Log("memcached:Get - Key: " + key);
            return memcached.Get<T>(key);
        }

        public static void Set(string key, object value)
        {
            Logger.Log("memcached:Set - Key: " + key + " Value: " + value.GetType().Name);
            if (memcached.Store(StoreMode.Set, key, value))
            {
                Logger.Log("memcached:Set was successful.");
            }
            else
            {
                Logger.Log("memcached:Set was unsuccessful.");                
            }
        }

        public static void Add(string key, object value)
        {
            Logger.Log("memcached:Add - Key: " + key + " Value: " + value.GetType().Name);
            if (memcached.Store(StoreMode.Add, key, value))
            {
                Logger.Log("memcached:Add was successful.");
            }
            else
            {
                Logger.Log("memcached:Add was unsuccessful.");
            }
        }

        public static void Replace(string key, object value)
        {
            Logger.Log("memcached:Replace - Key: " + key + " Value: " + value.GetType().Name);
            if (memcached.Store(StoreMode.Replace, key, value))
            {
                Logger.Log("memcached:Replace was successful.");
            }
            else
            {
                Logger.Log("memcached:Replace was unsuccessful.");
            }
        }

        public static void Remove(string key)
        {
            Logger.Log("memcached:Remove - Key: " + key);
            if (memcached.Remove(key))
            {
                Logger.Log("memcached:Remove was successful.");
            }
            else
            {
                Logger.Log("memcached:Remove was unsuccessful.");
            }
        }    
         * */
    }
}
