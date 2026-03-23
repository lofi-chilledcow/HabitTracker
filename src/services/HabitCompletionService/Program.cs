using DotNetEnv;
using HabitCompletionService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Resolve environment variable tokens
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!
    .Replace("#{HABITTRACKER_DB_PASSWORD}#", Environment.GetEnvironmentVariable("HABITTRACKER_DB_PASSWORD") ?? string.Empty);

builder.Configuration["Jwt:Key"] = builder.Configuration["Jwt:Key"]!
    .Replace("#{HABITTRACKER_JWT_SECRET}#", Environment.GetEnvironmentVariable("HABITTRACKER_JWT_SECRET") ?? string.Empty);

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
