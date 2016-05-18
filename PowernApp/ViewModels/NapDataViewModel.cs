using PhoneKit.Framework.Core.MVVM;
using System;
using System.Windows.Input;

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
        /// The delete single item command.
        /// </summary>
        private DelegateCommand _deleteCommand;

        /// <summary>
        /// Creates a NapDataViewModel instance.
        /// </summary>
        public NapDataViewModel()
        {
            InitializeCommands();
        }

        /// <summary>
        /// Creates a NapDataViewModel instance.
        /// </summary>
        /// <param name="startTime"> The start time of the nap.</param>
        /// <param name="duration">The nap duration.</param>
        public NapDataViewModel(DateTime startTime, int duration)
            : this()
        {
            _startTime = startTime;
            _duration = duration;
        }

        private void InitializeCommands()
        {
            _deleteCommand = new DelegateCommand(() =>
            {
                // remove itself from the list
                NapStatisticsViewModel.Instance.Delete(this);
            });
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

        /// <summary>
        /// 
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand;
            }
        }
    }
}
