using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PowernApp.ViewModels;
using System.Windows.Media;

namespace PowernApp
{
    /// <summary>
    /// The statistics page.
    /// </summary>
    public partial class StatisticPage : PhoneApplicationPage
    {
        /// <summary>
        /// Creates a StatisticsPage instance.
        /// </summary>
        public StatisticPage()
        {
            InitializeComponent();

            DataContext = NapStatisticsViewModel.Instance;

            BuildLocalizedAppBar();
        }

        /// <summary>
        /// Builds the localized application bar.
        /// </summary>
        private void BuildLocalizedAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = (Color)Application.Current.Resources["ThemeBackgroundMediumColor"];
            ApplicationBar.ForegroundColor = (Color)Application.Current.Resources["ThemeForegroundLightColor"];

            // clean up
            ApplicationBarIconButton appBarButton1 = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.clean.png", UriKind.Relative));
            appBarButton1.Text = "clean up";
            ApplicationBar.Buttons.Add(appBarButton1);
            appBarButton1.Click += (s, e) =>
            {
                if (NapStatisticsViewModel.Instance.CleanupCommand.CanExecute(null))
                    NapStatisticsViewModel.Instance.CleanupCommand.Execute(null);
            };
        }
    }
}