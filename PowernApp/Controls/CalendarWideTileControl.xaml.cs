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
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace PowernApp.Controls
{
    public partial class CalendarWideTileControl : UserControl
    {
        public CalendarWideTileControl()
        {
            InitializeComponent();

            NapCalendar.DataContext = NapStatisticsViewModel.Instance;

            // values manually
            NapsCount.Text = NapStatisticsViewModel.Instance.NapsCount.ToString();
            MinNapTime.Text = string.Format("{0:00}", NapStatisticsViewModel.Instance.MinNapTime);
            AvgNapTime.Text = string.Format("{0:00}", NapStatisticsViewModel.Instance.AvgNapTime);
            MaxNapTime.Text = string.Format("{0:00}", NapStatisticsViewModel.Instance.MaxNapTime);

            NapCalendar.Update();

            Uri uri = new Uri("/PowernAPP;component/Assets/Images/tileLogo.png", UriKind.Relative);
            StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);
            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(resourceInfo.Stream);
            LogoImage.Source = bmp;
        }
    }
}
