using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Exceptions;

namespace Sentinel.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (BusinessException ex)
        {
            _logger.LogWarning("İş kuralı hatası: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Bad Request", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Yetkilendirme hatası: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage);
            _logger.LogWarning("Validasyon hatası: {Errors}", string.Join(", ", errors));
            await WriteProblemAsync(context, StatusCodes.Status422UnprocessableEntity, "Validation Error", string.Join(", ", errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Server Error", "Beklenmeyen bir hata oluştu");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}