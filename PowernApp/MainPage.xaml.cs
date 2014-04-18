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

            if (Settings.EnableSuppressLockScreen.Value)
            {
                // disable lock screen
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

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
                BuildActiveLocalizedApplicationBar();

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
                    timer.Interval = TimeSpan.FromSeconds(5);
                    timer.Start();
                }
            }
            else
            {
                ActivePanel.Visibility = Visibility.Collapsed;
                InactivePanel.Visibility = Visibility.Visible;
                BuildInactiveLocalizedApplicationBar();
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
            string identifier;
            string text;
            string minText;
            string relative;
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
                case "startNap3":
                    hours = NavigationContext.QueryString["hour"];
                    minutes = NavigationContext.QueryString["minute"];
                    handleStartNap3Command(hours, minutes);
                    break;
                case "startNap3Short":
                    hours = NavigationContext.QueryString["hour"];
                    handleStartNap3ShortCommand(hours);
                    break;
                case "startNap3DEText":
                    hours = NavigationContext.QueryString["hour"];
                    text = NavigationContext.QueryString["text"];
                    handleStartNap3DETextCommand(hours, text);
                    break;
                case "startNap3US":
                    hours = NavigationContext.QueryString["hour"];
                    minutes = NavigationContext.QueryString["minute"];
                    identifier = NavigationContext.QueryString["identifier"];
                    handleStartNap3USCommand(hours, minutes, identifier);
                    break;
                case "startNap3ShortUS":
                    hours = NavigationContext.QueryString["hour"];
                    identifier = NavigationContext.QueryString["identifier"];
                    handleStartNap3ShortUSCommand(hours, identifier);
                    break;
                case "startNap3GB":
                    hours = NavigationContext.QueryString["hour"];
                    minText = NavigationContext.QueryString["minText"];
                    relative = NavigationContext.QueryString["relative"];
                    handleStartNap3GBCommand(hours, minText, relative);
                    break;
                case "startNap3ShortGB":
                    hours = NavigationContext.QueryString["hour"];
                    handleStartNap3ShortGBCommand(hours);
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
            handleStartNap2Command("0", minutes);
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

            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Handles the start nap command with 12h clock.
        /// </summary>
        /// <param name="hours">The hour clock time to sleep to.</param>
        /// <param name="minutes">The minutes clock time to sleep to.</param>
        private void handleStartNap3Command(string hours, string minutes)
        {
            int h = DateTime.Now.Hour;
            int min = DateTime.Now.Minute + 30;

            if (min > 59)
            {
                min -= 60;
                h++;
                if (h > 23)
                    h = 0;
            }

            int.TryParse(minutes, out min);
            int.TryParse(hours, out h);

            var now = DateTime.Now;
            var sleepTo = new DateTime(now.Year, now.Month, now.Day, h, min, 0);
           
            if (sleepTo < now)
            {
                sleepTo = sleepTo.AddDays(1);
            }

            int totalMins = (int)(sleepTo - now).TotalMinutes + 1;

            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Handles the start nap command.
        /// </summary>
        /// <param name="hours">The hour clock time to sleep to.</param>
        /// <param name="minutes">The minutes clock time to sleep to.</param>
        private void handleStartNap3ShortCommand(string hours)
        {
            handleStartNap3Command(hours, "0");
        }

        /// <summary>
        /// Handles the start nap command with 12h clock.
        /// </summary>
        /// <param name="hours">The hour clock time to sleep to.</param>
        /// <param name="identifier12h">The 12h clock identifier (am/pm)</param>
        private void handleStartNap3ShortUSCommand(string hours, string identifier12h)
        {
            handleStartNap3USCommand(hours, "0", identifier12h);
        }

        /// <summary>
        /// Handles the start nap command with 12h clock.
        /// </summary>
        /// <param name="hours">The hour clock time to sleep to.</param>
        /// <param name="minutes">The minutes clock time to sleep to.</param>
        /// <param name="identifier12h">The 12h clock identifier (am/pm)</param>
        private void handleStartNap3USCommand(string hours, string minutes, string identifier12h)
        {
            int h = DateTime.Now.Hour;
            int min = DateTime.Now.Minute + 30;

            if (min > 59)
            {
                min -= 60;
                h++;
                if (h > 23)
                    h = 0;
            }

            int.TryParse(minutes, out min);
            int.TryParse(hours, out h);

            var now = DateTime.Now;
            var sleepTo = new DateTime(now.Year, now.Month, now.Day, h, min, 0);

            if (identifier12h == "PM")
            {
                sleepTo = sleepTo.AddHours(12);
            }

            if (sleepTo < now)
            {
                sleepTo = sleepTo.AddDays(1);
            }

            int totalMins = (int)(sleepTo - now).TotalMinutes + 1;

            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Handles the start nap command with german short quarter text.
        /// </summary>
        /// <param name="hours">The hour clock time to sleep to.</param>
        /// <param name="text">The german quarter text (viertel vor/halb/viertel nach)</param>
        private void handleStartNap3DETextCommand(string hours, string text)
        {
            int minOffset = -30; // "halb" as default
            int h = DateTime.Now.Hour + 1;
            if (h > 23)
                h = 0;

            int.TryParse(hours, out h);

            switch (text)
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

            var now = DateTime.Now;
            var sleepTo = new DateTime(now.Year, now.Month, now.Day, h, 0, 0);
            sleepTo = sleepTo.AddMinutes(minOffset);

            if (sleepTo < now)
            {
                sleepTo = sleepTo.AddHours(12);
            }
            // do this a second time, bacause sleepTo could still be before NOW
            if (sleepTo < now)
            {
                sleepTo = sleepTo.AddHours(12);
            }

            int totalMins = (int)(sleepTo - now).TotalMinutes + 1;

            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Handles the britain sleep time.
        /// </summary>
        /// <param name="hours">The next full hours.</param>
        /// <param name="minText">The minute text in interval 5.</param>
        /// <param name="relative">The relative (offset/downset) as "past" or "to".</param>
        private void handleStartNap3GBCommand(string hours, string minText, string relative)
        {
            int factor = 1;
            int minOffset = 30; // "half" as default
            int h = DateTime.Now.Hour + 1;
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

            var now = DateTime.Now;
            var sleepTo = new DateTime(now.Year, now.Month, now.Day, h, 0, 0);
            sleepTo = sleepTo.AddMinutes(minOffset);

            while (sleepTo < now)
            {
                sleepTo = sleepTo.AddHours(12);
            }

            int totalMins = (int)(sleepTo - now).TotalMinutes + 1;

            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Hanldes the short britain sleep time with 12h clock.
        /// </summary>
        /// <param name="hours">The next full hours.</param>
        private void handleStartNap3ShortGBCommand(string hours)
        {
            int h = DateTime.Now.Hour + 1;
            if (h > 23)
                h = 0;

            int.TryParse(hours, out h);

            var now = DateTime.Now;
            var sleepTo = new DateTime(now.Year, now.Month, now.Day, h, 0, 0);

            while (sleepTo < now)
            {
                sleepTo = sleepTo.AddHours(12);
            }

            int totalMins = (int)(sleepTo - now).TotalMinutes + 1;

            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Handles the sleep total minutes and fives sound feedback.
        /// </summary>
        /// <param name="totalMinutes">The total minutes to sleep.</param>
        private void handleSleepMinutesInternal(int totalMinutes)
        {
            if (AlarmClockViewModel.Instance.Set(totalMinutes))
            {
                string startFormat = (new Random().Next(2) == 0) ? AppResources.SpeakStartNap1 : AppResources.SpeakStartNap2;

                GiveVoiceFeedback(string.Format(startFormat, totalMinutes));
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

            // settings
            ApplicationBarMenuItem appBarMenuItem1 = new ApplicationBarMenuItem(AppResources.SettingsTitle);
            ApplicationBar.MenuItems.Add(appBarMenuItem1);
            appBarMenuItem1.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            };

            // settings
            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.InAppStoreTitle);
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
            appBarMenuItem2.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/InAppStorePage.xaml", UriKind.Relative));
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
        /// Builds the localized application bar buttons in active mode.
        /// </summary>
        private void BuildActiveLocalizedApplicationBar()
        {
            ApplicationBar.Buttons.Clear();

            // info
            ApplicationBarIconButton appBarButton1 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.questionmark.png", UriKind.Relative));
            appBarButton1.Text = AppResources.AppBarInfo;
            ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
            };

            // naptistics
            ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.statistics.png", UriKind.Relative));
            appBarButton2.Text = "naptistics";
            ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/StatisticPage.xaml", UriKind.Relative));
            };

            // flight mode
            ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.nocellular.png", UriKind.Relative));
            appBarButton3.Text = AppResources.AppBarOffline;
            ApplicationBar.Buttons.Add(appBarButton3);
            appBarButton3.Click += async (s, e) =>
            {
                // TODO: navigate to info page
                await SettingsLauncher.LaunchAirplaneModeAsync();
            };
        }


        /// <summary>
        /// Builds the localized application bar buttons in inactive mode.
        /// </summary>
        private void BuildInactiveLocalizedApplicationBar()
        {
            ApplicationBar.Buttons.Clear();

            // info
            ApplicationBarIconButton appBarButton1 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.questionmark.png", UriKind.Relative));
            appBarButton1.Text = AppResources.AppBarInfo;
            ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
            };

            // naptistics
            ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.statistics.png", UriKind.Relative));
            appBarButton2.Text = "naptistics";
            ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/StatisticPage.xaml", UriKind.Relative));
            };

            if (ConnectivityHelper.IsAirplaneMode)
            {
                // flight mode
                ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.cellular.png", UriKind.Relative));
                appBarButton3.Text = AppResources.AppBarOnline;
                ApplicationBar.Buttons.Add(appBarButton3);
                appBarButton3.Click += async (s, e) =>
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