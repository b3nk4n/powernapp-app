using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowernApp.Napping;

namespace PowernApp.UnitTesting
{
    [TestClass]
    public class NapTimeCalculatorTest
    {
        #region calculateTime

        [TestMethod]
        public void TestCalculateTimeHoursAndMinutes()
        {
            // arrange
            NapTimeCalculator calc = new NapTimeCalculator(DateTime.Now);
            string hours = "1";
            string minutes = "20";
            int expected = 80;

            // act
            int acutal = calc.calculateTime(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeHoursOnly()
        {
            // arrange
            NapTimeCalculator calc = new NapTimeCalculator(DateTime.Now);
            string hours = "2";
            string minutes = "0";
            int expected = 120;

            // act
            int acutal = calc.calculateTime(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeMinztesOnly()
        {
            // arrange
            NapTimeCalculator calc = new NapTimeCalculator(DateTime.Now);
            string hours = "0";
            string minutes = "40";
            int expected = 40;

            // act
            int acutal = calc.calculateTime(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeZero()
        {
            // arrange
            NapTimeCalculator calc = new NapTimeCalculator(DateTime.Now);
            string hours = "0";
            string minutes = "0";
            int expected = 0;

            // act
            int acutal = calc.calculateTime(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateInvalidString()
        {
            // arrange
            NapTimeCalculator calc = new NapTimeCalculator(DateTime.Now);
            string hours = "abc";
            string minutes = "10";
            int expected = 10;

            // act
            int acutal = calc.calculateTime(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        #endregion

        #region calculateTimeUntilTimeAs24Clock

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs24ClockWithZeroSeconds()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 12, 0, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "16";
            string minutes = "40";
            int expected = 4 * 60 + 40;

            // act
            int acutal = calc.calculateTimeUntilTimeAs24Clock(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs24ClockWithoutZeroSeconds()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 13, 0, 20);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "16";
            string minutes = "40";
            int expected = 3 * 60 + 40;

            // act
            int acutal = calc.calculateTimeUntilTimeAs24Clock(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs24ClockSameTime()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 13, 20, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "13";
            string minutes = "20";
            int expected = 0;

            // act
            int acutal = calc.calculateTimeUntilTimeAs24Clock(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs24ClockSameTimeWithSeconds()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 13, 20, 10);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "13";
            string minutes = "20";
            int expected = 60 * 24;

            // act
            int acutal = calc.calculateTimeUntilTimeAs24Clock(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs24ClockNextDay()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 22, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "0";
            string minutes = "50";
            int expected = 2 * 60 + 10;

            // act
            int acutal = calc.calculateTimeUntilTimeAs24Clock(hours, minutes);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        #endregion 

        #region calculateTimeUntilTimeAs12ClockUS

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockUS()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            string minutes = "50";
            string identifier12 = NapTimeCalculator.IDENTIFIER12_AM;
            int expected = 2 * 60 + 10;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockUS(hours, minutes, identifier12);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockUSWithSeconds()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 40, 30);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            string minutes = "50";
            string identifier12 = NapTimeCalculator.IDENTIFIER12_AM;
            int expected = 2 * 60 + 10;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockUS(hours, minutes, identifier12);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockUSIntervalJumpFromAmToPm()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 9, 30, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "1";
            string minutes = "25";
            string identifier12 = NapTimeCalculator.IDENTIFIER12_PM;
            int expected = 3 * 60 + 55;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockUS(hours, minutes, identifier12);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockUSIntervalJumpFromPmToAm()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 22, 30, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "1";
            string minutes = "25";
            string identifier12 = NapTimeCalculator.IDENTIFIER12_AM;
            int expected = 2 * 60 + 55;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockUS(hours, minutes, identifier12);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        #endregion

        #region calculateTimeUntilTimeAs12ClockDEText

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockDETextQuarterTo()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            string minText = NapTimeCalculator.DE_MINTEXT_QUARTER_TO;
            int expected = 1 * 60 + 5;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockDEText(hours, minText);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockDETextHalf()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 6, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            string minText = NapTimeCalculator.DE_MINTEXT_HALF;
            int expected = 1 * 60 + 50;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockDEText(hours, minText);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockDETextQuarterPast()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            string minText = NapTimeCalculator.DE_MINTEXT_QUARTER_PAST;
            int expected = 1 * 60 + 35;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockDEText(hours, minText);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockDETextQuarterToNextInterval()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 30, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "3";
            string minText = NapTimeCalculator.DE_MINTEXT_QUARTER_TO;
            int expected = 7 * 60 + 15;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockDEText(hours, minText);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockDETextHalfNextInterval()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 11, 20, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "2";
            string minText = NapTimeCalculator.DE_MINTEXT_HALF;
            int expected = 2 * 60 + 10;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockDEText(hours, minText);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockDETextQuarterPastNextInterval()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 10, 35, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "1";
            string minText = NapTimeCalculator.DE_MINTEXT_QUARTER_PAST;
            int expected = 2 * 60 + 40;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockDEText(hours, minText);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        #endregion

        #region calculateTimeUntilTimeAs12ClockGBText

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBTextQuarterTo()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            string minText = NapTimeCalculator.EN_MINTEXT_QUARTER;
            string relative = NapTimeCalculator.EN_RELATIVE_TO;
            int expected = 1 * 60 + 5;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGBText(hours, minText, relative);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBTextHalf()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 6, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            string minText = NapTimeCalculator.EN_MINTEXT_HALF;
            string relative = NapTimeCalculator.EN_RELATIVE_TO;
            int expected = 1 * 60 + 50;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGBText(hours, minText, relative);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBTextQuarterPast()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            string minText = NapTimeCalculator.EN_MINTEXT_QUARTER;
            string relative = NapTimeCalculator.EN_RELATIVE_PAST;
            int expected = 1 * 60 + 35;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGBText(hours, minText, relative);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBTextQuarterToNextInterval()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 30, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "3";
            string minText = NapTimeCalculator.EN_MINTEXT_QUARTER;
            string relative = NapTimeCalculator.EN_RELATIVE_TO;
            int expected = 7 * 60 + 15;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGBText(hours, minText, relative);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBTextHalfNextInterval()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 11, 20, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "2";
            string minText = NapTimeCalculator.EN_MINTEXT_HALF;
            string relative = NapTimeCalculator.EN_RELATIVE_TO;
            int expected = 2 * 60 + 10;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGBText(hours, minText, relative);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBTextQuarterPastNextInterval()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 10, 35, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "1";
            string minText = NapTimeCalculator.EN_MINTEXT_QUARTER;
            string relative = NapTimeCalculator.EN_RELATIVE_PAST;
            int expected = 2 * 60 + 40;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGBText(hours, minText, relative);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        #endregion

        #region calculateTimeUntilTimeAs12ClockGB

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGB()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 40, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            int expected = 1 * 60 + 20;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGB(hours);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBWithSeconds()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 7, 40, 30);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "9";
            int expected = 1 * 60 + 20;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGB(hours);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBIntervalJumpFromAmToPm()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 9, 30, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "1";
            int expected = 3 * 60 + 30;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGB(hours);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        [TestMethod]
        public void TestCalculateTimeUntilTimeAs12ClockGBIntervalJumpFromPmToAm()
        {
            // arrange
            DateTime now = new DateTime(2014, 1, 1, 22, 30, 0);
            NapTimeCalculator calc = new NapTimeCalculator(now);
            string hours = "1";
            int expected = 2 * 60 + 30;

            // act
            int acutal = calc.calculateTimeUntilTimeAs12ClockGB(hours);

            // assert
            Assert.AreEqual(expected, acutal);
        }

        #endregion
    }
}
