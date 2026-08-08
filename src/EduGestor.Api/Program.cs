using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using EduGestor.Core.Entities;
using EduGestor.Infrastructure;
using EduGestor.Infrastructure.Auth;
using EduGestor.Infrastructure.Data;
using EduGestor.Infrastructure.Services;
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

    // Identity (Spec 30)
    builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = false; // We enforce uniqueness per tenant
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // JWT Authentication (Spec 30)
    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret is not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

    // Google OAuth (only if configured)
    var googleClientId = builder.Configuration["Google:ClientId"];
    if (!string.IsNullOrEmpty(googleClientId))
    {
        builder.Services.AddAuthentication().AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? string.Empty;
        });
    }

    // Auth services (Spec 30)
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    var app = builder.Build();

    // Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();

    // Auth pipeline (Spec 30)
    app.UseAuthentication();
    app.UseMiddleware<TenantMiddleware>();     // Spec 20 — tenant resolution from JWT
    app.UseAuthorization();

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
