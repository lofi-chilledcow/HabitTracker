using DotNetEnv;
using HabitCompletionService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Resolve environment variable tokens
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!
    .Replace("#{HABITTRACKER_DB_PASSWORD}#", Environment.GetEnvironmentVariable("HABITTRACKER_DB_PASSWORD") ?? string.Empty);

builder.Configuration["Jwt:Key"] = builder.Configuration["Jwt:Key"]!
    .Replace("#{HABITTRACKER_JWT_SECRET}#", Environment.GetEnvironmentVariable("HABITTRACKER_JWT_SECRET") ?? string.Empty);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("ServiceName", "HabitCompletionService")
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/app-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}");

    if (ctx.HostingEnvironment.IsDevelopment())
        lc.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token from /api/auth/login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<HabitCompletionDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var url = app.Urls.FirstOrDefault() ?? "http://localhost:5225";
        app.Logger.LogInformation("Swagger UI: {Url}/swagger", url);
    });
}

app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diag, ctx) =>
    {
        diag.Set("RequestHost", ctx.Request.Host.Value);
        diag.Set("UserAgent", ctx.Request.Headers.UserAgent.ToString());
    };
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
