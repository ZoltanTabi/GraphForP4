using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GraphForP4.Helpers
{
    public static class FileHelper
    {
        private static readonly String[] CHARACTERS = { " ", "(", "{", "=" };

        public static string InputClean(string input)
        {
            input = Regex.Replace(input, @"(//(.*?)\r?\n)", " ");
            input = Regex.Replace(input, @"(/\*(.*)\*/)", " ");
            input = Regex.Replace(input, @"(<[^0-9]*>)|([\n\r])", " ");
            input = Regex.Unescape(input);

            return input;
        }

        public static string GetMethod(string input, string firstEqual, char startChar = '{', char endChar = '}')
        {
            var result = String.Empty;
            var count = 0;
            var findStartChar = false;
            var index = -1;

            for (var i = 0; i < CHARACTERS.Length && index == -1; ++i)
            {
                index = input.IndexOf(firstEqual + CHARACTERS[i]);
            }

            for (var i = index; i > -1 && (count != 0 || !findStartChar); ++i)
            {
                if (input[i] == startChar)
                {
                    findStartChar = true;
                    ++count;
                }
                else if (input[i] == endChar)
                {
                    --count;
                }
                if (findStartChar)
                {
                    result += input[i];
                }
            }

            return result;
        }

        public static List<String> SplitAndClean(String input)
        {
            var result = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            result.RemoveAt(0);
            result.RemoveAt(result.Count - 1);

            return result;
        }

        public static string GetIngressControlName(String input)
        {
            var matchString = Regex.Match(input, "V1Switch(.*)main").Value;
            return Regex.Split(matchString, @"\(([^\(]*)\)([^,]*),").Where(x => !string.IsNullOrWhiteSpace(x)).ToList()[2].Trim();
        }
    }
}
