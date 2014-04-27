using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Globalization;
using PowernApp.ViewModels;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace PowernApp.Controls
{
    public partial class NapCalendarControl : UserControl
    {
        private DateTime _currentDate = DateTime.Now;

        /// <summary>
        /// The white color brush for reuse.
        /// </summary>
        private readonly Brush WHITE_BRUSH = new SolidColorBrush(Colors.White);

        public NapCalendarControl()
        {
            InitializeComponent();
        }

        public void Update()
        {
            UpdateHeaderUI();
            CreateCalendar(_currentDate.Year, _currentDate.Month);
        }

        public void Next()
        {
            _currentDate = _currentDate.AddMonths(1);
            Update();
        }

        public void Previous()
        {
            _currentDate = _currentDate.AddMonths(-1);
            Update();
        }

        private void UpdateHeaderUI()
        {
            MonthHeader.Text = _currentDate.ToString("MMMM yyyy");
            MoHeader.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Monday).Substring(0, 2);
            TuHeader.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Tuesday).Substring(0, 2);
            WeHeader.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Wednesday).Substring(0, 2);
            ThHeader.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Thursday).Substring(0, 2);
            FrHeader.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Friday).Substring(0, 2);
            SaHeader.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Saturday).Substring(0, 2);
            SuHeader.Text = DateTimeFormatInfo.CurrentInfo.GetDayName(DayOfWeek.Sunday).Substring(0, 2);
        }

        private void CreateCalendar(int year, int month)
        {
            // clear calendar container
            CalendarGrid.Children.Clear();

            var date = new DateTime(year, month, 1);

            int firstDayOfWeek = GetDayOfWeekAsInt(date.DayOfWeek);
            int nextDaysOffset = 0;

            nextDaysOffset += CreateCalendarPreviousMonth(date, firstDayOfWeek);
            nextDaysOffset += CreateCalendarActiveMonth(date, firstDayOfWeek);
            CreateCalendarNextMonth(date, nextDaysOffset);
        }

        private int CreateCalendarPreviousMonth(DateTime activeMonth, int firstDayOfWeek)
        {
            var dataViewModel = DataContext as NapStatisticsViewModel;
            DateTime previousMonth = activeMonth.AddMonths(-1);

            int daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            var currentDayCounter = daysInPreviousMonth;
            int dayCounter = 0;

            for (int c = firstDayOfWeek - 1; c >= 0; c--)
            {
                dayCounter++;
                var isNapDay = dataViewModel.HasNapAtDate(new DateTime(previousMonth.Year, previousMonth.Month, dayCounter));
                var dayControl = CreateDayControl(currentDayCounter.ToString(), false, isNapDay);
                Grid.SetRow(dayControl, 0);
                Grid.SetColumn(dayControl, c);
                CalendarGrid.Children.Add(dayControl);
            }
            return dayCounter;
        }

        private int CreateCalendarActiveMonth(DateTime activeMonth, int firstDayOfWeek)
        {
            var dataViewModel = DataContext as NapStatisticsViewModel;
            int daysInMonth = DateTime.DaysInMonth(activeMonth.Year, activeMonth.Month);
            int dayCounter = 0;
            for (int r = 0; r < 6; r++)
            {
                int columnOffset = 0;
                // column offset for the first row
                if (r == 0)
                    columnOffset = firstDayOfWeek;

                for (int c = columnOffset; c < 7; c++)
                {
                    var currentDayCounter = dayCounter + 1;
                    if (currentDayCounter > daysInMonth)
                        break;

                    var isNapDay = dataViewModel.HasNapAtDate(new DateTime(activeMonth.Year, activeMonth.Month, currentDayCounter));
                    var dayControl = CreateDayControl(currentDayCounter.ToString(), true, isNapDay);
                    Grid.SetRow(dayControl, r);
                    Grid.SetColumn(dayControl, c);
                    CalendarGrid.Children.Add(dayControl);

                    dayCounter++;
                }
            }
            return dayCounter;
        }

        /// <summary>
        /// Crates the next month days of the calendar.
        /// </summary>
        /// <param name="activeMonth">The currently shown month.</param>
        /// <param name="nextDaysOffset">The number of days already rendered to know where to start.</param>
        private void CreateCalendarNextMonth(DateTime activeMonth, int nextDaysOffset)
        {
            var dataViewModel = DataContext as NapStatisticsViewModel;
            DateTime nextMonth = activeMonth.AddMonths(1);

            var dayCounter = 0;

            for (int r = (nextDaysOffset / 6) - 1; r < 6; r++)
            {
                int columnOffset = 0;
                // column offset for the first row
                if (r == (nextDaysOffset / 6) - 1)
                    columnOffset = nextDaysOffset % 7;

                for (int c = columnOffset; c < 7; c++)
                {
                    var currentDayCounter = dayCounter + 1;

                    var isNapDay = dataViewModel.HasNapAtDate(new DateTime(nextMonth.Year, nextMonth.Month, currentDayCounter));
                    var dayControl = CreateDayControl(currentDayCounter.ToString(), false, isNapDay);
                    Grid.SetRow(dayControl, r);
                    Grid.SetColumn(dayControl, c);
                    CalendarGrid.Children.Add(dayControl);

                    dayCounter++;
                }
            }
        }

        private FrameworkElement CreateDayControl(string text, bool isActive, bool hasNap)
        {
            Grid container = new Grid();
            if (!isActive)
                container.Opacity = 0.33;
            container.Height = 50;
            container.Width = 50;
            var ellipse = new Ellipse();
            ellipse.Stroke = WHITE_BRUSH;
            ellipse.StrokeThickness = 3;
            container.Width = 46;
            container.Height = 46;
            container.Children.Add(ellipse);
            if (hasNap)
            {
                // set image source in code, because the XAML implementation is asyc,
                // so it is not going to be rendered.
                Image image = new Image();
                Uri uri = new Uri("/PowernAPP;component/Assets/Images/check.png", UriKind.Relative);
                StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(resourceInfo.Stream);
                image.Source = bmp;
                container.Children.Add(image);
            }
            else
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = text;
                textBlock.Foreground = WHITE_BRUSH;
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                container.Children.Add(textBlock);
            }
            return container;
        }

        /// <summary>
        /// Gets the day of week in the interval [0..6]-
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <returns>The day of week as integer.</returns>
        private int GetDayOfWeekAsInt(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
               case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                case DayOfWeek.Sunday:
                    return 6;
            }

            return -1;
        }
    }
}
