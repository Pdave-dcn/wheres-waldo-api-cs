using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;

using WheresWaldoApi.Services;
using WheresWaldoApi.Data;
using WheresWaldoApi.Middleware;
using WheresWaldoApi.Validators;
using WheresWaldoApi.Extensions;
using WheresWaldoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateImageDtoValidator>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICompletionService, CompletionService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IGuessService, GuessService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
            builder.Configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>()!
        );

        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

builder.Services.AddCustomRateLimiters();

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
