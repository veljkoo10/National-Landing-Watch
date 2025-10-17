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


// Services
builder.Services.AddScoped<ILandfillSiteService, LandfillSiteService>();
builder.Services.AddScoped<IProductionLandfillProcessor, ProductionLandfillProcessor>();
builder.Services.AddScoped<IMethaneCalculationService, MethaneCalculationService>();

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
