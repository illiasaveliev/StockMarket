namespace StockMarket.Service.Validation
{
    public class ValidationResult
    {
        public ValidationResult(int rowNumber, string message)
        {
            Message = message;
            RowNumber = rowNumber;
        }

        public string Message { get; }

        public int RowNumber { get; }
    }
}