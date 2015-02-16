using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Squid.Database;
using Squid.Log;

namespace Fantine
{
    public static class Housekeeping
    {
        public static void Run()
        {
            try
            {                
                EmailManager.Poll();
            }
            catch { }

            try
            {
                WishManager.Poll();
            }
            catch { }

            try
            {
                Thread.Sleep(100);
            }
            catch { }
        }
    }
}
