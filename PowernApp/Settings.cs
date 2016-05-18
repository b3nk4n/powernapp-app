﻿using PhoneKit.Framework.Core.Storage;
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
        /// Setting for whether the vibration is enabled.
        /// </summary>
        public static readonly StoredObject<bool> EnableVibration = new StoredObject<bool>("enableVibration", true);

        /// <summary>
        /// The a
        /// </summary>
        public static readonly StoredObject<string> AlarmUriString = new StoredObject<string>("alarmUri", "Assets/Audio/classic.wav");

        /// <summary>
        /// Setting for whether the voice feedback is enabled.
        /// </summary>
        public static readonly StoredObject<bool> EnableVoiceFeedback = new StoredObject<bool>("enableVoiceFeedback", true);

        /// <summary>
        /// Setting for whether the lockscreen should be suppressed.
        /// </summary>
        public static readonly StoredObject<bool> EnableSuppressLockScreen = new StoredObject<bool>("enableSuppressLockScreen", false);

        /// <summary>
        /// Settings for the frist alarm preset.
        /// </summary>
        public static readonly StoredObject<int> AlarmPreset1 = new StoredObject<int>("alarmPreset1", 30);

        /// <summary>
        /// Settings for the frist alarm preset.
        /// </summary>
        public static readonly StoredObject<int> AlarmPreset2 = new StoredObject<int>("alarmPreset2", 90);
    }
}
