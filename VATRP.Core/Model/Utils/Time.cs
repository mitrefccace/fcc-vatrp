#region copyright
/**
 * Copyright © The MITRE Corporation.
 *
 * This program is licensed under the terms of the GNU General Public License Version 2, as published by the Free Software Foundation. This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  Please see the
 * GNU General Public License for more details.
 
 * This program or code contains content developed by The MITRE Corporation. If this program is used in a deployment or embedded within another project, MITRE requests that you send an email to opensource@mitre.org to let us know where and how this program is being used.
 
 * NOTICE
 * This (software/technical data) was produced for the U. S. Government under Contract Number HHSM-500-2012-00008I, and is subject to Federal Acquisition Regulation Clause 52.227-14, Rights in Data-General. No other use other than that granted to the U. S. Government, or to those acting on behalf of the U. S. Government under that Clause is authorized without the express written permission of The MITRE Corporation. For further information, please contact The MITRE Corporation, Contracts Management Office, 7515 Colshire Drive, McLean, VA  22102-7539, (703) 983-6000.
 */
#endregion

using System;
using System.Diagnostics;

namespace VATRP.Core.Model.Utils
{
    public class Time
    {
        public static readonly DateTime ZeroTime = DateTime.SpecifyKind(DateTime.Parse("1970-01-01 00:00:00"), (DateTimeKind) DateTimeKind.Utc);

        public static long ConvertDateTimeTicksToLong(DateTime dateTime)
        {
            long ticks = 0L;
            try
            {
                TimeSpan span = (TimeSpan) (dateTime - ZeroTime);
                ticks = span.Ticks;
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return ticks;
        }

        public static long ConvertDateTimeToLong(DateTime dateTime)
        {
            long totalMilliseconds = 0L;
            try
            {
                TimeSpan span = (TimeSpan) (dateTime - ZeroTime);
                totalMilliseconds = (long) span.TotalMilliseconds;
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return totalMilliseconds;
        }

        public static string ConvertDateTimeToString(DateTime dateTime)
        {
            return ((long) ConvertDateTimeToLong(dateTime)).ToString();
        }

        public static string ConvertLocalTimeToUtcTime(DateTime localTime)
        {
            DateTime dateTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Utc);
            return ConvertDateTimeToString(dateTime);
        }

        public static DateTime ConvertLongToDateTime(long timeLong)
        {
            DateTime zeroTime = ZeroTime;
            try
            {
                zeroTime = zeroTime.AddMilliseconds((double) timeLong);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return zeroTime;
        }

        public static DateTime ConvertSecondsToDateTime(long timeLong)
        {
            DateTime zeroTime = ZeroTime;
            try
            {
                zeroTime = zeroTime.AddSeconds((double)timeLong);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return zeroTime;
        }

        public static DateTime ConvertStringToDateTime(string timeString)
        {
            try
            {
                return ConvertLongToDateTime(Convert.ToInt64(timeString));
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return ZeroTime;
        }

        public static DateTime ConvertUtcTimeToLocalTime(long utcTime)
        {
            return TimeZoneInfo.ConvertTime(ConvertSecondsToDateTime(utcTime), TimeZoneInfo.Local);
        }

        public static string GetTimeTicksUTCString()
        {
            return ((long) ConvertDateTimeTicksToLong(DateTime.UtcNow)).ToString();
        }

        public static long GetTimeUTCInMilliseconds()
        {
            return ConvertDateTimeToLong(DateTime.UtcNow);
        }

        public static long GetTimeUTCInSeconds()
        {
            long totalSeconds = 0L;
            try
            {
                TimeSpan span = (TimeSpan) (DateTime.UtcNow - ZeroTime);
                totalSeconds = (long) span.TotalSeconds;
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return totalSeconds;
        }

        public static string GetTimeUTCString()
        {
            return ConvertDateTimeToString(DateTime.UtcNow);
        }

        public static int GetTimeZoneInSeconds()
        {
            return (int) TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;
        }
    }
}

