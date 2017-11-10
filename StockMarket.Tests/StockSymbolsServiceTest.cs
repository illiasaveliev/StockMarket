using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StockMarket.Domain;
using StockMarket.Service;
using StockMarket.Service.Dto;
using StockMarket.Service.Validation;

namespace StockMarket.Tests
{
    [TestClass]
    public class StockSymbolsServiceTest
    {
        private const string User = "User1";
        private const string SymbolName = "Goog";

        [TestMethod]
        public async Task UploadAsyncTest()
        {
            Mock<ICsvReader> csvReaderMock = new Mock<ICsvReader>();
            Mock<IStockSymbolsRepository> stockSymbolsRepositoryMock = new Mock<IStockSymbolsRepository>();
            DateTime mostRecentDate = DateTime.UtcNow;
            stockSymbolsRepositoryMock.Setup(r => r.GetByUserAndSymbolAsync(User, SymbolName, null, null))
                .ReturnsAsync(new List<StockSymbol>());

            StockSymbol[] stockSymbols = GetStockSymbols(mostRecentDate);

            csvReaderMock.Setup(r => r.ReadStockSymbols(User, SymbolName, It.IsAny<Stream>())).Returns(
                new ValidationResponse<IEnumerable<StockSymbol>>(
                    stockSymbols,
                    new List<ValidationResult>()));

            ValidationResponse expectedResponse = new ValidationResponse(new List<ValidationResult>());

            StockSymbolsService stockSymbolsService = new StockSymbolsService(csvReaderMock.Object, stockSymbolsRepositoryMock.Object);

            using (Stream stream = new MemoryStream())
            {
                ValidationResponse result = await stockSymbolsService.UploadAsync(User, SymbolName, stream);
                stockSymbolsRepositoryMock.Verify(r => r.AddRangeAsync(stockSymbols), Times.Once);
                Assert.AreEqual(expectedResponse.IsValid, result.IsValid);
            }
        }

        [TestMethod]
        public async Task UploadInvalidAsyncTest()
        {
            Mock<ICsvReader> csvReaderMock = new Mock<ICsvReader>();
            Mock<IStockSymbolsRepository> stockSymbolsRepositoryMock = new Mock<IStockSymbolsRepository>();
            DateTime mostRecentDate = DateTime.UtcNow;
            stockSymbolsRepositoryMock.Setup(r => r.GetByUserAndSymbolAsync(User, SymbolName, null, null))
                .ReturnsAsync(new List<StockSymbol>());

            StockSymbol[] stockSymbols = GetStockSymbols(mostRecentDate);

            List<ValidationResult> validationResults = new List<ValidationResult> { new ValidationResult(0, "Incorrect Name.") };
            csvReaderMock.Setup(r => r.ReadStockSymbols(User, SymbolName, It.IsAny<Stream>())).Returns(
                new ValidationResponse<IEnumerable<StockSymbol>>(
                    stockSymbols, validationResults));

            ValidationResponse expectedResponse = new ValidationResponse(validationResults);

            StockSymbolsService stockSymbolsService = new StockSymbolsService(csvReaderMock.Object, stockSymbolsRepositoryMock.Object);

            using (Stream stream = new MemoryStream())
            {
                ValidationResponse result = await stockSymbolsService.UploadAsync(User, SymbolName, stream);
                stockSymbolsRepositoryMock.Verify(r => r.AddRangeAsync(stockSymbols), Times.Never);
                Assert.AreEqual(expectedResponse.IsValid, result.IsValid);
                Assert.AreEqual(expectedResponse.ValidationResults.First().Message, result.ValidationResults.First().Message);
            }
        }

        [TestMethod]
        public async Task UploadInvalidPeriodAsyncTest()
        {
            Mock<ICsvReader> csvReaderMock = new Mock<ICsvReader>();
            Mock<IStockSymbolsRepository> stockSymbolsRepositoryMock = new Mock<IStockSymbolsRepository>();
            DateTime mostRecentDate = DateTime.UtcNow;
            stockSymbolsRepositoryMock.Setup(r => r.GetByUserAndSymbolAsync(User, SymbolName, null, null))
                .ReturnsAsync(new List<StockSymbol> { new StockSymbol { Name = SymbolName, Date = mostRecentDate } });

            StockSymbol[] stockSymbols = GetStockSymbols(mostRecentDate);

            List<ValidationResult> validationResults = new List<ValidationResult> { new ValidationResult(0, $"You already have data for this period. Please upload new values starting from {mostRecentDate}.") };
            csvReaderMock.Setup(r => r.ReadStockSymbols(User, SymbolName, It.IsAny<Stream>())).Returns(
                new ValidationResponse<IEnumerable<StockSymbol>>(
                    stockSymbols, new List<ValidationResult>()));

            ValidationResponse expectedResponse = new ValidationResponse(validationResults);

            StockSymbolsService stockSymbolsService = new StockSymbolsService(csvReaderMock.Object, stockSymbolsRepositoryMock.Object);

            using (Stream stream = new MemoryStream())
            {
                ValidationResponse result = await stockSymbolsService.UploadAsync(User, SymbolName, stream);
                stockSymbolsRepositoryMock.Verify(r => r.AddRangeAsync(stockSymbols), Times.Never);
                Assert.AreEqual(expectedResponse.IsValid, result.IsValid);
                Assert.AreEqual(expectedResponse.ValidationResults.First().Message, result.ValidationResults.First().Message);
            }
        }

