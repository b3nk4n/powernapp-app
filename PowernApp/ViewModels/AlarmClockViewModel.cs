using Microsoft.Devices;
using Microsoft.Phone.Scheduler;
using Microsoft.Xna.Framework.Audio;
using PhoneKit.Framework.Audio;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Storage;
using System;
using System.Windows.Input;
using System.Windows.Resources;
using System.Windows.Threading;

namespace PowernApp.ViewModels
{
    /// <summary>
    /// Represents a down counting alarm clock.
    /// </summary>
    public class AlarmClockViewModel : ViewModelBase
    {
        # region Members

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static AlarmClockViewModel instance;

        /// <summary>
        /// The alarms unique name.
        /// </summary>
        private const string ALARM_NAME = "powerNapAlarm";

        /// <summary>
        /// Timer to adjust the alarm time each second.
        /// </summary>
        private DispatcherTimer _dispatcherTimer;

        /// <summary>
        /// The alarm sound.
        /// </summary>
        private SoundEffectInstance _alarmSound;

        /// <summary>
        /// The progress of the total power nap [0,...,100].
        /// </summary>
        private int _progress;

        /// <summary>
        /// The alarm time.
        /// </summary>
        private readonly StoredObject<DateTime> _alarmTime = new StoredObject<DateTime>("alarmTime", DateTime.MinValue);

        /// <summary>
        /// The time when the user has set the alarm.
        /// </summary>
        private readonly StoredObject<DateTime> _alarmSetTime = new StoredObject<DateTime>("alarmSetTime", DateTime.MinValue);

        /// <summary>
        /// the phones alarm scheduler.
        /// </summary>
        private static Alarm alarm;

        /// <summary>
        /// The start alarm command.
        /// </summary>
        private DelegateCommand<string> _startCommand;

        /// <summary>
        /// The snooze alarm command.
        /// </summary>
        private DelegateCommand<string> _snoozeCommand;

        /// <summary>
        /// The anti snooze alarm command.
        /// </summary>
        private DelegateCommand<string> _antiSnoozeCommand;

