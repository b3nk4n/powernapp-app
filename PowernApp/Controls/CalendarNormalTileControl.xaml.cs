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

namespace PowernApp.Controls
{
    public partial class CalendarNormalTileControl : UserControl
    {
        public CalendarNormalTileControl()
        {
            InitializeComponent();

            NapCalendar.DataContext = NapStatisticsViewModel.Instance;
            NapCalendar.Update();
        }
    }
}
