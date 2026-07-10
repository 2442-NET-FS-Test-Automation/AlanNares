using Microsoft.EntityFrameworkCore;
using LegendaryItemsEngine.Data;
using Serilog;
using LegendaryItemsEngine.Data.Repositories;
using LegendaryItemsEngine.Data.Services;
using LegendaryItemsEngine.Data.Seeding;

//======================================================

var builder = WebApplication.CreateBuilder(args);

//Configurar Serilog para que lea el appsettings.json de la app
//Here we create a LoggerConfiguration for Serilog who reads appsettings.json
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() 
    .WriteTo.File("logs/fulfillment-engine-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Le decimos al host que limpie los proveedores nativos y use Serilog
builder.Host.UseSerilog();


//Here we get our connection "String" from our Json "appsettings.json"
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Connection with SQL server
builder.Services.AddDbContext<LegendaryItemsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IOrderFulfillmentService, OrderFulfillmentService>();

//Here we call our endpoints from: LegendaryItemsEngine.Api.Controllers in
//OrdersController.cs and FulfillmentController.cs
builder.Services.AddControllers();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Legendary Items Engine API V1");
});

using (var scope = app.Services.CreateScope())
{
    //We create a temporal Scope who check if we have data in our tables Item and ItemInventory
    //If we don't have data in our tables we add 10 items with stock of 100 each
    //Once this method its executed we discard this scope
    DbInitializer.SeedData(scope.ServiceProvider);
}

//================================================
Log.Information("The Legendary Items Engine API has booted up successfully.");

app.MapGet("/", () => "Legendary Items Engine API Running!");

app.MapControllers();

app.Run();
