using System.Text.Json;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Middleware;

public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger _logger;

  public ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
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
    catch(DomainException ex)
    {
      _logger.LogError(
        ex,
        "Domain exception occurred"
      );

      

      context.Response.StatusCode = ex.StatusCode;
      context.Response.ContentType = "application/json";

      var response = new
      {
        code = ex.Code,
        message = ex.Message
      };

      await context.Response.WriteAsync(
        JsonSerializer.Serialize(response)
      );
    }
    catch(Exception ex)
    {
      _logger.LogError(
        ex,
        "Unhandled exception"
      );

      context.Response.StatusCode = 500;
      context.Response.ContentType = "application/json";

      var response = new
      {
        code = "INTERNAL_SERVER_ERROR",
        message = "An unexpected error occurred."
      };

      await context.Response.WriteAsync(
        JsonSerializer.Serialize(response)
      );
    }
  }
}