using Microsoft.AspNetCore.RateLimiting;

namespace WheresWaldoApi.Extensions;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddCustomRateLimiters(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        code = "RATE_LIMIT_EXCEEDED",
                        message = "Too many requests."
                    },
                    cancellationToken: token
                );
            };

            var policies = new[]
            {
                new { Name = "ImageSelection",      Limit = 30, Window = TimeSpan.FromMinutes(1) },
                new { Name = "ImageUpload",         Limit = 10, Window = TimeSpan.FromHours(1)   },
                new { Name = "LeaderboardView",     Limit = 60, Window = TimeSpan.FromMinutes(1) },
                new { Name = "CharacterSelection",  Limit = 30, Window = TimeSpan.FromMinutes(1) },
                new { Name = "CharacterAddition",   Limit = 10, Window = TimeSpan.FromHours(1)   },
                new { Name = "CompletionsView",     Limit = 60, Window = TimeSpan.FromMinutes(1) },
                new { Name = "CompletionCreation",  Limit = 30, Window = TimeSpan.FromMinutes(1) }
            };

            foreach (var policy in policies)
            {
                options.AddFixedWindowLimiter(policy.Name, config =>
                {
                    config.PermitLimit = policy.Limit;
                    config.Window = policy.Window;
                });
            }
        });

        return services;
    }
}