        [TestMethod]
        public async Task GetStocksAsyncTest()
        {
            Mock<ICsvReader> csvReaderMock = new Mock<ICsvReader>();
            Mock<IStockSymbolsRepository> stockSymbolsRepositoryMock = new Mock<IStockSymbolsRepository>();
            DateTime mostRecentDate = DateTime.UtcNow;

            StockSymbol[] stockSymbols = GetStockSymbols(mostRecentDate);
            stockSymbolsRepositoryMock.Setup(r => r.GetByUserAsync(User)).ReturnsAsync(stockSymbols);

            StockSymbolsService stockSymbolsService = new StockSymbolsService(csvReaderMock.Object, stockSymbolsRepositoryMock.Object);

            StockSymbolInfo[] expected = { new StockSymbolInfo(SymbolName, mostRecentDate) };

            StockSymbolInfo[] result = (await stockSymbolsService.GetStocksAsync(User)).ToArray();

            Assert.AreEqual(expected.Length, result.Length);
            Assert.AreEqual(expected[0].Name, result[0].Name);
            Assert.AreEqual(expected[0].MostRecentDate, result[0].MostRecentDate);
        }

        [TestMethod]
        public async Task GetStockSymbolStatisticsAsyncTest()
        {
            PriceType[] priceTypes = { PriceType.Open, PriceType.Close };
            DateTime mostRecentDate = DateTime.UtcNow;

            StockSymbolStatistics[] expected =
                {
                    new StockSymbolStatistics
                        {
                            Name = SymbolName,
                            Date = mostRecentDate,
                            Avg = 50,
                            Min = 40,
                            Max = 60,
                            Median = 40,
                            Percentile95 = 60,
                            Close = 60,
                            Open = 40,
                            High = 65,
                            Low = 44
                        }
                };

            await GetStockSymbolStatisticsAsync(priceTypes, expected, mostRecentDate);
        }

        [TestMethod]
        public async Task GetStockSymbolStatisticsAllPricesAsyncTest()
        {
            PriceType[] priceTypes = new PriceType[0];
            DateTime mostRecentDate = DateTime.UtcNow;

            StockSymbolStatistics[] expected =
                {
                    new StockSymbolStatistics
                        {
                            Name = SymbolName,
                            Date = mostRecentDate,
                            Avg = 52.25,
                            Min = 40,
                            Max = 65,
                            Median = 44,
                            Percentile95 = 65,
                            Close = 60,
                            Open = 40,
                            High = 65,
                            Low = 44
                        }
                };

            await GetStockSymbolStatisticsAsync(priceTypes, expected, mostRecentDate);
        }

        private static StockSymbol[] GetStockSymbols(DateTime mostRecentDate)
        {
            return new[]
                       {
                           new StockSymbol { Name = SymbolName, Date = mostRecentDate.AddDays(-2) },
                           new StockSymbol { Name = SymbolName, Date = mostRecentDate }
                       };
        }

        private static async Task GetStockSymbolStatisticsAsync(PriceType[] priceTypes, StockSymbolStatistics[] expected, DateTime mostRecentDate)
        {
            Mock<ICsvReader> csvReaderMock = new Mock<ICsvReader>();
            Mock<IStockSymbolsRepository> stockSymbolsRepositoryMock = new Mock<IStockSymbolsRepository>();

            StockSymbol[] stockSymbols = { new StockSymbol { Name = SymbolName, Date = mostRecentDate, Open = 40, Close = 60, High = 65, Low = 44 } };

            DateTime? from = mostRecentDate.AddDays(-10);
            DateTime? to = mostRecentDate.AddDays(1);

            stockSymbolsRepositoryMock.Setup(r => r.GetByUserAndSymbolAsync(User, SymbolName, from, to)).ReturnsAsync(stockSymbols);

            StockSymbolsService stockSymbolsService = new StockSymbolsService(csvReaderMock.Object, stockSymbolsRepositoryMock.Object);

            StockSymbolStatistics[] result = (await stockSymbolsService.GetStockSymbolStatisticsAsync(User, SymbolName, priceTypes, from, to)).ToArray();

            Assert.AreEqual(expected.Length, result.Length);
            Assert.AreEqual(expected[0].Name, result[0].Name);
            Assert.AreEqual(expected[0].Date, result[0].Date);
            Assert.AreEqual(expected[0].Avg, result[0].Avg);
            Assert.AreEqual(expected[0].Min, result[0].Min);
            Assert.AreEqual(expected[0].Max, result[0].Max);
            Assert.AreEqual(expected[0].Median, result[0].Median);
            Assert.AreEqual(expected[0].Percentile95, result[0].Percentile95);
            Assert.AreEqual(expected[0].Open, result[0].Open);
            Assert.AreEqual(expected[0].Close, result[0].Close);
            Assert.AreEqual(expected[0].High, result[0].High);
            Assert.AreEqual(expected[0].Low, result[0].Low);
        }
    }
}