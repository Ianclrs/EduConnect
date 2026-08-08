using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using EduGestor.Infrastructure;
using EduGestor.Infrastructure.Data;
using EduGestor.Infrastructure.Tenancy;
using EduGestor.Api.Middleware;
using EduGestor.Infrastructure.Data.Seeders;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

    // Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Multi-tenancy (Spec 20)
    builder.Services.AddScoped<TenantContext>();
    builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

    var app = builder.Build();

    // Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();

    // Auth pipeline (Spec 30 will add UseAuthentication/UseAuthorization)
    // app.UseAuthentication();               // Spec 30
    app.UseMiddleware<TenantMiddleware>();     // Spec 20 — tenant resolution from JWT
    // app.UseAuthorization();                // Spec 30

    app.MapControllers();

    // Auto-migrate database and seed in Development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            await db.Database.MigrateAsync();
            await TenantSeeder.SeedAsync(db, app.Environment,
                scope.ServiceProvider.GetRequiredService<ILogger<Program>>());
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database migration failed — database may not be available");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
