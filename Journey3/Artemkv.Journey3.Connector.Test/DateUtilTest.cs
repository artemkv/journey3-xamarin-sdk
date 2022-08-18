using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemkv.Journey3.Connector.Test
{
    [TestClass]
    public class DateUtilTest
    {
        [TestMethod]
        public void TestSameYearNotSameMonth()
        {
            var date1 = new DateTime(2020, 1, 1, 0, 0, 0);
            var date2 = new DateTime(2020, 2, 3, 1, 2, 3);

            Assert.IsTrue(date1.IsSameYear(date2));
            Assert.IsFalse(date1.IsSameMonth(date2));
        }

        [TestMethod]
        public void TestSameMonthNotSameDay()
        {
            var date1 = new DateTime(2020, 1, 1, 0, 0, 0);
            var date2 = new DateTime(2020, 1, 3, 1, 2, 3);

            Assert.IsTrue(date1.IsSameMonth(date2));
            Assert.IsFalse(date1.IsSameDay(date2));
        }

        [TestMethod]
        public void TestSameDayNotSameHour()
        {
            var date1 = new DateTime(2020, 1, 1, 0, 0, 0);
            var date2 = new DateTime(2020, 1, 1, 1, 2, 3);

            Assert.IsTrue(date1.IsSameDay(date2));
            Assert.IsFalse(date1.IsSameHour(date2));
        }

        [TestMethod]
        public void TestSameHour()
        {
            var date1 = new DateTime(2020, 1, 1, 0, 0, 0);
            var date2 = new DateTime(2020, 1, 1, 0, 2, 3);

            Assert.IsTrue(date1.IsSameHour(date2));
        }
    }
}
