using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowernApp.Napping
{
    /// <summary>
    /// Calculates the nap time.
    /// </summary>
    public class NapTimeCalculator
    {
        /// <summary>
        /// The base time, which is usually the current time.
        /// </summary>
        private DateTime _now;

        /// <summary>
        /// Creates a new NapTimeCalculator instance.
        /// </summary>
        /// <param name="baseTime">The base time. Typically now. Required for better unit testing.</param>
        public NapTimeCalculator(DateTime baseTime)
        {
            _now = baseTime;
        }

        /// <summary>
        /// Calculates the time.
        /// </summary>
        /// <param name="hours">The length of the nap in hours.</param>
        /// <param name="minutes">The length of the nap in minutes.</param>
        /// <returns>The nap time in minutes.</returns>
        public int calculateTime(string hours, string minutes)
        {
            int h = 0;
            int min = 30;
            int.TryParse(minutes, out min);
            int.TryParse(hours, out h);
            int totalMins = 60 * h + min;

            return totalMins;
        }

        /// <summary>
        /// Calculates the time until the given 24-h-clock-time.
        /// </summary>
        /// <param name="hours">The target clock time hours.</param>
        /// <param name="minutes">The traget clock time minutes.</param>
        /// <returns>The calculated minutes.</returns>
        public int calculateTimeUntilTimeAs24Clock(string hours, string minutes)
        {
            int h = _now.Hour;
            int min = _now.Minute + 30;

            if (min > 59)
            {
                min -= 60;
                h++;
                if (h > 23)
                    h = 0;
            }

            int.TryParse(minutes, out min);
            int.TryParse(hours, out h);

            var sleepTo = new DateTime(_now.Year, _now.Month, _now.Day, h, min, 0);

            if (sleepTo < _now)
            {
                sleepTo = sleepTo.AddDays(1);
            }

            int totalMins = (int)(sleepTo - _now).TotalMinutes + 1;
            return totalMins;
        }

        /// <summary>
        /// Calculates the time until the given US clock time as 12h clock.
        /// </summary>
        /// <param name="hours">The hours of the target time.</param>
        /// <param name="minutes">The minutes of the target time.</param>
        /// <param name="identifier12h">The time identifier as AM or PM.</param>
        /// <returns>The calculated time.</returns>
        public int calculateTimeUntilTimeAs12ClockUS(string hours, string minutes, string identifier12h)
        {
            int h = _now.Hour;
            int min = _now.Minute + 30;

            if (min > 59)
            {
                min -= 60;
                h++;
                if (h > 23)
                    h = 0;
            }

            int.TryParse(minutes, out min);
            int.TryParse(hours, out h);

            var sleepTo = new DateTime(_now.Year, _now.Month, _now.Day, h, min, 0);

            if (identifier12h == "PM")
            {
                sleepTo = sleepTo.AddHours(12);
            }

            if (sleepTo < _now)
            {
                sleepTo = sleepTo.AddDays(1);
            }

            int totalMins = (int)(sleepTo - _now).TotalMinutes + 1;
            return totalMins;
        }

        /// <summary>
        /// Calculates the time until the given target time as German 12h clock time text.
        /// </summary>
        /// <param name="hours">The target time hours as 12h clock time</param>
        /// <param name="minText">The minutes text.</param>
        /// <returns>The calculated time.</returns>
        public int calculateTimeUntilTimeAs12ClockDEText(string hours, string minText)
        {
            int minOffset = -30; // "halb" as default
            int h = _now.Hour + 1;
            if (h > 23)
                h = 0;

            int.TryParse(hours, out h);

            switch (minText)
            {
                case "viertel vor":
                    minOffset = -15;
                    break;
                case "viertel nach":
                    minOffset = 15;
                    break;
                case "halb":
                    minOffset = -30;
                    break;
            }

            var sleepTo = new DateTime(_now.Year, _now.Month, _now.Day, h, 0, 0);
            sleepTo = sleepTo.AddMinutes(minOffset);

            if (sleepTo < _now)
            {
                sleepTo = sleepTo.AddHours(12);
            }
            // do this a second time, bacause sleepTo could still be before NOW
            if (sleepTo < _now)
            {
                sleepTo = sleepTo.AddHours(12);
            }

            int totalMins = (int)(sleepTo - _now).TotalMinutes + 1;
            return totalMins;
        }

        /// <summary>
        /// Calculates the time until the given GB clock time in 12h clock.
        /// </summary>
        /// <param name="hours">The target time hours.</param>
        /// <param name="minText">The min text in multiples of five from five to thirty/half.</param>
        /// <param name="relative">The relative of the full hour (from/to).</param>
        /// <returns>The calculated time.</returns>
        public int calculateTimeUntilTimeAs12ClockGBText(string hours, string minText, string relative)
        {
            int factor = 1;
            int minOffset = 30; // "half" as default
            int h = _now.Hour + 1;
            if (h > 23)
                h = 0;

            int.TryParse(hours, out h);

            switch (relative)
            {
                case "past":
                    factor = 1;
                    break;
                case "to":
                    factor = -1;
                    break;
            }

            switch (minText)
            {
                case "five":
                    minOffset = 5;
                    break;
                case "ten":
                    minOffset = 10;
                    break;
                case "fifteen":
                case "quarter":
                    minOffset = 15;
                    break;
                case "twenty":
                    minOffset = 20;
                    break;
                case "twenty-five":
                    minOffset = 25;
                    break;
                case "thirty":
                case "half":
                    minOffset = 30;
                    break;
            }

            minOffset = minOffset * factor;

            var sleepTo = new DateTime(_now.Year, _now.Month, _now.Day, h, 0, 0);
            sleepTo = sleepTo.AddMinutes(minOffset);

            while (sleepTo < _now)
            {
                sleepTo = sleepTo.AddHours(12);
            }

            int totalMins = (int)(sleepTo - _now).TotalMinutes + 1;
            return totalMins;
        }

        /// <summary>
        /// Calculates the time until the given 12h clock GB time in full hours.
        /// </summary>
        /// <param name="hours">The given full hours of the next 12h.</param>
        /// <returns>The calculated time.</returns>
        public int calculateTimeUntilTimeAs12ClockGB(string hours)
        {
            int h = _now.Hour + 1;
            if (h > 23)
                h = 0;

            int.TryParse(hours, out h);

            var sleepTo = new DateTime(_now.Year, _now.Month, _now.Day, h, 0, 0);

            while (sleepTo < _now)
            {
                sleepTo = sleepTo.AddHours(12);
            }

            int totalMins = (int)(sleepTo - _now).TotalMinutes + 1;
            return totalMins;
        }
    }
}
