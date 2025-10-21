using Enzivor.Api.Data;
using Enzivor.Api.Mapping;
using Enzivor.Api.Repositories.Implementations;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Implementations;
using Enzivor.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Encoding Configuration
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// CORS Configuration
const string LocalAngular = "LocalAngular";
builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalAngular, policy =>
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
    // .AllowCredentials() // enable only if needed
    );
});

builder.Services.AddControllers();

// Service Registrations
builder.Services.AddScoped<ILandfillSiteService, LandfillSiteService>();
builder.Services.AddScoped<ILandfillQueryService, LandfillQueryService>();
builder.Services.AddScoped<ILandfillDetectionService, LandfillDetectionService>();
builder.Services.AddScoped<IProductionLandfillProcessor, ProductionLandfillProcessor>();
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Repository Registrations
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
app.UseCors(LocalAngular);
app.MapControllers();

app.Run();