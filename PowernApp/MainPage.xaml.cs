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

            Loaded += (s, e) =>
                {
                    // register voice commands
                    Speech.Instance.InstallCommandSets(new Uri("ms-appx:///voicecommands.xml", UriKind.Absolute));
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

            // disable idle detection
            PhoneApplicationService.Current.UserIdleDetectionMode =
                IdleDetectionMode.Disabled;

            if (NavigationContext.QueryString != null &&
                NavigationContext.QueryString.ContainsKey("voiceCommandName"))
            {
                String commandName = NavigationContext.QueryString["voiceCommandName"];

                if (!string.IsNullOrEmpty(commandName))
                    handleVoiceCommands(commandName);

                // clear the QueryString or the page will retain the current value
                NavigationContext.QueryString.Clear();
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
            await Speech.Instance.Synthesizer.SpeakTextAsync("Have a nice " + minutes + " minutes nap.");
        }

        /// <summary>
        /// Handles the stop nap command.
        /// </summary>
        private async void handleStopNapCommand()
        {
            await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm is now off. Hope you feel more energized now.");
        }

        /// <summary>
        /// Handles the check alarm time command.
        /// </summary>
        private async void handleCheckAlarmTime()
        {
            await Speech.Instance.Synthesizer.SpeakTextAsync("Alarm is set for 7:30 pm");
        }

        /// <summary>
        /// Handles the check remaining time command.
        /// </summary>
        private async void handleCheckRemainingTime()
        {
            await Speech.Instance.Synthesizer.SpeakTextAsync("You have 9 minutes left.");
        }

        /// <summary>
        /// Handles the extend alarm time command.
        /// </summary>
        /// <param name="minutes">The minutes to extend the alarm.</param>
        private async void handleExtendAlarmTime(string minutes)
        {
            await Speech.Instance.Synthesizer.SpeakTextAsync("Done. You have now 14 minutes left. Happy napping.");
        }

        /// <summary>
        /// Builds the localizzed application bar.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            // assigns a new application bar to the page.
            ApplicationBar = new ApplicationBar();

            // about
            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.AboutTitle);
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
            appBarMenuItem2.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
            };
        }
    }
}