using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PowernApp.Resources;
using PhoneKit.Framework.Voice;
using PowernApp.ViewModels;
using PhoneKit.Framework.OS;
using System.Windows.Threading;
using System.Windows.Media;
using PhoneKit.Framework.Core.Net;
using PhoneKit.Framework.Support;

namespace PowernApp
{
    /// <summary>
    /// The applications default page.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Creates a MainPage instance.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // register voice commands
            Speech.Instance.InstallCommandSets(new Uri("ms-appx:///voicecommands.xml", UriKind.Absolute));

            CustomNapTimePicker.Value = AlarmClockViewModel.Instance.LastAlarmDuration;

            // late binding of timespan picker changed event
            CustomNapTimePicker.ValueChanged += CustomNapTimeChanged;

            Loaded += (s, e) =>
                {
                    // make sure all buttons are enabled/disabled properly
                    AlarmClockViewModel.Instance.UpdateCommands();

                   // Always play the blink animation, because it requires no
                   // resources when the element is collapsed
                    var timer = new DispatcherTimer();
                    timer.Tick += (se, ea) =>
                    {
                        // start clock async
                        AlarmBlinkingAnimation.Begin();
                        timer.Stop();
                    };
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Start();
                };

            ActivateAnimation.Completed += (s, e) =>
                {
                    UpdateGeneralViewState(false);
                };
            DeactivateAnimation.Completed += (s, e) =>
                {
                    UpdateGeneralViewState(false);
                };

            BuildLocalizedApplicationBar();

