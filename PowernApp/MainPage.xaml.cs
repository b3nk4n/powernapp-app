using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PowernApp.Resources;
using PhoneKit.Framework.Voice;
using PowernApp.ViewModels;
using Coding4Fun.Toolkit.Controls;
using PhoneKit.Framework.OS;

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
                    DataContext = AlarmClockViewModel.Instance;
                };

            ActivateAnimation.Completed += (s, e) =>
                {
                    UpdateViewState();
                };
            DeactivateAnimation.Completed += (s, e) =>
            {
                UpdateViewState();
            };

            BuildLocalizedApplicationBar();
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

            // determine view state
            UpdateViewState();
        }

        /// <summary>
        /// When the main page is navigates from.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // ensure sound and vibration is off
            AlarmClockViewModel.Instance.ForceStopSoundAndVibration();
        }

        /// <summary>
        /// Updates the view state of the main page and the application bar
        /// depending on whether the alarm is on or off.
        /// </summary>
        private void UpdateViewState()
        {
            if (AlarmClockViewModel.Instance.IsAlarmSet)
            {
                ActivePanel.Visibility = Visibility.Visible;
                InactivePanel.Visibility = Visibility.Collapsed;
                BuildActiveLocalizedApplicationBar();
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
            string minutes;
            switch (commandName)
            {
                case "startNap":
                    minutes = NavigationContext.QueryString["minute"];
                    handleStartNapCommand(minutes);
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
        private async void handleStartNapCommand(string minutes)
        {
            int min = int.Parse(minutes);

            if (AlarmClockViewModel.Instance.Set(min))
                await Speech.Instance.Synthesizer.SpeakTextAsync("Have a nice " + minutes + " minutes nap.");
            else
                await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm is already set.");
        }

        /// <summary>
        /// Handles the stop nap command.
        /// </summary>
        private async void handleStopNapCommand()
        {
            if (AlarmClockViewModel.Instance.Stop())
                await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm is now off. Hope you feel more energized now.");
            else
                await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm was not set.");
        }

        /// <summary>
        /// Handles the check alarm time command.
        /// </summary>
        private async void handleCheckAlarmTime()
        {
            if (AlarmClockViewModel.Instance.IsAlarmSet)
                await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm is set for " + AlarmClockViewModel.Instance.AlarmTime.ToString("t")); // 12:12 PM
            else
                await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm was not set.");
        }

        /// <summary>
        /// Handles the check remaining time command.
        /// </summary>
        private async void handleCheckRemainingTime()
        {
            if (AlarmClockViewModel.Instance.IsAlarmSet)
                await Speech.Instance.Synthesizer.SpeakTextAsync(string.Format("You have {0} minutes left.", (int)AlarmClockViewModel.Instance.TimeToAlarm.TotalMinutes));
            else
                await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm was not set.");
        }

        /// <summary>
        /// Handles the extend alarm time command.
        /// </summary>
        /// <param name="minutes">The minutes to extend the alarm.</param>
        private async void handleExtendAlarmTime(string minutes)
        {
            int min = int.Parse(minutes);

            if (AlarmClockViewModel.Instance.Snooze(min))
                await Speech.Instance.Synthesizer.SpeakTextAsync(string.Format("You have now {0} minutes left. Happy napping.", (int)AlarmClockViewModel.Instance.TimeToAlarm.TotalMinutes));
            else
                await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm was not set.");
        }

        /// <summary>
        /// Builds the localized application bar with all list items.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            // assigns a new application bar to the page.
            ApplicationBar = new ApplicationBar();

            // about
            ApplicationBarMenuItem appBarMenuItem1 = new ApplicationBarMenuItem(AppResources.AboutTitle);
            ApplicationBar.MenuItems.Add(appBarMenuItem1);
            appBarMenuItem1.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
            };

            // settings
            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.SettingsTitle);
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
            appBarMenuItem2.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
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
            appBarButton1.Text = "info";
            ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += (s, e) =>
            {
                // TODO: navigate to info page
            };

            // glight mode
            ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.nocellular.png", UriKind.Relative));
            appBarButton2.Text = "disable connectivity";
            ApplicationBar.Buttons.Add(appBarButton2);
            appBarButton2.Click += async (s, e) =>
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
            appBarButton1.Text = "info";
            ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += (s, e) =>
            {
                // TODO: navigate to info page
            };
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

            int minDelta = int.Parse(button.Tag.ToString());
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