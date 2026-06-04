using API;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

// Dependencies
var connectionString = builder.Configuration.GetConnectionString("LocalConnectionString");
DependencyContainer.RegisterDependencies(builder.Services, connectionString);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.UseSerilogRequestLogging();

app.MapGet("/", () => "Running...");

app.Run();