using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace POCGraphFreeBusyMeetings.Utilities
{
    public static class ExtendedText
    {
        public static string FormatWithInvariantCulture(this string format, params object[] args)
        {
            return FormatWith(format, CultureInfo.InvariantCulture, args);
        }
        public static string FormatWith(this string format, IFormatProvider formatProvider, params object[] args)
        {
            return string.Format(formatProvider, format, args);
        }
    }
}
