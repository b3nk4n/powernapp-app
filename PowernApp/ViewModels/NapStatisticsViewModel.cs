using PhoneKit.Framework.Core.MVVM;
using PhoneKit.Framework.Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    class NapStatisticsViewModel : ViewModelBase
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
        private NapStatisticsViewModel()
        {
            Load();

            _cleanupCommand = new DelegateCommand(() =>
                {
                    if (MessageBox.Show("Wirklich alle power naps aus den Statistiken löschen, die kürzer als 5 Minuten waren?", "Info", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        Cleanup(5);
                    }
                },
                () =>
                {
                    return true;
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

            var loadedData = StorageHelper.LoadSerializedFile<ObservableCollection<NapDataViewModel>>("statistics.data");

            if (loadedData != null)
                _napList = loadedData;

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

                return DateTime.Now - NapList[0].StartTime.AddMinutes(NapList[0].Duration);
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
