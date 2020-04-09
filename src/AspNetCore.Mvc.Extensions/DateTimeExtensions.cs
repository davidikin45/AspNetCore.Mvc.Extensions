﻿using System;
using System.Globalization;
using TimeZoneConverter;
using TimeZoneNames;

namespace AspNetCore.Mvc.Extensions
{
    //Notes:
    //CurrentThread.CurrentCulture has nothing to do with TimeZones. ToLocalTime and ToUniversalTime are based on the system timzezone. Accessed by TimeZoneInfo.Local. Control Panel > Date and Time > Change time zone ...
    //Local.TimeZoneInfo.Local.GetUtcOffset(flight.DepartureDate)
    //Unspecified/Local + Local.TimeZoneInfo.Local.GetUtcOffset(Unspecified/Local) = UTC

    //if you call ToLocalTime using an "unspecified" DateTime, then the value will be treated as if it were in UTC.
    //if you call ToUniversalTime using an "unspecified" DateTime, then the value will be treated as if it were in the system local time zone.

    //DateTime allows. Unspecified (Exactly what user entered), Local or UTC only.
    //DateTimeOffset is good for arithmetic operations. Unspecified + Offset = UTC.

    //MVC - RFC 3339
    //In MVC when DateTimeOffset is binded it is essentialy the same as Unspecified (Assumed as Local) > ToUtc() > UTC. The OffSet stored in the Db is the offset from server timezone, who cares? https://docs.microsoft.com/en-us/dotnet/standard/datetime/converting-between-time-zones
    //In MVC when DateTime is binded it is always Unspecified in format yyyy-MM-ddTHH:mm:ss.fff

    //API - ISO 8601
    //JsonSerializerSettings.DateParseHandling = DateParseHandling.DateTime is only for JObjects.
    //In API when DateTimeOffset is binded it is essentialy the same as Unspecified (Assumed as Local) > ToUtc() > UTC. https://docs.microsoft.com/en-us/dotnet/standard/datetime/converting-between-time-zones
    //In API the default JsonSerializerSettings.DateTimeZoneHandling == DateTimeZoneHandling.RoundtripKind //https://www.newtonsoft.com/json/help/html/SerializeDateTimeZoneHandling.htm
    //Unspecified = "2013-01-21T00:00:00"
    //Utc = "2013-01-21T00:00:00Z"
    //Local = "2013-01-21T00:00:00+01:00" (Anything with +HH:mm)

    //EF always returns dates as unspecified so when they get serialized for API they look like this 2013-01-21T00:00:00.

    //Outcome:
    //DateTimeOffset allows roundtrip for a user and there values. To a users values to other users still need timezone.
    //RoundtripKind allows all values a user sees to be as expteced.
    //[DataType("datetime-local")]


    //TZConvert.GetTimeZoneInfo works cross platform but TimeZoneInfo.FindSystemTimeZoneById does not.
    public static class DateTimeExtensions
    {

        public static string ToISO8601(this DateTime dt)
        {
            return dt.ToUniversalTime().ToString("s") + "Z";
        }

        public static string ToDaysTil(this DateTime value, DateTime endDateTime)
        {
            var ts = new TimeSpan(endDateTime.Ticks - value.Ticks);
            var delta = ts.TotalSeconds;
            if (delta < 60)
            {
                return ts.Seconds == 1 ? "one second" : ts.Seconds + " seconds";
            }
            if (delta < 120)
            {
                return "a minute";
            }
            if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes";
            }
            if (delta < 5400) // 90 * 60
            {
                return "an hour";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return ts.Days + " days";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month" : months + " months";
            }
            var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year" : years + " years";
        }

