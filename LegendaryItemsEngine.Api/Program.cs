using Microsoft.EntityFrameworkCore;
using LegendaryItemsEngine.Data;
using Serilog;

//======================================================

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 2. Configura Serilog para que lea el appsettings.json de la app
//Here we create a LoggerConfiguration for Serilog who reads appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Le decimos al host que limpie los proveedores nativos y use Serilog
builder.Host.UseSerilog();

//Here we get our connection "String" from our Json "appsettings.json"
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Connection with SQL server
builder.Services.AddDbContext<LegendaryItemsDbContext>(options =>
    options.UseSqlServer(connectionString));

app.MapGet("/", () => "Hello World!");

app.Run();
