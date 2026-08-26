using Microsoft.AspNetCore.Identity;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<ICompletionService, CompletionService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IGuessService, GuessService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<PasswordHasher<User>>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