        public static DateTime ToStartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime ToEndOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }

        #region Input
        /// <summary>
        /// Local to UTC.
        /// </summary>
        public static DateTime LocalToUtc(this DateTime unspecifiedasLocalOrLocalDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedasLocalOrLocalDateTime);
        }

        /// <summary>
        /// Unspecifieds to UTC.
        /// </summary>
        public static DateTime UnspecifiedToUtc(this DateTime unspecifiedDateTime, string timeZoneId)
        {
            var istTZ = TZConvert.GetTimeZoneInfo(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedDateTime, istTZ);
        }
        #endregion

        #region Output DateTime Display
        //https://stackoverflow.com/questions/15302083/timezone-abbreviations
        //TimeZoneNames library
        //AUS Eastern Standard Time - AEST
        //The UTC to convert. This must be a DateTime value whose Kind property is set to Unspecified or Utc.

        /// <summary>
        /// Assumes Unspecified = Utc
        /// </summary>
        public static string UtcToTimeZone(this DateTime unspecifiedasUtcOrutcDateTime, string timeZoneId)
        {
            var istTZ = TZConvert.GetTimeZoneInfo(timeZoneId);
            string lang = CultureInfo.CurrentCulture.Name;   // example: "en-US"
            var abbreviations = TZNames.GetAbbreviationsForTimeZone(timeZoneId, lang);
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(unspecifiedasUtcOrutcDateTime, istTZ);
            return String.Format("{0} ({1})", convertedTime.ToShortDateString(), convertedTime.IsDaylightSavingTime() ? abbreviations.Daylight : abbreviations.Standard);
        }

        /// <summary>
        /// Assumes Unspecified = Utc
        /// </summary>
        public static string UtcToTimeZoneNoAbbreviation(this DateTime unspecifiedOrutcDateTime, string timeZoneId)
        {
            var istTZ = TZConvert.GetTimeZoneInfo(timeZoneId);
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(unspecifiedOrutcDateTime, istTZ);
            return String.Format("{0}", convertedTime.ToShortDateString());
        }

        /// <summary>
        /// Assumes Unspecified = Local.
        /// </summary>
        public static string LocalOrUtcToTimeZone(this DateTime unspecifiedAsLocalOrLocalOrutcDateTime, string timeZoneId)
        {
            var istTZ = TZConvert.GetTimeZoneInfo(timeZoneId);
            string lang = CultureInfo.CurrentCulture.Name;   // example: "en-US"
            var abbreviations = TZNames.GetAbbreviationsForTimeZone(timeZoneId, lang);
            var convertedTime = TimeZoneInfo.ConvertTime(unspecifiedAsLocalOrLocalOrutcDateTime, istTZ);
            return String.Format("{0} ({1})", convertedTime.ToShortDateString(), convertedTime.IsDaylightSavingTime() ? abbreviations.Daylight : abbreviations.Standard);
        }

        /// <summary>
        /// Assumes Unspecified = Local.
        /// </summary>
        public static string LocalOrUtcToTimeZoneNoAbbreviation(this DateTime unspecifiedAsLocalOrLocalOrutcDateTime, string timeZoneId)
        {
            var istTZ = TZConvert.GetTimeZoneInfo(timeZoneId);
            var convertedTime = TimeZoneInfo.ConvertTime(unspecifiedAsLocalOrLocalOrutcDateTime, istTZ);
            return String.Format("{0}", convertedTime.ToShortDateString());
        }

        #endregion

        #region Input DateTimeOffset
        public static DateTimeOffset ToTimeZone(DateTimeOffset original, string timeZoneId)
        {
            TimeZoneInfo istTZ = TZConvert.GetTimeZoneInfo(timeZoneId);
            DateTimeOffset temp = TimeZoneInfo.ConvertTime(original, istTZ);
            var convertedTime = original
                .Subtract(temp.Offset)
                .ToOffset(temp.Offset);

            return convertedTime;
        }
        #endregion

        #region Output DateTimeOffset
        public static string ToTimeZoneFormatted(DateTimeOffset original, string timeZoneId)
        {
            TimeZoneInfo istTZ = TZConvert.GetTimeZoneInfo(timeZoneId);
            DateTimeOffset temp = TimeZoneInfo.ConvertTime(original, istTZ);
            var convertedTime = original
                .Subtract(temp.Offset)
                .ToOffset(temp.Offset);

            string lang = CultureInfo.CurrentCulture.Name;   // example: "en-US"
            var abbreviations = TZNames.GetAbbreviationsForTimeZone(timeZoneId, lang);

            return String.Format("{0} ({1})", convertedTime.Date.ToShortDateString(), convertedTime.Date.IsDaylightSavingTime() ? abbreviations.Daylight : abbreviations.Standard);
        }

        public static string ToTimeZoneFormattedNoAbbreviation(DateTimeOffset original, string timeZoneId)
        {
            TimeZoneInfo istTZ = TZConvert.GetTimeZoneInfo(timeZoneId);
            DateTimeOffset temp = TimeZoneInfo.ConvertTime(original, istTZ);
            var convertedTime = original
                .Subtract(temp.Offset)
                .ToOffset(temp.Offset);

            string lang = CultureInfo.CurrentCulture.Name;   // example: "en-US"
            var abbreviations = TZNames.GetAbbreviationsForTimeZone(timeZoneId, lang);

            return String.Format("{0}", convertedTime.Date.ToShortDateString());
        }
        #endregion
    }
}