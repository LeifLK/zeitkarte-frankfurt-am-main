using Scalar.AspNetCore;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://zeitkarteffm.de")
                .AllowAnyHeader()
                .WithMethods("GET", "OPTIONS");
        });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

dataSourceBuilder.UseNetTopologySuite();

var dataSource = dataSourceBuilder.Build();

builder.Services.AddSingleton(dataSource);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();

    app.MapGet("/", () => Results.Redirect("/scalar"));
}
else
{
    app.UseExceptionHandler("/error");
    app.MapGet("/error", () => Results.Problem("An unexpected error occurred."));
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();
