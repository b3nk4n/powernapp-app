#define TESTING

using BugSense;
using BugSense.Core.Model;
using Microsoft.Phone.Shell;
using PhoneKit.Framework.Core.Graphics;
using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Storage;
using PhoneKit.Framework.Core.Tile;
using PowernApp.Controls;
using PowernApp.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PowernApp.ViewModels
{
    /// <summary>
    /// The nap statistics view model.
    /// </summary>
    public class NapStatisticsViewModel : ViewModelBase
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static NapStatisticsViewModel instance;

        /// <summary>
        /// The nap data list.
        /// </summary>
        ObservableCollection<NapDataViewModel> _napList = new ObservableCollection<NapDataViewModel>();

        /// <summary>
        /// Indicates whether the data is loaded.
        /// </summary>
        private bool _isDataLoaded;

        /// <summary>
        /// Gets or sets whether the nap tracking / statistic recording is active.
        /// </summary>
        public bool IsTrackingActive { get; set; }

        /// <summary>
        /// The cleanup command.
        /// </summary>
        private DelegateCommand _cleanupCommand;

        /// <summary>
        /// Creates a NapStatisticsViewModel instance.
        /// </summary>
        /// <remarks>
        /// Do not call the constructor directly. Use <code>NapStatisticsViewModel.Instance</code>, please. The constructor is public
        /// for design time data purposes.
        /// </remarks>
        public NapStatisticsViewModel()
        {
            Load();

            _cleanupCommand = new DelegateCommand(() =>
                {
                    if (MessageBox.Show(AppResources.MessageBoxCleanUp, AppResources.MessageBoxAttention, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        Cleanup(10);
                    }
                },
                () =>
                {
                    return _napList.Count > 0;
                });
        }

        /// <summary>
        /// Cleans up all commands shorter than x minutes.
        /// </summary>
        /// <param name="minLimit"> The limit of which is the minimum allowed nap time in minutes.</param>
        private void Cleanup(int minLimit)
        {
            int listStartIndex = 0;

            // skip first element when alarm is active
            if (AlarmClockViewModel.Instance.IsAlarmSet)
                listStartIndex = 1;

            for (int i = _napList.Count - 1; i >= listStartIndex; --i)
            {
                if (_napList[i].Duration < minLimit)
                    _napList.RemoveAt(i);
            }
        }

        /// <summary>
        /// Loads the statistics data.
        /// </summary>
        public void Load()
        {
            if (_isDataLoaded)
                return;

#if !TESTING
            if (DesignerProperties.IsInDesignTool)
            {
#endif
                // design time data
                NapList.Add(new NapDataViewModel(new DateTime(2014, 4, 27, 14, 30, 0), 30));
                NapList.Add(new NapDataViewModel(new DateTime(2014, 4, 23, 12, 25, 0), 45));
                NapList.Add(new NapDataViewModel(new DateTime(2014, 4, 10, 13, 38, 0), 35));
                NapList.Add(new NapDataViewModel(new DateTime(2014, 4, 02, 12, 45, 0), 30));
                NapList.Add(new NapDataViewModel(new DateTime(2014, 3, 31, 13, 38, 0), 25));
#if !TESTING
            }
            else
            {
                // load real data.
                var loadedData = StorageHelper.LoadSerializedFile<ObservableCollection<NapDataViewModel>>("statistics.data");
                if (loadedData != null)
                    _napList = loadedData;
            }
#endif

            _isDataLoaded = true;
        }

        /// <summary>
        /// Saves the statistics data.
        /// </summary>
        public void Save()
        {
            StorageHelper.SaveAsSerializedFile("statistics.data", _napList);
        }

        /// <summary>
        /// Changes the duration of the current nap data (which is is first one of the list)
        /// </summary>
        /// <param name="delta"></param>
        public void ChangeDurationOfCurrentNap(int delta)
        {
            if (_napList.Count == 0)
                return;

            _napList[0].ChangeDuration(delta);
        }

        /// <summary>
        /// Checks whether a not with the given date exists.
        /// </summary>
        /// <param name="date">The year, month and day.</param>
        /// <returns>Returns TRUE when a nap exists, else FALSE.</returns>
        public bool HasNapAtDate(DateTime date)
        {
            foreach (var nap in _napList)
            {
                if (nap.StartTime.Date == date.Date)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the main live tile.
        /// </summary>
        public void UpdateLiveTile()
        {
            try
            {
                var image = GraphicsHelper.Create(new CalendarNormalTileControl());
                Uri imageUri = StorageHelper.SavePng(LiveTileHelper.SHARED_SHELL_CONTENT_PATH + "normalCalendar.png", image);
                var wideImage = GraphicsHelper.Create(new CalendarWideTileControl());
                Uri wideImageUri = StorageHelper.SavePng(LiveTileHelper.SHARED_SHELL_CONTENT_PATH + "wideCalendar.png", wideImage);
                LiveTileHelper.UpdateDefaultTile(
                    new FlipTileData
                    {
                        Title = AppResources.ApplicationTitle,
                        SmallBackgroundImage = new Uri("Assets/Tiles/FlipCycleTileSmall.png", UriKind.Relative),
                        BackgroundImage = new Uri("Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative),
                        WideBackgroundImage = new Uri("Assets/Tiles/FlipCycleTileLarge.png", UriKind.Relative),
                        BackBackgroundImage = imageUri,
                        WideBackBackgroundImage = wideImageUri,
                    });
            }
            catch (Exception ex)
            {
                // paranoia-catch block
                BugSenseLogResult logResult = BugSenseHandler.Instance.LogException(ex, "tileUpdate", "Failed to update the tile.");
                Debug.WriteLine("Bugsense:" + logResult.ResultState.ToString());
            }
        }

        /// <summary>
        /// Gets the NapStatisticsViewModel instance.
        /// </summary>
        public static NapStatisticsViewModel Instance
        {
            get
            {
                if (instance == null)
                    instance = new NapStatisticsViewModel();
                return instance;
            }
        }

        /// <summary>
        /// Adds a new nap data item to the beginning of the list.
        /// </summary>
        /// <param name="nap">The nap data.</param>
        public void AddNap(NapDataViewModel nap)
        {
            _napList.Insert(0, nap);
        }

        /// <summary>
        /// Gets the nap list.
        /// </summary>
        public ObservableCollection<NapDataViewModel> NapList
        {
            get
            {
                return _napList;
            }
        }

        /// <summary>
        /// Gets the number of recorded naps.
        /// </summary>
        public int NapsCount
        {
            get
            {
                return _napList.Count;
            }
        }

        /// <summary>
        /// Gets the minimum nap time in minutes.
        /// </summary>
        public int MinNapTime
        {
            get
            {
                if (NapList.Count == 0)
                    return 0;

                return NapList.Min(nap => nap.Duration);
            }
        }

        /// <summary>
        /// Gets the maximum nap time in minutes.
        /// </summary>
        public int MaxNapTime
        {
            get
            {
                if (NapList.Count == 0)
                    return 0;

                return NapList.Max(nap => nap.Duration);
            }
        }

        /// <summary>
        /// Gets the average nap time.
        /// </summary>
        public double AvgNapTime
        {
            get
            {
                if (NapList.Count == 0)
                    return 0;

                return NapList.Average(nap => nap.Duration);
            }
        }

        /// <summary>
        /// Gets the time span since the end of the last nap.
        /// </summary>
        public TimeSpan TimeSinceLastNap
        {
            get
            {
                if (NapList.Count == 0)
                    return TimeSpan.Zero;

                var timespan = DateTime.Now - NapList[0].StartTime.AddMinutes(NapList[0].Duration);

                if (timespan < TimeSpan.Zero)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return timespan;
                }
                
            }
        }

        /// <summary>
        /// Gets the cleanup command.
        /// </summary>
        public ICommand CleanupCommand
        {
            get
            {
                return _cleanupCommand;
            }
        }
    }
}
