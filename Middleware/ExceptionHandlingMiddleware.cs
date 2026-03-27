using System.Net;
using System.Text.Json;

namespace CommunityBankTellerAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        // variable that holds a reference to the next middleware in the pipeline
        private readonly RequestDelegate _next;

        // constructor injects the next middleware through dependency injection
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // called automatically by the framework for every incoming HTTP request
        public async Task InvokeAsync(HttpContext context)
        {
            try
                // pass the request to the next middleware or controller
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        // builds and writes a consitent JSON error response for unhandled exceptions
        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new { error = "An unexpected error occured." };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
