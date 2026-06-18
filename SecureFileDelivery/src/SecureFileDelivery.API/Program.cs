using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SecureFileDelivery.API.HealthChecks;
using SecureFileDelivery.API.Middleware;
using SecureFileDelivery.Application.Common.Extensions;
using SecureFileDelivery.Infrastructure.DependencyInjection;
using SecureFileDelivery.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("logs/secure-file-delivery-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Secure File Delivery API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT ****** only"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT secret key is missing.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

ValidateProductionConfiguration(builder.Configuration, builder.Environment);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("download", policy =>
    {
        policy.PermitLimit = 10;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.QueueLimit = 0;
    });

    options.AddSlidingWindowLimiter("statement-upload", policy =>
    {
        policy.PermitLimit = 5;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.SegmentsPerWindow = 2;
        policy.QueueLimit = 0;
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "database", tags: new[] { "ready" })
    .AddCheck<StorageHealthCheck>(name: "storage", tags: new[] { "ready" });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("SecureFileDelivery.API"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        if (builder.Environment.IsDevelopment())
        {
            tracing.AddConsoleExporter();
        }
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});

app.Run();

static void ValidateProductionConfiguration(IConfiguration configuration, IHostEnvironment environment)
{
    if (!environment.IsProduction())
    {
        return;
    }

    var issues = new List<string>();

    ValidateRequiredValue(configuration["Jwt:Issuer"], "Jwt:Issuer", issues);
    ValidateRequiredValue(configuration["Jwt:Audience"], "Jwt:Audience", issues);
    ValidateRequiredSecret(configuration["Jwt:SecretKey"], "Jwt:SecretKey", issues);
    ValidateRequiredValue(configuration.GetConnectionString("DefaultConnection"), "ConnectionStrings:DefaultConnection", issues);

    var storageProvider = configuration["Storage:Provider"] ?? "Local";
    if (string.Equals(storageProvider, "Minio", StringComparison.OrdinalIgnoreCase))
    {
        ValidateRequiredValue(configuration["Storage:Minio:Endpoint"], "Storage:Minio:Endpoint", issues);
        ValidateRequiredValue(configuration["Storage:Minio:BucketName"], "Storage:Minio:BucketName", issues);
        ValidateRequiredValue(configuration["Storage:Minio:AccessKey"], "Storage:Minio:AccessKey", issues);
        ValidateRequiredSecret(configuration["Storage:Minio:SecretKey"], "Storage:Minio:SecretKey", issues);
    }

    if (issues.Count > 0)
    {
        throw new InvalidOperationException($"Production configuration is incomplete: {string.Join(", ", issues)}");
    }
}

static void ValidateRequiredValue(string? value, string key, ICollection<string> issues)
{
    if (string.IsNullOrWhiteSpace(value) || value.Contains("******", StringComparison.Ordinal) || value.Contains("change-me", StringComparison.OrdinalIgnoreCase))
    {
        issues.Add(key);
    }
}

static void ValidateRequiredSecret(string? value, string key, ICollection<string> issues)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length < 32 || value.Contains("******", StringComparison.Ordinal) || value.Contains("change-me", StringComparison.OrdinalIgnoreCase))
    {
        issues.Add(key);
    }
}

public partial class Program;
