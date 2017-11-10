using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StockMarket.Domain;
using StockMarket.Service.Validation;

namespace StockMarket.Service
{
    public class CsvReader : ICsvReader
    {
        private const char CsvSeparator = ',';
        private const int HeaderRowsCount = 1;
        private const int ColumnsCount = 6;

        public ValidationResponse<IEnumerable<StockSymbol>> ReadStockSymbols(string user, string symbolName, Stream data)
        {
            List<StockSymbol> stockSymbols = new List<StockSymbol>();
            List<ValidationResult> validationResults = new List<ValidationResult>();

            int rowCounter = 1;
            using (StreamReader row = new StreamReader(data))
            {
                while (!row.EndOfStream)
                {
                    string rowData = row.ReadLine();
                    if (rowCounter > HeaderRowsCount)
                    {
                        ValidationResponse<StockSymbol> symbolResult = ReadSymbolData(user, symbolName, rowData, rowCounter);

                        if (symbolResult.IsValid)
                        {
                            stockSymbols.Add(symbolResult.Result);
                        }
                        validationResults.AddRange(symbolResult.ValidationResults);
                    }

                    rowCounter++;
                }
            }

            return new ValidationResponse<IEnumerable<StockSymbol>>(stockSymbols, validationResults);
        }

        private static ValidationResponse<StockSymbol> ReadSymbolData(string user, string symbolName, string rowData, int rowCounter)
        {
            string[] cells = rowData.Split(CsvSeparator);

            List<ValidationResult> validationResults = new List<ValidationResult>();

            if (cells.Length != ColumnsCount)
            {
                validationResults.Add(new ValidationResult(rowCounter, "The row format is incorrect."));
                return new ValidationResponse<StockSymbol>(null, validationResults);
            }

            DateTime? date = ParseDate(cells[0]);

            if (date == null)
            {
                validationResults.Add(new ValidationResult(rowCounter, $"Date is invalid: {cells[0]}"));
            }

            TryGetDoubleValue(rowCounter, cells[1], "Open", validationResults, out double open);
            TryGetDoubleValue(rowCounter, cells[2], "High", validationResults, out double high);
            TryGetDoubleValue(rowCounter, cells[3], "Low", validationResults, out double low);
            TryGetDoubleValue(rowCounter, cells[4], "Close", validationResults, out double close);
            TryGetDoubleValue(rowCounter, cells[5], "Volume", validationResults, out double volume);

            if (!validationResults.Any())
            {
                return new ValidationResponse<StockSymbol>(
                    new StockSymbol
                    {
                        Name = symbolName,
                        UserName = user,
                        Date = date.Value,
                        Open = open,
                        High = high,
                        Low = low,
                        Close = close,
                        Volume = volume
                    },
                    validationResults);
            }

            return new ValidationResponse<StockSymbol>(null, validationResults);
        }

        private static bool TryGetDoubleValue(int rowCounter, string value, string name, List<ValidationResult> validationResults, out double result)
        {
            double? parseResult = ParseDouble(value);
            result = 0;
            if (parseResult == null)
            {
                validationResults.Add(new ValidationResult(rowCounter, $"{name} is invalid: {value}"));
                return false;
            }

            result = parseResult.Value;
            return true;
        }

        private static double? ParseDouble(string value)
        {
            return double.TryParse(value, out double result) ? (double?)result : null;
        }

        private static DateTime? ParseDate(string value)
        {
            return DateTime.TryParse(value, out DateTime result) ? (DateTime?)result : null;
        }
    }
}