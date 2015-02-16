using System;
using System.Collections.Generic;
using System.Drawing;

namespace Disco.Models
{
    public static class Randomizer
    {
        private static Random randomGenerator = new Random();

        public static int GetInteger(int minimum, int maximum)
        {
            return randomGenerator.Next(minimum, maximum + 1);
        }

        public static List<int> GetIndexArray(int numberOfItems)
        {
            List<int> inOrder = new List<int>();
            for (int i = 0; i < numberOfItems; ++i)
                inOrder.Add(i);

            List<int> randomOrder = new List<int>();
            while (inOrder.Count > 0)
            {
                int index = GetInteger(0, inOrder.Count - 1);
                randomOrder.Add(inOrder[index]);
                inOrder.RemoveAt(index);
            }

            return randomOrder;
        }

        public static long GetLong(long minimum, long maximum)
        {
            long range = maximum - minimum;
            double random = randomGenerator.NextDouble();
            long randomRange = Convert.ToInt64(((double)range) * random);

            return randomRange + minimum;
        }

        public static List<int> GetIntegerUniqueList(int numItems, int minimum, int maximum)
        {
            if ((maximum - minimum + 1) < numItems)
                throw new Exception("You cannot request more items than this list can create.");

            List<int> retList = new List<int>();

            while (retList.Count < numItems)
            {
                int val = GetInteger(minimum, maximum);
                if (!retList.Contains(val))
                    retList.Add(val);
            }
            return retList;
        }

        public static double GetDouble(double minimum, double maximum)
        {
            double range = maximum - minimum;
            double randomNumber = randomGenerator.NextDouble();

            return minimum + (range * randomNumber);
        }

        public static decimal GetDecimal(decimal minimum, decimal maximum)
        {
            return Convert.ToDecimal(GetDouble((double)minimum, (double)maximum));
        }

        public static DateTime? GetDate(bool allowNull, DateTime? minimumDate, DateTime? maximumDate)
        {
            if (allowNull && (randomGenerator.Next(1, 5) == 1))
                return null;

            int daysInThePast = -365;
            int daysInTheFuture = 365;

            if (minimumDate.HasValue)
                daysInThePast = (minimumDate.Value - DateTime.Now).Days;
            if (maximumDate.HasValue)
                daysInTheFuture = (maximumDate.Value - DateTime.Now).Days;

            double whichDay = GetDouble(daysInThePast, daysInTheFuture);

            return DateTime.Now.AddDays(whichDay);
        }

        public static String GetAlphaString(int minChars, int maxChars)
        {
            if (maxChars == 0)
                return String.Empty;
            if (maxChars < minChars)
                return String.Empty;

            int strLength = randomGenerator.Next(minChars, maxChars);
            String retStr = String.Empty;

            for (int i = 0; i < strLength; ++i)
                retStr += GetAlphaChar();
            return retStr;
        }

        public static char GetAlphaChar()
        {
            int retChar = randomGenerator.Next('A', 'Z');

            if (randomGenerator.Next(1, 2) == 1)
                retChar += 32;                       // convert to lower                              //
            return Convert.ToChar(retChar);
        }

        public static String GetEmail()
        {
            return GetAlphaString(5, 10) + "@" + GetAlphaString(5, 10) + ".com";
        }

        public static String GetID()
        {
            return Convert.ToString(randomGenerator.Next(100000, 999999));
        }

        public static byte[] GetByteArray(int? numElements)
        {
            int numberOfElements = numElements.HasValue ? numElements.Value : GetInteger(500, 100000);
            byte[] retArray = new byte[numberOfElements];

            for (int i = 0; i < retArray.Length; ++i)
                retArray[i] = (byte)GetInteger(0, 255);
            return retArray;
        }

        public static T GetListElement<T>(List<T> list)
        {
            if (list.Count <= 0)
                return default(T);
            else
                return list[GetInteger(0, list.Count - 1)];
        }

        public static Boolean GetBoolean(double chanceForTrue = 0.5d)
        {
            return (GetDouble(0, 1) <= chanceForTrue);
        }

        public static Color GetColor(int? red = null, int? green = null, int? blue = null)
        {
            int realRed = red.GetValueOrDefault(GetInteger(0, 255));
            int realGreen = green.GetValueOrDefault(GetInteger(0, 255));
            int realBlue = blue.GetValueOrDefault(GetInteger(0, 255));

            return Color.FromArgb(realRed, realGreen, realBlue);
        }

        public static String GetImageLocation(bool createImage = false)
        {
            return "../../Images/orderedList" + GetInteger(0, 9).ToString() + ".png";
        }
    }
}