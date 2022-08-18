using System;

namespace Artemkv.Journey3.Connector
{
    public static class DateUtil
    {
        public static bool IsSameYear(this DateTime d1, DateTime d2)
        {
            if (d1 == null)
            {
                return false;
            }

            return d1.Year == d2.Year;
        }

        public static bool IsSameMonth(this DateTime d1, DateTime d2)
        {
            if (d1 == null)
            {
                return false;
            }

            return d1.Year == d2.Year && d1.Month == d2.Month;
        }

        public static bool IsSameDay(this DateTime d1, DateTime d2)
        {
            if (d1 == null)
            {
                return false;
            }

            return d1.Year == d2.Year && d1.Month == d2.Month && d1.Day == d2.Day;
        }

        public static bool IsSameHour(this DateTime d1, DateTime d2)
        {
            if (d1 == null)
            {
                return false;
            }

            return d1.Year == d2.Year &&
                d1.Month == d2.Month &&
                d1.Day == d2.Day &&
                d1.Hour == d2.Hour;
        }
    }
}
