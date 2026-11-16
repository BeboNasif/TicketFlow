using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TicketFlow.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var problemDetails = exception switch
            {
                KeyNotFoundException => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Resource Not Found",
                    Detail = exception.Message
                },
                InvalidOperationException => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Invalid Operation",
                    Detail = exception.Message
                },
                ArgumentException => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Bad Request",
                    Detail = exception.Message
                },
                UnauthorizedAccessException => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Title = "Unauthorized",
                    Detail = exception.Message
                },
                _ => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while processing your request."
                }
            };

            context.Response.StatusCode = problemDetails.Status.Value;

            var json = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(json);
        }
    }
}