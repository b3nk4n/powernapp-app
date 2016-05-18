using System;

namespace PowernApp.Napping
{
    /// <summary>
    /// Calculates the nap time.
    /// </summary>
    public class NapTimeCalculator
    {
        public const string IDENTIFIER12_AM = "AM";
        public const string IDENTIFIER12_PM = "PM";

        public const string DE_MINTEXT_QUARTER_TO = "viertel vor";
        public const string DE_MINTEXT_HALF = "halb";
        public const string DE_MINTEXT_QUARTER_PAST = "viertel nach";

        public const string EN_RELATIVE_TO = "to";
        public const string EN_RELATIVE_PAST = "past";

        public const string EN_MINTEXT_FIVE = "five";
        public const string EN_MINTEXT_TEN = "ten";
        public const string EN_MINTEXT_FIFTEEN = "fifteen";
        public const string EN_MINTEXT_QUARTER = "quarter";
        public const string EN_MINTEXT_TWENTY = "twenty";
        public const string EN_MINTEXT_TWENTYFIVE = "twenty-five";
        public const string EN_MINTEXT_THIRTY = "thirty";
        public const string EN_MINTEXT_HALF = "half";

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
            int min = 0;
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
            int min = _now.Minute;

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

            int totalMins = TimespanToTotalMinutesCeiled(sleepTo - _now);
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
            int min = _now.Minute;

            if (min > 59)
            {
                min -= 60;
                h++;
                if (h > 23)
                    h = 0;
            }

            int.TryParse(minutes, out min);
            int.TryParse(hours, out h);

            // use fallback values when values are negative
            if (h <= 0)
            {
                h = 0;
            }
            if (min <= 0)
            {
                if (h <= 0)
                    min = 20;
                else
                    min = 0;
            }

            var sleepTo = new DateTime(_now.Year, _now.Month, _now.Day, h, min, 0);

            if (identifier12h == IDENTIFIER12_PM)
            {
                sleepTo = sleepTo.AddHours(12);
            }

            if (sleepTo < _now)
            {
                sleepTo = sleepTo.AddDays(1);
            }

            int totalMins = TimespanToTotalMinutesCeiled(sleepTo - _now);
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
                case DE_MINTEXT_QUARTER_TO:
                    minOffset = -15;
                    break;
                case DE_MINTEXT_QUARTER_PAST:
                    minOffset = 15;
                    break;
                case DE_MINTEXT_HALF:
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

            int totalMins = TimespanToTotalMinutesCeiled(sleepTo - _now);
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
                case EN_RELATIVE_PAST:
                    factor = 1;
                    break;
                case EN_RELATIVE_TO:
                    factor = -1;
                    break;
            }

            switch (minText)
            {
                case EN_MINTEXT_FIVE:
                    minOffset = 5;
                    break;
                case EN_MINTEXT_TEN:
                    minOffset = 10;
                    break;
                case EN_MINTEXT_FIFTEEN:
                case EN_MINTEXT_QUARTER:
                    minOffset = 15;
                    break;
                case EN_MINTEXT_TWENTY:
                    minOffset = 20;
                    break;
                case EN_MINTEXT_TWENTYFIVE:
                    minOffset = 25;
                    break;
                case EN_MINTEXT_THIRTY:
                case EN_MINTEXT_HALF:
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

            int totalMins = TimespanToTotalMinutesCeiled(sleepTo - _now);
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

            int totalMins = TimespanToTotalMinutesCeiled(sleepTo - _now);
            return totalMins;
        }

        /// <summary>
        /// Gets the ceiled (rounded up) total minutes of the time span.
        /// </summary>
        /// <param name="timespan">The time span.</param>
        /// <returns>The total minutes of the timespan.</returns>
        private int TimespanToTotalMinutesCeiled(TimeSpan timespan)
        {
            return (int)Math.Ceiling(timespan.TotalMinutes);
        }
    }
}
