using System;
using System.Windows;
using System.Windows.Controls;
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
