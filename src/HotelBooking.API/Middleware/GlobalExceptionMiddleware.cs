using System.Data;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Domain.Exceptions;

namespace HotelBooking.API.Middleware;

// Tangkap semua exception domain dan ubah jadi HTTP response yang proper
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            ValidationException    => (422, "Validation error"),
            NotFoundException      => (404, "Not found"),
            RoomUnavailableException => (409, "Room unavailable"),
            DBConcurrencyException     => (409, "Conflict"),
            _                      => (500, "Internal server error")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            title,
            status = statusCode,
            detail = ex.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}