using PhoneKit.Framework.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowernApp
{
    /// <summary>
    /// The application settings.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Setting for wheter a 24h clock will be displayed.
        /// </summary>
        public static readonly StoredObject<bool> Is24HourClock = new StoredObject<bool>("is24Hr", true);

        /// <summary>
        /// Setting for whether the vibration is enabled.
        /// </summary>
        public static readonly StoredObject<bool> EnableVibration = new StoredObject<bool>("enableVibration", true);

        public static readonly StoredObject<DateTime> AlarmTime = new StoredObject<DateTime>("alarmTime", DateTime.MinValue);
        public static readonly StoredObject<DateTime> AlarmStartTime = new StoredObject<DateTime>("alarmStartTime", DateTime.MinValue);
    }
}
