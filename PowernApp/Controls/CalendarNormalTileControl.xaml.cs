using System.Windows.Controls;
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
