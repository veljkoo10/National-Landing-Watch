using Enzivor.Api.Data;
using Enzivor.Api.Repositories.Implementations;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Implementations;
using Enzivor.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MVC
builder.Services.AddControllers();

builder.Services.AddScoped<LandfillProcessingService>();
builder.Services.AddScoped<CsvPredictionReader>();
builder.Services.AddScoped<CoordinateConverter>();
builder.Services.AddScoped<SurfaceCalculator>();

// Services
builder.Services.AddScoped<ILandfillSiteService, LandfillSiteService>();
builder.Services.AddScoped<ILandfillProcessingService, LandfillProcessingService>();
builder.Services.AddScoped<ICsvPredictionReader, CsvPredictionReader>();
builder.Services.AddScoped<ICoordinateConverter, CoordinateConverter>();
builder.Services.AddScoped<ISurfaceCalculator, SurfaceCalculator>();


// Repositories
builder.Services.AddScoped<ILandfillDetectionRepository, LandfillDetectionRepository>();
builder.Services.AddScoped<ILandfillSiteRepository, LandfillSiteRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
