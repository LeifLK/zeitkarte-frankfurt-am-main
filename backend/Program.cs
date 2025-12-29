using Scalar.AspNetCore;
using RmvApiBackend.Settings;
using RmvApiBackend.Services;
using System;
using backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add services to the container.
// Learn more about configuring Open API at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<RmvApiSettings>(
    builder.Configuration.GetSection("RmvApiSettings")
);

// Register the HttpClientFactory with a "named" client for the RMV API.
// This sets the BaseAddress so we don't have to type it every time.
builder.Services.AddHttpClient("RMV", client =>
{
    client.BaseAddress = new Uri("https://www.rmv.de/hapi/");
    // You can set other defaults here, like headers
});

// Register our custom service.
// We tell the app: "When a class asks for IRmvService, give it an RmvService."
// AddScoped means a new RmvService is created for each web request.
builder.Services.AddScoped<IRmvService, RmvService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();

    // Automatically redirect to Scalar documentation
    app.MapGet("/", () => Results.Redirect("/scalar"));
}

app.UseHttpsRedirection();

app.MapControllers();

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast = Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
