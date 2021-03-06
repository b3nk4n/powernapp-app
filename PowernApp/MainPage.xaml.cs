using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PowernApp.Resources;
using PhoneKit.Framework.Voice;
using PowernApp.ViewModels;
using PhoneKit.Framework.OS;
using System.Windows.Threading;
using System.Windows.Media;
using PhoneKit.Framework.Support;
using PowernApp.Napping;
using PhoneKit.Framework.InAppPurchase;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace PowernApp
{
    /// <summary>
    /// The applications default page.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage, AlarmClockCallback
    {
        private bool _isBannerVisible = false;

        /// <summary>
        /// Creates a MainPage instance.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // register voice commands
            Speech.Instance.InstallCommandSets(new Uri("ms-appx:///voicecommands.xml", UriKind.Absolute));

            //// late binding of timespan picker changed event
            CustomNapTimePicker.ValueChanged += CustomNapTimeChanged;
            CustomNapTimePicker.Value = AlarmClockViewModel.Instance.LastAlarmDuration;

            Loaded += (s, e) =>
                {
                    // make sure all buttons are enabled/disabled properly
                    AlarmClockViewModel.Instance.UpdateCommands();

                   // Always play the blink animation, because it requires no
                   // resources when the element is collapsed
                    AlarmBlinkingAnimation.Begin();

                    var timer = new DispatcherTimer();
                    timer.Tick += (se, ea) =>
                    {
                        UpdateBannerViewState();
                    };
                    timer.Interval = TimeSpan.FromSeconds(5);
                    timer.Start();

                };

            ActivateAnimation.Completed += (s, e) =>
                {
                    UpdateGeneralViewState();
                };
            DeactivateAnimation.Completed += (s, e) =>
                {
                    UpdateGeneralViewState();
                };

            BuildLocalizedApplicationBar();

            // register startup actions
            StartupActionManager.Instance.Register(10, ActionExecutionRule.Equals, () =>
            {
                if (!InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
                    FeedbackManager.Instance.StartFirst();
            });
            StartupActionManager.Instance.Register(20, ActionExecutionRule.Equals, () =>
            {
                if (!InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
                    FeedbackManager.Instance.StartSecond();
            });
        }

        /// <summary>
        /// Updates the banner view state. This function is called every 5 second!
        /// </summary>
        private void UpdateBannerViewState()
        {
            if (!_isBannerVisible && AlarmClockViewModel.Instance.IsAlarmSet && AlarmClockViewModel.Instance.TimeSinceStart.TotalSeconds >= 20 && AlarmClockViewModel.Instance.TimeToAlarm.TotalSeconds > 20)
            {
                ConnectivityMessageOut.Begin();
                BannerIn.Begin();
                _isBannerVisible = true;
            }
            else if (_isBannerVisible && AlarmClockViewModel.Instance.IsAlarmSet && AlarmClockViewModel.Instance.TimeToAlarm.TotalSeconds <= 20)
            {
                BannerOut.Begin();
                _isBannerVisible = false;
            }
        }

        /// <summary>
        /// Helper function to shuffle a list.
        /// </summary>
        /// <typeparam name="T"> The list type.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        private static void ShuffleList<T>(IList<T> list) // TODO: shift to framework, also used in image info app
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
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

                if (!string.IsNullOrEmpty(commandName) && e.NavigationMode == NavigationMode.New)
                    handleVoiceCommands(commandName);

                // clear the QueryString or the page will retain the current value
                NavigationContext.QueryString.Clear();
            }
            else if (AlarmClockViewModel.Instance.IsAlarmNotRinging &&
                (!AlarmClockViewModel.Instance.IsAlarmSet || AlarmClockViewModel.Instance.TimeToAlarm.Minutes > 5))
            {
                // fire startup events only when the app started without voice command
                StartupActionManager.Instance.Fire(e);
            }

            // determine view state
            UpdateGeneralViewState();

            if (Settings.EnableSuppressLockScreen.Value)
            {
                // disable lock screen
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            UpdateBannerVisibility();

            UpdatePreviewTime();

            // set data context to view model
            AlarmClockViewModel.Instance.SetCallbackContext(this);
            DataContext = AlarmClockViewModel.Instance;

            AlarmClockViewModel.Instance.UpdatePresets();
        }

        /// <summary>
        /// Updates the banner visiblilty.
        /// </summary>
        private void UpdateBannerVisibility()
        {
            if (InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
            {
                BannerControl.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                BannerControl.Visibility = System.Windows.Visibility.Visible;
            }
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

            // update live tile in PRO version
#if !DEBUG
            if (InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
#endif
            {
                // update is a bit performance consuming. Only update the live tile when the app is deactivated/closed
                if (e.NavigationMode == NavigationMode.Back ||
                    e.Uri.OriginalString == "app://external/")
                {
                    NapStatisticsViewModel.Instance.UpdateLiveTile();
                }
            }
        }

        /// <summary>
        /// Updates the view state of the main page and the application bar
        /// depending on whether the alarm is on or off.
        /// </summary>
        private void UpdateGeneralViewState()
        {
            ResetRotationAnimation.Begin();

            if (AlarmClockViewModel.Instance.IsAlarmSet)
            {
                ActivePanel.Visibility = Visibility.Visible;
                InactivePanel.Visibility = Visibility.Collapsed;
                BuildLocalizedApplicationBarButtons(true);
            }
            else
            {
                ActivePanel.Visibility = Visibility.Collapsed;
                InactivePanel.Visibility = Visibility.Visible;
                BuildLocalizedApplicationBarButtons(false);
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
            string minText;
            string relative;
            switch (commandName)
            {
                case "startNap1":
                    minutes = NavigationContext.QueryString["minute"];
                    handleStartNap1Command(minutes);
                    break;
                case "startNap1b":
                    hours = NavigationContext.QueryString["hour"];
                    handleStartNap2Command(hours, "0");
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
                    minText = NavigationContext.QueryString["text"];
                    handleStartNap3DETextCommand(hours, minText);
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
            NapTimeCalculator napTimeCalulator = new NapTimeCalculator(DateTime.Now);
            var totalMins = napTimeCalulator.calculateTime(hours, minutes);

            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Handles the start nap command with 12h clock.
        /// </summary>
        /// <param name="hours">The hour clock time to sleep to.</param>
        /// <param name="minutes">The minutes clock time to sleep to.</param>
        private void handleStartNap3Command(string hours, string minutes)
        {
            NapTimeCalculator napTimeCalulator = new NapTimeCalculator(DateTime.Now);
            var totalMins = napTimeCalulator.calculateTimeUntilTimeAs24Clock(hours, minutes);
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
            NapTimeCalculator napTimeCalulator = new NapTimeCalculator(DateTime.Now);
            var totalMins = napTimeCalulator.calculateTimeUntilTimeAs12ClockUS(hours, minutes, identifier12h);
            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Handles the start nap command with german short quarter text.
        /// </summary>
        /// <param name="hours">The hour clock time to sleep to.</param>
        /// <param name="text">The german quarter text (viertel vor/halb/viertel nach)</param>
        private void handleStartNap3DETextCommand(string hours, string text)
        {
            NapTimeCalculator napTimeCalulator = new NapTimeCalculator(DateTime.Now);
            var totalMins = napTimeCalulator.calculateTimeUntilTimeAs12ClockDEText(hours, text);
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
            NapTimeCalculator napTimeCalculator = new NapTimeCalculator(DateTime.Now);
            var totalMins = napTimeCalculator.calculateTimeUntilTimeAs12ClockGBText(hours, minText, relative);
            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Hanldes the short britain sleep time with 12h clock.
        /// </summary>
        /// <param name="hours">The next full hours.</param>
        private void handleStartNap3ShortGBCommand(string hours)
        {
            NapTimeCalculator napTimeCalculator = new NapTimeCalculator(DateTime.Now);
            var totalMins = napTimeCalculator.calculateTimeUntilTimeAs12ClockGB(hours);
            handleSleepMinutesInternal(totalMins);
        }

        /// <summary>
        /// Handles the sleep total minutes and fives sound feedback.
        /// </summary>
        /// <param name="totalMinutes">The total minutes to sleep.</param>
        private void handleSleepMinutesInternal(int totalMinutes)
        {
            // in case the value was negative, use a fallback value.
            if (totalMinutes < 1)
                totalMinutes = 20;

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
            ApplicationBar.Opacity = 0.99;
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
        /// Builds the localized application bar buttons in the appropriate mode.
        /// </summary>
        private void BuildLocalizedApplicationBarButtons(bool isActiveMode)
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
            appBarButton2.Text = AppResources.NaptisticsTitle;
            ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += (s, e) =>
            {
                if (InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
                {
                    NavigationService.Navigate(new Uri("/StatisticPage.xaml", UriKind.Relative));
                }
                else
                {
#if DEBUG
                    NavigationService.Navigate(new Uri("/StatisticPage.xaml", UriKind.Relative));
#else
                    NavigationService.Navigate(new Uri("/InAppStorePage.xaml", UriKind.Relative));
#endif                
                }
            };

            // flight mode
            if (isActiveMode)
            {
                ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.nocellular.png", UriKind.Relative));
                appBarButton3.Text = AppResources.AppBarOffline;
                ApplicationBar.Buttons.Add(appBarButton3);
                appBarButton3.Click += async (s, e) =>
                {
                    ConnectivityMessageOut.Begin();
                    await SettingsLauncher.LaunchAirplaneModeAsync();
                };
            }
            else
            {
                ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.cellular.png", UriKind.Relative));
                appBarButton3.Text = AppResources.AppBarOnline;
                ApplicationBar.Buttons.Add(appBarButton3);
                appBarButton3.Click += async (s, e) =>
                {
                    await SettingsLauncher.LaunchAirplaneModeAsync();
                };
            }
            
        }

        /// <summary>
        /// Handles the stop button click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            // ensure the last alarm time is going to be displayed.
            CustomNapTimePicker.Value = AlarmClockViewModel.Instance.LastAlarmDuration;

            DeactivateAnimation.Begin();
            ConnectivityMessageOut.Begin();

            UpdatePlusMinusButtons();
            UpdatePreviewTime();

            BannerOut.Begin();
            _isBannerVisible = false;
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
            ConnectivityMessageIn.Begin();
        }

        /// <summary>
        /// Handles the change of the timespan using one of the buttons.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void ChangeAlarmTimeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;

            if (button == null)
                return;

            int minDelta = 1;
            int.TryParse(button.Tag.ToString(), out minDelta);

            ChangeAlarmTime(minDelta);
        }

        private void ChangeAlarmTime(int minDelta)
        {
            var value = CustomNapTimePicker.Value.Value;

            value = value.Add(TimeSpan.FromMinutes(minDelta));

            // verify at least 1 min
            if (value.TotalMinutes < 1)
                value = TimeSpan.FromMinutes(1);

            CustomNapTimePicker.Value = value;
        }

        private void SetAlarmTime(int minValue)
        {
            var value = TimeSpan.FromMinutes(minValue);

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
            UpdatePlusMinusButtons();
            
            UpdatePreviewTime();
        }

        private void UpdatePlusMinusButtons()
        {
            // update button view state in inactive mode.
            var minutes = (int)CustomNapTimePicker.Value.Value.TotalMinutes;
            bool noAlarmOn = !AlarmClockViewModel.Instance.IsAlarmSet;

            ButtonMinus1.IsEnabled = minutes > 1;
            ButtonPlus1.IsEnabled = true;
        }

        /// <summary>
        /// Updates the preview sleep time.
        /// </summary>
        private void UpdatePreviewTime()
        {
            var minutes = (int)CustomNapTimePicker.Value.Value.TotalMinutes;
            AlarmClockViewModel.Instance.AlarmPreviewTime = DateTime.Now.AddMinutes(minutes);
        }

        public void SetTime(int minutes)
        {
            SetAlarmTime(minutes);
        }
    }
}