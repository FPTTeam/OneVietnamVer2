using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneVietnam
{
    public static class Utilities
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength)+"...";
        }
        public static string GetTimeInterval(DateTimeOffset createdDate)
        {
            DateTime start = DateTime.Now;
            // Do some work
            TimeSpan timeDiff = DateTime.Now - createdDate;
            string result;
            if (timeDiff.TotalDays >= 30)
            {
                //Tailm :delete "Tu"
                result = createdDate.Date.ToShortDateString();
            }
            else if (timeDiff.TotalHours >= 24)
            {
                result = Math.Round(timeDiff.TotalDays)+ " ngày trước ";
            }
            else if (timeDiff.TotalMinutes >= 60)
            {
                result = Math.Round(timeDiff.TotalHours)+ " giờ trước ";
            }
            else if (timeDiff.TotalSeconds >= 60)
            {
                result = Math.Round(timeDiff.TotalMinutes)+" phút trước";
            }
            else
            {
                result = " Vừa xong ";
            }
            return result;
        }

        public static TimeSpan ConvertTimeZoneOffSetToTimeSpan(int zone)
        {
            int h = zone/60;
            int m = zone - h*60;
            return new TimeSpan(h,m,0);
        }

    }
}