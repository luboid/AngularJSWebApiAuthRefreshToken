using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Runtime.InteropServices;

namespace AngularJSAuthRefreshToken.Web
{
    public static class Culture
    {
        public sealed class Win32
        {
            private Win32()
            { }

            public static readonly int BG_LOCALE = 1026;

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int SetThreadLocale(int locale);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int GetThreadLocale();
        }

        public static CultureInfo Create(string culture)
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture(culture);

            DateTimeFormatInfo di = ci.DateTimeFormat;

            di.DateSeparator = ".";
            di.FullDateTimePattern = "dd.MM.yyyy HH:mm:ss";
            di.LongDatePattern = "dd.MM.yyyy";
            di.LongTimePattern = "HH:mm:ss";
            di.MonthDayPattern = "dd MMMM";
            di.PMDesignator = "";
            di.ShortDatePattern = "dd.M.yyyy";
            di.ShortTimePattern = "HH:mm";
            di.TimeSeparator = ":";
            di.YearMonthPattern = "MMMM yyyy";

            NumberFormatInfo ni = ci.NumberFormat;

            ni.CurrencyDecimalDigits = 2;
            ni.CurrencyDecimalSeparator = ".";
            ni.CurrencyGroupSeparator = " ";
            ni.CurrencySymbol = string.Empty;

            ni.NumberDecimalDigits = 2;
            ni.NumberDecimalSeparator = ".";
            ni.NumberGroupSeparator = " ";

            ni.PercentDecimalDigits = 2;
            ni.PercentDecimalSeparator = ".";
            ni.PercentGroupSeparator = " ";

            return ci;
        }

        public static void Set(string culture)
        {
            CultureInfo ci = Create(culture);

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            //if (ci.LCID != Win32.GetThreadLocale())
            //{
            //    Win32.SetThreadLocale(ci.LCID);
            //}
        }

        public static void Set()
        {
            Set("bg-BG");
        }
    }
}
