using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockMarket.Service.Extensions;

namespace StockMarket.Tests
{
    [TestClass]
    public class EnumerableExtensionsTest
    {
        private readonly double[] values = { 43, 54, 56, 61, 62, 66, 68, 69, 69, 70, 71, 72, 77, 78, 79, 85, 87, 88, 89, 93, 95, 96, 98, 99, 99 };

        [TestMethod]
        public void PercentileCalculationTest()
        {
            double percentile = values.Percentile(0.95);
            Assert.AreEqual(99, percentile);
        }


        [TestMethod]
        public void MedianCalculationTest()
        {
            double median = values.Percentile(0.5);
            Assert.AreEqual(77, median);
        }
    }
}