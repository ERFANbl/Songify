using EntityFrameworkCore.Configuration;
using Serilog;
using WebAPI.configurations;

Log.Logger = LoggingConfiguration.ConfigureSerilog();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.WebHost.UseUrls("http://127.0.0.1:4000");

builder.Services.AddControllers();

// Register the DbContext with PostgreSQL connection string from appsettings.json or environment variable.
builder.Services.AddDbContext<SongifyDbContext>();

// Add Swagger generation
builder.Services.AddEndpointsApiExplorer();  // This is needed for OpenAPI (Swagger) documentation generation
builder.Services.AddSwaggerGen();

// Add other services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  // Generates the Swagger JSON documentation
    app.UseSwaggerUI();  // Serves the Swagger UI (interactive docs page)
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
