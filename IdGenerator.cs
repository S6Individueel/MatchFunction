using System;
using System.Collections.Generic;
using System.Text;

namespace MatchFunction
{
    public static class IdGenerator
    {
        private static Random _random = new Random();
        private static char[] _base36chars =
                                    "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                                    .ToCharArray();
        /// <summary>
        /// Creates a random unique string sequence
        /// 6 chars in base 62 will give you 62^6 unique IDs = 56,800,235,584 (56+ billion) At 10k IDs per day you will be ok for 5+ million days / ~13698 years
        /// 6 chars in base 36 will give you 36^6 unique IDs = 2,176,782,336 (2+ billion) At 10k IDs per day you will be ok for ~217+ thousand days/ ~596 years
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetBase36(int length)
            {
                var sb = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                    sb.Append(_base36chars[_random.Next(36)]);

                return sb.ToString();
            }
    }
}
