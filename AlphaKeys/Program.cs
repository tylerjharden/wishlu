using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AlphaKeys
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamWriter keys = new StreamWriter("C:\\alphakeys.txt");
            keys.AutoFlush = true;

            StreamWriter hashes = new StreamWriter("C:\\alphakeys.dat");
            hashes.AutoFlush = true;

            HashAlgorithm algorithm = SHA512.Create();

            HashSet<string> hs = new HashSet<string>();
            for (int i = 0; i < 5000; i++)
            {                
                string newkey = CreateKey();

                if (hs.Contains(newkey))
                {
                    i--;
                    continue;
                }

                hs.Add(newkey);

                byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(newkey));

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }
                
                Console.WriteLine(i + ": " + newkey);
                keys.WriteLine(newkey);
                hashes.WriteLine(sb.ToString());
            }
        }

        static string CreateKey()
        {
            int length = 10;

            const string valid = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ123456789@$";
            StringBuilder res = new StringBuilder();
                                                            
            while (0 < length--)
            {
                var bytes = new byte[1];
                do
                {
                    new RNGCryptoServiceProvider().GetBytes(bytes);
                }
                while (!IsValidIndex(bytes[0], valid.Length));

                res.Append(valid[bytes[0] % valid.Length]);
            }
            return res.ToString();
        }

        private static bool IsValidIndex(byte index, int length)
        {
            int fullSet = Byte.MaxValue / length;

            return index < length * fullSet;
        }
    }
}
