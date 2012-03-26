using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace EB.Crime.Map.Views
{
    public class ViewUtil
    {
        public static string GetIconName(int categoryId)
        {
            switch (categoryId)
            {
                case 2: return "thefth";
                case 8:
                case 9: return "crimescene";
                case 11: return "rape";
                case 13: return "gun";
                case 14: return "revolution";
                case 5: return "fire";
                case 4: return "icyroad";
                case 3: return "bank";
                default: return "accident";
            }
        }
        
        public static IEnumerable<DateTime> GetDateRange(DateTime StartingDate, DateTime EndingDate)
        {
            while (StartingDate <= EndingDate)
            {
                yield return StartingDate;
                StartingDate = StartingDate.AddDays(1);
            }
        }

        public static int FromBase26(string base26)
        {
            var digits = base26.ToCharArray();
            int res = (int)(digits[0] - 'a') + 1;

            for (int i = 1; i < digits.Length; i++)
            {
                if (i != digits.Length )
                {
                    // not at last digit
                    res = res * 26;
                }
                res += (int)(digits[i] - 'a') + 1;
            }
            return res;
        }

        public static string ToBase26(int number)
        {
            string converted = "";
            number -= 1;
            // Repeatedly divide the number by 26 and convert the
            // remainder into the appropriate letter.
            do
            {
                int remainder = number % 26;
                converted = (char)(remainder + 'a') + converted;
                number = number / 26 - 1;
            } while (number >= 0);

            return converted;
        }


    }

    public static class Extensions
    {
        public static string Truncate(this string str, int maxlength, bool addDots)
        {
            if (str == null || str.Length <= maxlength)
            {
                return str;
            }

            if (addDots && maxlength >= 3)
            {
                string shortened = str.Substring(0, maxlength - 3);
                if (!shortened.EndsWith("..."))
                {
                    shortened += "...";
                }
                return shortened;
            }
            else
            {
                return str.Substring(0, maxlength);
            }
        }


        /// <summary>
        /// This is lifted from a Stackoverflow post
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToUrlFriendly(this string s)
        {
            // make it all lower case
            string title = s.ToLower();
            // replace ae, oe aa
            title = title.Replace("æ", "ae").Replace("å", "aa").Replace("ø", "oe");
            // remove entities
            title = Regex.Replace(title, @"&\w+;", "");
            // remove anything that is not letters, numbers, dash, or space
            title = Regex.Replace(title, @"[^a-z0-9\-\s]", "");
            // replace spaces
            title = title.Replace(' ', '-');
            // collapse dashes
            title = Regex.Replace(title, @"-{2,}", "-");
            // trim excessive dashes at the beginning
            title = title.TrimStart(new[] { '-' });
            // if it's too long, clip it
            if (title.Length > 80)
                title = title.Substring(0, 79);
            // remove trailing dashes
            title = title.TrimEnd(new[] { '-' });
            return title;
        }

    }
}