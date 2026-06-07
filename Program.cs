using System.Text.Json.Serialization;
using WheresWaldoApi.Services;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();


app.Run();