        /// <summary>
        /// The stop alarm command.
        /// </summary>
        private DelegateCommand _stopCommand;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an AlarmClock instance.
        /// </summary>
        private AlarmClockViewModel()
        {
            // timer
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();

            // alarm
            AlarmClockViewModel.alarm = new Alarm(ALARM_NAME);
            AlarmClockViewModel.alarm.ExpirationTime = DateTime.MaxValue;
            AlarmClockViewModel.alarm.RecurrenceType = RecurrenceInterval.None;
            AlarmClockViewModel.alarm.Content = "Power nap done!";

            // commands
            _startCommand = new DelegateCommand<string>(
                (minutes) =>
                {
                    var min = int.Parse(minutes);
                    Set(min);
                },
                (minutes) =>
                {
                    return !IsAlarmSet;
                });

            _snoozeCommand = new DelegateCommand<string>(
                (minutes) =>
                {
                    var min = int.Parse(minutes);
                    Snooze(min);
                },
                (minutes) =>
                {
                    return IsAlarmSet;
                });

            _antiSnoozeCommand = new DelegateCommand<string>(
                (minutes) =>
                {
                    var min = int.Parse(minutes);
                    Snooze(-min);
                },
                (minutes) =>
                {
                    var min = int.Parse(minutes);
                    return IsAlarmSet && TimeToAlarm.TotalMinutes > min;
                });

            _stopCommand = new DelegateCommand(
                () =>
                {
                    Stop();
                },
                () =>
                {
                    return IsAlarmSet;
                });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to set the alarm.
        /// </summary>
        /// <param name="minutes">The alarm time in minutes until now.</param>
        /// <returns>Returns true if successful, else false.</returns>
        public bool Set(int minutes)
        {
            if (minutes < 1)
                throw new ArgumentException("Alarm time must be at least 1 minute.");

            if (!IsAlarmSet)
            {
                AlarmSetTime = DateTime.Now;
                AlarmTime = DateTime.Now.AddMinutes(minutes);

                UpdateCommands();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Snoozes the alarm time.
        /// </summary>
        /// <param name="minutes">The snooze time.</param>
        /// <returns>Returns true if successful, else false.</returns>
        public bool Snooze(int minutes)
        {
            if (minutes == 0)
                throw new ArgumentException("Alarm snooze time must be non zero.");

            if (IsAlarmSet)
            {
                DateTime newAlarmBaseTime;

                // verify new alarm time offset is not based on a passed time
                if (AlarmTime < DateTime.Now)
                    newAlarmBaseTime = DateTime.Now;
                else
                    newAlarmBaseTime = AlarmTime;

                if (minutes > 0)
                {
                    AlarmTime = newAlarmBaseTime.AddMinutes(minutes);
                }
                else if (TimeToAlarm.TotalMinutes > minutes)
                {
                    AlarmTime = newAlarmBaseTime.AddMinutes(minutes);
                }

                UpdateCommands();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the alarm.
        /// </summary>
        public bool Stop()
        {
            if (IsAlarmSet)
            {
                AlarmTime = DateTime.MinValue;
                AlarmSetTime = DateTime.MinValue;
                UpdateCommands();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to add the alarm scheduler.
        /// </summary>
        public void TryAddToScheduler()
        {
            if (IsAlarmSet)
            {
                try
                {
                    if (AlarmTime > DateTime.Now)
                    {
                        AlarmClockViewModel.alarm.BeginTime = AlarmTime;
                        AlarmClockViewModel.alarm.Sound = new Uri(Settings.AlarmUriString.Value, UriKind.Relative);
                        ScheduledActionService.Add(AlarmClockViewModel.alarm);
                    }
                }
                catch (Exception)
                {
                    // do nothing, only to prevent the ScheduledActionService from adding
                    // the alarm member variable more than once
                }
            }
        }

        /// <summary>
        /// Tries to remove the alarm scheduler.
        /// </summary>
        public void TryRemoveFromScheduler()
        {
                var oldAlarm = ScheduledActionService.Find(ALARM_NAME) as Alarm;

                if (oldAlarm != null)
                {
                    // check if alarm was dismissed
                    if (oldAlarm.IsScheduled == false)
                    {
                        Stop();
                    }

                    ScheduledActionService.Remove(ALARM_NAME);
                }
        }

        /// <summary>
        /// Updates the binded button states depending on the CanExecute function.
        /// </summary>
        private void UpdateCommands()
        {
            _snoozeCommand.RaiseCanExecuteChanged();
            _antiSnoozeCommand.RaiseCanExecuteChanged();
            _stopCommand.RaiseCanExecuteChanged();
            _startCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Updates the alarm click every second.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (IsAlarmSet)
            {
                var totalSeconds = (AlarmTime - AlarmSetTime).TotalSeconds;
                var passedSeconds = (DateTime.Now - AlarmSetTime).TotalSeconds;

                if (totalSeconds == 0)
                    Progress = 0;
                else
                    Progress = (int)(100 * passedSeconds / totalSeconds);

                NotifyPropertyChanged("TimeToAlarm");
                if (TimeToAlarm.TotalSeconds <= 0 && TimeToAlarm.TotalSeconds >= -60)
                {
                    // vibrate only if enabled
                    if (Settings.EnableVibration.Value)
                    {
                        VibrateController.Default.Start(TimeSpan.FromSeconds(0.5));
                    }

                    if (_alarmSound == null)
                    {
                        StreamResourceInfo alarmResource = App.GetResourceStream(new Uri(Settings.AlarmUriString.Value, UriKind.Relative));
                        SoundEffects.Instance.Load(Settings.AlarmUriString.Value, alarmResource);
                        _alarmSound = SoundEffects.Instance[Settings.AlarmUriString.Value].CreateInstance();
                        _alarmSound.IsLooped = true;
                    }

                    TryPlayAlarmSound();
                }
                else
                {
                    TryStopAlarmSound();
                }
            }
            else 
            {
                Progress = 0;
                NotifyPropertyChanged("TimeToAlarm");

                TryStopAlarmSound();
            }
        }

        /// <summary>
        /// Plays the alarm sound if sound instance has been loaded.
        /// </summary>
        private void TryPlayAlarmSound()
        {
            if (_alarmSound == null)
                return;

            _alarmSound.Play();
        }

        /// <summary>
        /// Stops the alarm sound if sound instance has been loaded.
        /// </summary>
        private void TryStopAlarmSound()
        {
            if (_alarmSound == null)
                return;

            if (_alarmSound.State == SoundState.Playing)
                _alarmSound.Stop();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton AlarmClock instance.
        /// </summary>
        public static AlarmClockViewModel Instance
        {
            get
            {
                if (instance == null)
                    instance = new AlarmClockViewModel();
                return instance;
            }
        }

        /// <summary>
        /// Gets wheter the alarm is active.
        /// </summary>
        public bool IsAlarmSet
        {
            get
            {
                return AlarmTime != DateTime.MinValue;
            }
        }

        /// <summary>
        /// Sets or gets the alarm time.
        /// </summary>
        public TimeSpan TimeToAlarm
        {
            get
            {
                var timeToAlarm = AlarmTime - DateTime.Now;
                return (timeToAlarm < TimeSpan.Zero) ? TimeSpan.Zero : timeToAlarm;
            }
        }

        public string TotalNapTime
        {
            get
            {
                return ((int)(AlarmTime - AlarmSetTime).TotalMinutes).ToString();
            }
        }

        /// <summary>
        /// Gets or sets the alarm time.
        /// </summary>
        public DateTime AlarmTime
        {
            get
            {
                return _alarmTime.Value;
            }
            private set
            {
                if (_alarmTime.Value != value)
                {
                    _alarmTime.Value = value;
                    NotifyPropertyChanged("AlarmTime");
                    NotifyPropertyChanged("TimeToAlarm");
                    NotifyPropertyChanged("TotalNapTime");
                }
            }
        }

        /// <summary>
        /// Gets or sets the alarm set time.
        /// </summary>
        public DateTime AlarmSetTime
        {
            get
            {
                return _alarmSetTime.Value;
            }
            private set
            {
                if (_alarmSetTime.Value != value)
                {
                    _alarmSetTime.Value = value;
                    NotifyPropertyChanged("AlarmSetTime");
                }
            }
        }

        /// <summary>
        /// Gets the progress of the total power nap.
        /// </summary>
        public int Progress
        {
            get
            {
                return _progress;
            }
            private set
            {
                if (_progress != value)
                {
                    _progress = Math.Min(Math.Max(value, 0), 100);
                    NotifyPropertyChanged("Progress");
                }
            }
        }

        /// <summary>
        /// Gets the start alarm command.
        /// </summary>
        public ICommand StartCommand
        {
            get
            {
                return _startCommand;
            }
        }

        /// <summary>
        /// Gets the snooze alarm command.
        /// </summary>
        public ICommand SnoozeCommand
        {
            get
            {
                return _snoozeCommand;
            }
        }

        /// <summary>
        /// Gets the anti snooze alarm command.
        /// </summary>
        public ICommand AntiSnoozeCommand
        {
            get
            {
                return _antiSnoozeCommand;
            }
        }

        /// <summary>
        /// Gets the stop alarm command.
        /// </summary>
        public ICommand StopCommand
        {
            get
            {
                return _stopCommand;
            }
        }

        #endregion
    }
}
