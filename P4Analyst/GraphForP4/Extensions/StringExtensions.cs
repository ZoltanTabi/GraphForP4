using System.Collections.Generic;

namespace GraphForP4.Extensions
{
    public static class StringExtensions
    {
        public static List<int> AllIndexesOf(this string str, string value)
        {
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        public static string SubStringByEndChar(this string str, int startIndex, char end)
        {
            return str.Substring(startIndex, str.IndexOf(end, startIndex) - startIndex + 1);
        }
    }

}
