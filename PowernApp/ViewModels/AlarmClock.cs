using Microsoft.Devices;
using Microsoft.Phone.Scheduler;
using Microsoft.Xna.Framework.Audio;
using PhoneKit.Framework.Audio;
using PhoneKit.Framework.Core.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Resources;
using System.Windows.Threading;

namespace PowernApp.ViewModels
{
    /// <summary>
    /// Represents a down counting alarm clock.
    /// </summary>
    public class AlarmClock : ViewModelBase
    {
        # region Members

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static AlarmClock instance;

        /// <summary>
        /// The alarms unique name.
        /// </summary>
        private const string ALARM_NAME = "powerNapAlarm";

        /// <summary>
        /// The URI specifying the default alarm audio file.
        /// </summary>
        private readonly Uri _alarmUri = new Uri("Assets/Audio/alarm.wav", UriKind.Relative);

        /// <summary>
        /// Timer to adjust the alarm time each second.
        /// </summary>
        private DispatcherTimer _dispatcherTimer;

        /// <summary>
        /// The alarm sound.
        /// </summary>
        private SoundEffectInstance _alarmSound;

        /// <summary>
        /// The remaining time to start the alarm.
        /// </summary>
        private TimeSpan _timeToAlarm;

        /// <summary>
        /// The progress of the total power nap [0,...,100].
        /// </summary>
        private int _progress;

        /// <summary>
        /// the phones alarm scheduler.
        /// </summary>
        private static Alarm alarm;

        /// <summary>
        /// The start alarm command.
        /// </summary>
        private DelegateCommand<string> _startAlarm;

        /// <summary>
        /// The snooze alarm command.
        /// </summary>
        private DelegateCommand<string> _snoozeAlarm;

        /// <summary>
        /// The stop alarm command.
        /// </summary>
        private DelegateCommand _stopAlarm;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an AlarmClock instance.
        /// </summary>
        private AlarmClock()
        {
            // sound
            StreamResourceInfo alarmResource = App.GetResourceStream(_alarmUri);
            SoundEffects.Instance.Load("alarm", alarmResource);
            _alarmSound = SoundEffects.Instance["alarm"].CreateInstance();
            _alarmSound.IsLooped = true;

            // timer
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();

            // alarm
            AlarmClock.alarm = new Alarm(ALARM_NAME);
            AlarmClock.alarm.ExpirationTime = DateTime.MaxValue;
            AlarmClock.alarm.RecurrenceType = RecurrenceInterval.None;
            AlarmClock.alarm.Sound = _alarmUri;
            AlarmClock.alarm.Content = "Power nap done!";

            // commands
            _startAlarm = new DelegateCommand<string>(
                (minutes) =>
                {
                    var now = DateTime.Now;
                    var alarmTime = now.AddMinutes(int.Parse(minutes));
                    Settings.AlarmTime.Value = alarmTime;
                    Settings.AlarmStartTime.Value = now;
                    UpdateCommands();
                },
                (minutes) =>
                {
                    return !IsAlarmSet;
                });

            _snoozeAlarm = new DelegateCommand<string>(
                (minutes) =>
                {
                    var alarmTime = Settings.AlarmTime.Value.AddMinutes(int.Parse(minutes));
                    Settings.AlarmTime.Value = alarmTime;
                    UpdateCommands();
                },
                (minutes) =>
                {
                    return IsAlarmSet;
                });

            _stopAlarm = new DelegateCommand(
                () =>
                {
                    Settings.AlarmTime.Value = DateTime.MinValue;
                    Settings.AlarmStartTime.Value = DateTime.MinValue;
                    UpdateCommands();
                },
                () =>
                {
                    return IsAlarmSet;
                });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to add the alarm scheduler.
        /// </summary>
        public void TryAddToScheduler()
        {
            if (IsAlarmSet)
            {
                try
                {
                    if (Settings.AlarmTime.Value > DateTime.Now)
                    {
                        AlarmClock.alarm.BeginTime = Settings.AlarmTime.Value;
                        ScheduledActionService.Add(AlarmClock.alarm);
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
                        Settings.AlarmTime.Value = DateTime.MinValue;
                    }

                    ScheduledActionService.Remove(ALARM_NAME);
                }
        }

        /// <summary>
        /// Updates the binded button states depending on the CanExecute function.
        /// </summary>
        private void UpdateCommands()
        {
            _snoozeAlarm.RaiseCanExecuteChanged();
            _stopAlarm.RaiseCanExecuteChanged();
            _startAlarm.RaiseCanExecuteChanged();
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
                var totalSeconds = (Settings.AlarmTime.Value - Settings.AlarmStartTime.Value).TotalSeconds;
                var passedSeconds = (DateTime.Now - Settings.AlarmStartTime.Value).TotalSeconds;

                if (totalSeconds == 0)
                    Progress = 0;
                else
                    Progress = (int)(100 * passedSeconds / totalSeconds);

                TimeToAlarm = (Settings.AlarmTime.Value - DateTime.Now); 
                if (TimeToAlarm.TotalSeconds <= 0 && TimeToAlarm.TotalSeconds >= -60)
                {
                    // vibrate only if enabled
                    if (Settings.EnableVibration.Value)
                    {
                        VibrateController.Default.Start(TimeSpan.FromSeconds(0.5));
                    }
                    _alarmSound.Play();
                }
                else if (_alarmSound.State == SoundState.Playing)
                {
                    _alarmSound.Stop();
                }
            }
            else 
            {
                Progress = 0;
                TimeToAlarm = TimeSpan.Zero;

                if (_alarmSound.State == SoundState.Playing)
                    _alarmSound.Stop();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton AlarmClock instance.
        /// </summary>
        public static AlarmClock Instance
        {
            get
            {
                if (instance == null)
                    instance = new AlarmClock();
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
                return Settings.AlarmTime.Value > DateTime.Now;
            }
        }

        /// <summary>
        /// Sets or gets the alarm time.
        /// </summary>
        public TimeSpan TimeToAlarm
        {
            get
            {
                return _timeToAlarm;
            }
            private set
            {
                if (_timeToAlarm != value)
                {
                    _timeToAlarm = value;
                    NotifyPropertyChanged("TimeToAlarm");
                    NotifyPropertyChanged("FormattedTimeToAlarm");
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
            set
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
        public ICommand StartAlarm
        {
            get
            {
                return _startAlarm;
            }
        }

        /// <summary>
        /// Gets the snooze alarm command.
        /// </summary>
        public ICommand SnoozeAlarm
        {
            get
            {
                return _snoozeAlarm;
            }
        }

        /// <summary>
        /// Gets the stop alarm command.
        /// </summary>
        public ICommand StopAlarm
        {
            get
            {
                return _stopAlarm;
            }
        }

        #endregion
    }
}
