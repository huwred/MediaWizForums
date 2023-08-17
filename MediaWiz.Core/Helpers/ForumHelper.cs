using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MediaWiz.Forums.Helpers
{
    public class ForumHelper
    {
        public static string GenerateUniqueCode(int length)
        {
            char[] chars = "ABCDEF0123456789".ToCharArray();
            byte[] data = new byte[1];
            using (RandomNumberGenerator crypto = RandomNumberGenerator.Create())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[length];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(length);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
        public static string GravatarURL(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                return "https://www.gravatar.com/avatar/00000000000000000000000000000000?d=mm&f=y"; // the gray man...

            //Get email to lower
            var emailToHash = emailAddress.ToLower();

            // Create a new instance of the MD5CryptoServiceProvider object.  
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.  
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(emailToHash));

            // Create a new Stringbuilder to collect the bytes  
            // and create a string.  
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string.  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            var hashedEmail = sBuilder.ToString();  // Return the hexadecimal string.

            //Return the gravatar URL
            return "https://www.gravatar.com/avatar/" + hashedEmail + "?d=mm";
        }


        public static string GetRelativeDate(DateTime date)
        {
            // takes a date and displays a relative thing
            // i.e 10minutes ago, 1 day ago...
            var now = DateTime.Now;

            if (date == DateTime.MinValue)
                return "never";
            var months = date.GetTotalMonthsFrom(now);

            var span = now.Subtract(date);
            if (months > 0)
            {
                if (months == 1)
                {
                    return "last month";
                }
                return $"{months} months ago";
            }
            else if (span.Days > 0)
            {
                if (span.Days == 1)
                {
                    return "yesterday";
                }
                else
                {
                    if (span.Days % 7 > 0)
                    {
                        return $"{span.Days} days ago";
                    }
                    else
                    {
                        return $"{span.Days/7} weeks ago";
                    }
                    
                }
            }
            else if (span.Hours > 0)
            {
                return $"{span.Hours} hour{(span.Hours > 1 ? "s" : "")} ago";
            }
            else if (span.Minutes > 0)
            {
                return $"{span.Minutes} minute{(span.Minutes > 1 ? "s" : "")} ago";
            }
            else
            {
                return "just now";
            }
        }

        public  static Uri GetAbsoluteUri(HttpRequest request)
        {
            //var request =_httpContextAccessor.HttpContext.Request;
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };
            return uriBuilder.Uri;
        }
    }

    public static class DateTimeExtensions
    {
        public static string GetRelativeDate(this DateTime date)
        {
            // takes a date and displays a relative thing
            // i.e 10minutes ago, 1 day ago...
            var now = DateTime.Now;

            if (date == DateTime.MinValue)
                return "never";
            var months = date.GetTotalMonthsFrom(now);

            var span = now.Subtract(date);
            if (months > 0)
            {
                if (months == 1)
                {
                    return "last month";
                }
                return $"{months} months ago";
            }
            else if (span.Days > 0)
            {
                if (span.Days == 1)
                {
                    return "yesterday";
                }
                else
                {
                    if (span.Days % 7 > 0)
                    {
                        return $"{span.Days} days ago";
                    }
                    else
                    {
                        return $"{span.Days/7} weeks ago";
                    }
                    
                }
            }
            else if (span.Hours > 0)
            {
                return $"{span.Hours} hour{(span.Hours > 1 ? "s" : "")} ago";
            }
            else if (span.Minutes > 0)
            {
                return $"{span.Minutes} minute{(span.Minutes > 1 ? "s" : "")} ago";
            }
            else
            {
                return "just now";
            }
        }

        public static int GetTotalMonthsFrom(this DateTime dt1, DateTime dt2)
        {
            DateTime earlyDate = (dt1 > dt2) ? dt2.Date : dt1.Date;
            DateTime lateDate = (dt1 > dt2) ? dt1.Date : dt2.Date;

            // Start with 1 month's difference and keep incrementing
            // until we overshoot the late date
            int monthsDiff = 1;
            while (earlyDate.AddMonths(monthsDiff) <= lateDate)
            {
                monthsDiff++;
            }

            return monthsDiff - 1;
        }

        public static string ToDisplayDate(this DateTime dt)
        {
            return dt.ToLocalTime().ToString(CultureInfo.CurrentUICulture);
        }
    }
}