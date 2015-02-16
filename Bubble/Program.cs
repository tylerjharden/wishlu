using Schloss.Data.Redis;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Bubble
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionMultiplexer redis;
            ISubscriber sub;

            redis = ConnectionMultiplexer.Connect("10.0.0.40:6379");
            redis.PreserveAsyncOrder = true;

            sub = redis.GetSubscriber();

            sub.Subscribe("squid_log", (channel, message) =>
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss") + ": " + message);
            });
            
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.BufferHeight = Int16.MaxValue - 1;
            Console.BufferWidth = 2048;

            Console.Clear();

            Console.WriteLine(" >> " + "Bubble Logging Server Started");

            while (true)
            {
                Thread.Sleep(1);
            }
        }
    }
}     

