using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StockMarket.WebApi.Models;

namespace StockMarket.WebApi.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override Task OnExceptionAsync(ExceptionContext context)
        {
            HandleException(context);
            return base.OnExceptionAsync(context);
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);
            base.OnException(context);
        }

        private static void HandleException(ExceptionContext context)
        {
            Exception exception = context.Exception;

            string message = exception.Message;
#if DEBUG
            ApiExceptionResponse response = new ApiExceptionResponse(message, exception.StackTrace);
#else
            ApiExceptionResponse response = new ApiExceptionResponse(message);
#endif
            context.Result = new JsonResult(response) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }
}