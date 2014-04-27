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
        /// The pages app bar state.
        /// </summary>
        private enum AppBarState
        {
            CleanUp,
            PrevNext
        }

        /// <summary>
        /// The current appbar state.
        /// </summary>
        private AppBarState _currentAppBarState = AppBarState.CleanUp; // TODO: persist state + selectedIndex for tombstoning?

        /// <summary>
        /// Creates a StatisticsPage instance.
        /// </summary>
        public StatisticPage()
        {
            InitializeComponent();

            StatisticPivot.SelectionChanged += (s, e) =>
                {
                    var newAppBarState = GetStateOfPivotIndex(StatisticPivot.SelectedIndex);
                    if (newAppBarState != _currentAppBarState)
                    {
                        _currentAppBarState = newAppBarState;
                        BuildLocalizedAppBar();
                    }
                };

            DataContext = NapStatisticsViewModel.Instance;
            NapCalendar.DataContext = NapStatisticsViewModel.Instance;
            NapCalendar.Update();

            BuildLocalizedAppBar();
        }

        /// <summary>
        /// Builds the localized application bar.
        /// </summary>
        private void BuildLocalizedAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.99;
            ApplicationBar.BackgroundColor = (Color)Application.Current.Resources["ThemeBackgroundMediumColor"];
            ApplicationBar.ForegroundColor = (Color)Application.Current.Resources["ThemeForegroundLightColor"];

            if (StatisticPivot.SelectedIndex == 2)
            {
                // prev month
                ApplicationBarIconButton appBarButtonPrevMonth = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.back.png", UriKind.Relative));
                appBarButtonPrevMonth.Text = "previous";
                ApplicationBar.Buttons.Add(appBarButtonPrevMonth);
                appBarButtonPrevMonth.Click += (s, e) =>
                {
                    NapCalendar.Previous();
                };

                // next month
                ApplicationBarIconButton appBarButtonNextMonth = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.next.png", UriKind.Relative));
                appBarButtonNextMonth.Text = "next";
                ApplicationBar.Buttons.Add(appBarButtonNextMonth);
                appBarButtonNextMonth.Click += (s, e) =>
                {
                    NapCalendar.Next();
                };
            }
            else
            {
                // clean up
                ApplicationBarIconButton appBarButtonCleanUp = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.clean.png", UriKind.Relative));
                appBarButtonCleanUp.Text = "clean up";
                ApplicationBar.Buttons.Add(appBarButtonCleanUp);
                appBarButtonCleanUp.Click += (s, e) =>
                {
                    if (NapStatisticsViewModel.Instance.CleanupCommand.CanExecute(null))
                        NapStatisticsViewModel.Instance.CleanupCommand.Execute(null);
                };
            }
        }

        /// <summary>
        /// Gets the appbar state of the given privot index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The app bar state.</returns>
        private AppBarState GetStateOfPivotIndex(int index)
        {
            if (index == 2)
                return AppBarState.PrevNext;
            else
                return AppBarState.CleanUp;
        }
    }
}