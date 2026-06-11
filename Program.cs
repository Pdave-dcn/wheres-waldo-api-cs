using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using WheresWaldoApi.Services;
using WheresWaldoApi.Data;
using WheresWaldoApi.Middleware;
using WheresWaldoApi.Validators;

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

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.MapControllers();


app.Run();
