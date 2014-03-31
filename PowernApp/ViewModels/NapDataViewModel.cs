using PhoneKit.Framework.Core.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowernApp.ViewModels
{
    /// <summary>
    /// The napping data for the statistics.
    /// </summary>
    public class NapDataViewModel : ViewModelBase
    {
        /// <summary>
        /// The start time of the nap.
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// The nap duration in minutes.
        /// </summary>
        private int _duration;

        /// <summary>
        /// Creates a NapDataViewModel instance.
        /// </summary>
        public NapDataViewModel()
        {
        }

        /// <summary>
        /// Creates a NapDataViewModel instance.
        /// </summary>
        /// <param name="startTime"> The start time of the nap.</param>
        /// <param name="duration">The nap duration.</param>
        public NapDataViewModel(DateTime startTime, int duration)
        {
            _startTime = startTime;
            _duration = duration;
        }

        /// <summary>
        /// Changes the nap duration.
        /// </summary>
        /// <param name="delta">The change delta in minutes</param>
        public void ChangeDuration(int delta)
        {
            Duration += delta;
            Duration = Math.Max(1, Duration);
        }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    NotifyPropertyChanged("StartTime");
                }
            }
        }

        /// <summary>
        /// Gets or sets the napping duration in minutes.
        /// </summary>
        public int Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    NotifyPropertyChanged("Duration");
                }
            }
        }
    }
}
