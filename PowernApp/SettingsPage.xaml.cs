using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneKit.Framework.Voice;
using Windows.Phone.Speech.Synthesis;
using PowernApp.Resources;

namespace PowernApp
{
    /// <summary>
    /// The settings page.
    /// </summary>
    public partial class SettingsPage : PhoneApplicationPage
    {
        /// <summary>
        /// Creates a SettingsPage instance.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
                {
                    RefreshVoiceCommandsStatus();
                };
        }

        /// <summary>
        /// Refreshes the voice command status settings.
        /// </summary>
        private void RefreshVoiceCommandsStatus()
        {
            // verify the current language is supported
            if (InstalledVoices.Default.Language == "de-DE" ||
                InstalledVoices.Default.Language == "en-GB")
            {
                StatusText.Text = AppResources.VoiceSupported;
                StatusMessageText.Visibility = Visibility.Collapsed;
            }
            else
            {
                StatusText.Text = AppResources.VoiceUnsupported;
                StatusMessageText.Visibility = Visibility.Visible;
            }

            LanguageText.Text = InstalledVoices.Default.Language;
        }
    }
}