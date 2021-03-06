using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PowernApp.ViewModels;
using System.Windows.Media;
using PowernApp.Resources;

namespace PowernApp
{
    /// <summary>
    /// The statistics page.
    /// </summary>
    public partial class StatisticPage : PhoneApplicationPage
    {
        /// <summary>
        /// The index of the calendar control in the pivot control.
        /// </summary>
        private const int CALENDAR_PIVOT_INDEX = 1;

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

            // install update callback to update the calendar
            NapStatisticsViewModel.Instance.RegisterUpdateCallback(() =>
            {
                NapCalendar.Update();
            });

            BuildLocalizedAppBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NapStatisticsViewModel.Instance.NotifyAll();
            NapCalendar.Update();
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

            if (StatisticPivot.SelectedIndex == CALENDAR_PIVOT_INDEX)
            {
                // prev month
                ApplicationBarIconButton appBarButtonPrevMonth = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.back.png", UriKind.Relative));
                appBarButtonPrevMonth.Text = AppResources.AppBarPrevious;
                ApplicationBar.Buttons.Add(appBarButtonPrevMonth);
                appBarButtonPrevMonth.Click += (s, e) =>
                {
                    NapCalendar.Previous();
                };

                // next month
                ApplicationBarIconButton appBarButtonNextMonth = new ApplicationBarIconButton(new Uri("Assets/AppBar/appbar.next.png", UriKind.Relative));
                appBarButtonNextMonth.Text = AppResources.AppBarNext;
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
                appBarButtonCleanUp.Text = AppResources.AppBarCleanUp;
                ApplicationBar.Buttons.Add(appBarButtonCleanUp);
                appBarButtonCleanUp.Click += (s, e) =>
                {
                    if (NapStatisticsViewModel.Instance.CleanupCommand.CanExecute(null))
                        NapStatisticsViewModel.Instance.CleanupCommand.Execute(null);
                };
            }

            // clear all
            ApplicationBarMenuItem appBarClearMenuItem = new ApplicationBarMenuItem(AppResources.ClearAllTitle);
            ApplicationBar.MenuItems.Add(appBarClearMenuItem);
            appBarClearMenuItem.Click += (s, e) =>
            {
                if (NapStatisticsViewModel.Instance.ClearCommand.CanExecute(null))
                    NapStatisticsViewModel.Instance.ClearCommand.Execute(null);
            };

            // backup
            ApplicationBarMenuItem appBarBackupMenuItem = new ApplicationBarMenuItem(AppResources.BackupTitle);
            ApplicationBar.MenuItems.Add(appBarBackupMenuItem);
            appBarBackupMenuItem.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/BackupPage.xaml", UriKind.Relative));
            };
        }

        /// <summary>
        /// Gets the appbar state of the given privot index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The app bar state.</returns>
        private AppBarState GetStateOfPivotIndex(int index)
        {
            if (index == CALENDAR_PIVOT_INDEX)
                return AppBarState.PrevNext;
            else
                return AppBarState.CleanUp;
        }
    }
}