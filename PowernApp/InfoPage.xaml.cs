using System;
using Microsoft.Phone.Controls;
using PhoneKit.Framework.OS;
using PowernApp.Resources;
using System.Windows.Media.Imaging;

namespace PowernApp
{
    public partial class InfoPage : PhoneApplicationPage
    {
        public InfoPage()
        {
            InitializeComponent();

            UpdateVersionDependentContent();
        }

        /// <summary>
        /// Updates the special content for WP 8.1 OS version.
        /// </summary>
        /// <remarks>
        /// The default content is based on WP8.0 OS. 
        /// </remarks>
        private void UpdateVersionDependentContent()
        {
            if (VersionHelper.IsPhoneWP8_1)
            {
                VoiceCommandsActivationDescription.Text = AppResources.Commands1Message_8_1_OS;
                VoiceCommandsActivationImage.Source = new BitmapImage(new Uri("/Assets/Images/search.png", UriKind.Relative));
            }
        }
    }
}