            // register startup actions
            StartupActionManager.Instance.Register(5, ActionExecutionRule.Equals, () =>
            {
                FeedbackManager.Instance.StartFirst();
            });
            StartupActionManager.Instance.Register(10, ActionExecutionRule.Equals, () =>
            {
                FeedbackManager.Instance.StartSecond();
            });
        }

        /// <summary>
        /// When main page gets active, disables idle detection (to not interrupt the speech)
        /// and try to parse voce commands from query string.
        /// </summary>
        /// <param name="e">The navigation args.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString != null &&
                NavigationContext.QueryString.ContainsKey("voiceCommandName"))
            {
                String commandName = NavigationContext.QueryString["voiceCommandName"];

                if (!string.IsNullOrEmpty(commandName))
                    handleVoiceCommands(commandName);

                // clear the QueryString or the page will retain the current value
                NavigationContext.QueryString.Clear();
            }
            else if (AlarmClockViewModel.Instance.IsAlarmNotRinging &&
                (!AlarmClockViewModel.Instance.IsAlarmSet || AlarmClockViewModel.Instance.TimeToAlarm.Minutes > 5))
            {
                // fire startup events only when the app started without voice command
                StartupActionManager.Instance.Fire();
            }

            // determine view state
            UpdateGeneralViewState(true);

            // set data context to view model
            DataContext = AlarmClockViewModel.Instance;
        }

        /// <summary>
        /// When the main page is navigates from.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (AlarmClockViewModel.Instance.IsAlarmRinging)
            {
                AlarmClockViewModel.Instance.Stop();
            }

            // ensure sound and vibration is off
            AlarmClockViewModel.Instance.ForceStopSoundAndVibration();
        }

        /// <summary>
        /// Updates the view state of the main page and the application bar
        /// depending on whether the alarm is on or off.
        /// <param name="withTimer">Indicates whether there should be a second async check of the message baloon.</param>
        /// </summary>
        private void UpdateGeneralViewState(bool withTimer)
        {
            ResetRotationAnimation.Begin();

            if (AlarmClockViewModel.Instance.IsAlarmSet)
            {
                ActivePanel.Visibility = Visibility.Visible;
                InactivePanel.Visibility = Visibility.Collapsed;
                BuildLocalizedApplicationBar(ConnectivityHelper.IsAirplaneMode);

                if (ConnectivityHelper.IsAirplaneMode)
                    ConnectivityMessageOut.Begin();
                else
                    ConnectivityMessageIn.Begin();

                if (withTimer)
                {
                    var timer = new DispatcherTimer();
                    timer.Tick += (s, e) =>
                    {
                        // check again after 2 sec, because sometime the enabling/disabling of
                        // flight mode takes a while.
                        if (AlarmClockViewModel.Instance.IsAlarmSet)
                        {
                            if (ConnectivityHelper.IsAirplaneMode)
                            {
                                ConnectivityMessageOut.Begin();
                            }
                            else
                            {
                                ConnectivityMessageIn.Begin();
                            }
                        }
                        else
                        {
                            ConnectivityMessageOut.Begin();
                        }

                        timer.Stop();
                    };
                    timer.Interval = TimeSpan.FromSeconds(3);
                    timer.Start();
                }
            }
            else
            {
                ActivePanel.Visibility = Visibility.Collapsed;
                InactivePanel.Visibility = Visibility.Visible;
                BuildLocalizedApplicationBar(false);
            }
        }

        /// <summary>
        /// Handles the voice commands.
        /// </summary>
        /// <param name="commandName">The command name.</param>
        private void handleVoiceCommands(string commandName)
        {
            string hours;
            string minutes;
            switch (commandName)
            {
                case "startNap1":
                    minutes = NavigationContext.QueryString["minute"];
                    handleStartNap1Command(minutes);
                    break;
                case "startNap2":
                    hours = NavigationContext.QueryString["hour"];
                    minutes = NavigationContext.QueryString["minute"];
                    handleStartNap2Command(hours, minutes);
                    break;
                case "stopNap":
                    handleStopNapCommand();
                    break;
                case "checkAlarmTime":
                    handleCheckAlarmTime();
                    break;
                case "checkRemainingTime":
                    handleCheckRemainingTime();
                    break;
                case "extendAlarmTime":
                    minutes = NavigationContext.QueryString["minute"];
                    handleExtendAlarmTime(minutes);
                    break;
            }
        }

        /// <summary>
        /// Handles the start nap command.
        /// </summary>
        /// <param name="minutes">The length of the nap in minutes.</param>
        private void handleStartNap1Command(string minutes)
        {
            int min = 30;
            int.TryParse(minutes, out min);

            if (AlarmClockViewModel.Instance.Set(min))
            {
                string startFormat = (new Random().Next(2) == 0) ? AppResources.SpeakStartNap1 : AppResources.SpeakStartNap2;

                GiveVoiceFeedback(string.Format(startFormat, minutes));
            }
            else
                GiveVoiceFeedback(AppResources.SpeakAlarmAlreadySet);
        }

        /// <summary>
        /// Handles the start nap command.
        /// </summary>
        /// <param name="hours">The length of the nap in hours.</param>
        /// <param name="minutes">The length of the nap in minutes.</param>
        private void handleStartNap2Command(string hours, string minutes)
        {
            int h = 0;
            int min = 30;
            int.TryParse(minutes, out min);
            int.TryParse(hours, out h);
            int totalMins = 60 * h + min;

            if (AlarmClockViewModel.Instance.Set(totalMins))
            {
                string startFormat = (new Random().Next(2) == 0) ? AppResources.SpeakStartNap1 : AppResources.SpeakStartNap2;

                GiveVoiceFeedback(string.Format(startFormat, totalMins));
            }
            else
                GiveVoiceFeedback(AppResources.SpeakAlarmAlreadySet);
        }

        /// <summary>
        /// Handles the stop nap command.
        /// </summary>
        private void handleStopNapCommand()
        {
            if (AlarmClockViewModel.Instance.Stop())
            {
                string stopString = (new Random().Next(2) == 0) ? AppResources.SpeakStopNap1 : AppResources.SpeakStopNap2;

                GiveVoiceFeedback(stopString);
            }
            else
                GiveVoiceFeedback(AppResources.SpeakNoAlarmSet);
        }

        /// <summary>
        /// Handles the check alarm time command.
        /// </summary>
        private void handleCheckAlarmTime()
        {
            if (AlarmClockViewModel.Instance.IsAlarmSet)
                GiveVoiceFeedback(string.Format(AppResources.SpeakAlarmSetFor, AlarmClockViewModel.Instance.AlarmTime.ToString("t"))); // 12:12 PM
            else
                GiveVoiceFeedback(AppResources.SpeakNoAlarmSet);
        }

        /// <summary>
        /// Handles the check remaining time command.
        /// </summary>
        private void handleCheckRemainingTime()
        {
            if (AlarmClockViewModel.Instance.IsAlarmSet)
                GiveVoiceFeedback(string.Format(AppResources.SpeakTimeLeft, (int)AlarmClockViewModel.Instance.TimeToAlarm.TotalMinutes));
            else
                GiveVoiceFeedback(AppResources.SpeakNoAlarmSet);
        }

        /// <summary>
        /// Handles the extend alarm time command.
        /// </summary>
        /// <param name="minutes">The minutes to extend the alarm.</param>
        private void handleExtendAlarmTime(string minutes)
        {
            int min = 5;
            int.TryParse(minutes, out min);

            if (AlarmClockViewModel.Instance.Snooze(min))
                GiveVoiceFeedback(string.Format(AppResources.SpeakTimeShifted, (int)AlarmClockViewModel.Instance.TimeToAlarm.TotalMinutes));
            else
                GiveVoiceFeedback(AppResources.SpeakNoAlarmSet);
        }

        /// <summary>
        /// Speaks a text if the setting for voice feedback is active.
        /// </summary>
        /// <param name="text">The text to speak.</param>
        private async void GiveVoiceFeedback(string text)
        {
            if (Settings.EnableVoiceFeedback.Value)
                await Speech.Instance.TrySpeakTextAsync(text);
        }

        /// <summary>
        /// Builds the localized application bar with all list items.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            // assigns a new application bar to the page.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)Application.Current.Resources["ThemeBackgroundMediumColor"];
            ApplicationBar.ForegroundColor = (Color)Application.Current.Resources["ThemeForegroundLightColor"];

            // info
            ApplicationBarMenuItem appBarMenuItem1 = new ApplicationBarMenuItem(AppResources.AppBarInfo);
            ApplicationBar.MenuItems.Add(appBarMenuItem1);
            appBarMenuItem1.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
            };

            // settings
            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.SettingsTitle);
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
            appBarMenuItem2.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            };

            // about
            ApplicationBarMenuItem appBarMenuItem3 = new ApplicationBarMenuItem(AppResources.AboutTitle);
            ApplicationBar.MenuItems.Add(appBarMenuItem3);
            appBarMenuItem3.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
            };
        }

        /// <summary>
        /// Builds the localized application bar buttons.
        /// <param name="showCellularButton">Indicates whether the cellular buttons should be generated</param>
        /// </summary>
        private void BuildLocalizedApplicationBar(bool showCellularButton)
        {
            ApplicationBar.Buttons.Clear();

            // info
            ApplicationBarIconButton appBarButton1 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.statistics.png", UriKind.Relative));
            appBarButton1.Text = "naptistics";
            ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/StatisticPage.xaml", UriKind.Relative));
            };

            if (showCellularButton)
            {
                // flight mode
                ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.cellular.png", UriKind.Relative));
                appBarButton2.Text = AppResources.AppBarOnline;
                ApplicationBar.Buttons.Add(appBarButton2);
                appBarButton2.Click += async (s, e) =>
                {
                    // TODO: navigate to info page
                    await SettingsLauncher.LaunchAirplaneModeAsync();
                };
            }
        }

        /// <summary>
        /// Open the timespan picker box.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void CustomNapClick(object sender, RoutedEventArgs e)
        {
            CustomNapTimePicker.OpenPicker();
        }

        /// <summary>
        /// Handles the stop button click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            DeactivateAnimation.Begin();
            ConnectivityMessageOut.Begin();
        }

        /// <summary>
        /// Handles the start button click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void StartAlarmClick(object sender, RoutedEventArgs e)
        {
            var minutes = (int)CustomNapTimePicker.Value.Value.TotalMinutes;
            AlarmClockViewModel.Instance.Set(minutes);
            ActivateAnimation.Begin();
        }

        /// <summary>
        /// Handles the change of the timespan using one of the buttons.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void ChangeAlarmTimeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button == null)
                return;

            int minDelta = 1;
            int.TryParse(button.Tag.ToString(), out minDelta);
            var value = CustomNapTimePicker.Value.Value;

            value = value.Add(TimeSpan.FromMinutes(minDelta));
            
            // verify at least 1 min
            if (value.TotalMinutes < 1)
                value = TimeSpan.FromMinutes(1);

            CustomNapTimePicker.Value = value;
        }

        /// <summary>
        /// Handles the stop button click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void CustomNapTimeChanged(object sender, RoutedPropertyChangedEventArgs<TimeSpan> e)
        {
            // update button view state in inactive mode.
            var minutes = (int)CustomNapTimePicker.Value.Value.TotalMinutes;
            bool noAlarmOn = !AlarmClockViewModel.Instance.IsAlarmSet;

            ButtonMinus5.IsEnabled = minutes > 5 && noAlarmOn;
            ButtonMinus1.IsEnabled = minutes > 1 && noAlarmOn;
            ButtonPlus5.IsEnabled = noAlarmOn;
            ButtonPlus1.IsEnabled = noAlarmOn;
        }
    }
}