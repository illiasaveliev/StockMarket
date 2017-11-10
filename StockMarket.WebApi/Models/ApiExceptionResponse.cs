namespace StockMarket.WebApi.Models
{
    internal class ApiExceptionResponse
    {
        public ApiExceptionResponse(string message)
        {
            Message = message;
        }

        public ApiExceptionResponse(string message, string details)
        {
            Message = message;
            Details = details;
        }

        public string Message { get; }

        public string Details { get; }
    }
}