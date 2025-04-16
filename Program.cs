using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Smart_ERP.Data;
using Smart_ERP.Modules.Auth.Models;
using Smart_ERP.Modules.Auth.Services;
using DotNetEnv;

Env.Load();
Env.Load(".env");

var builder = WebApplication.CreateBuilder(args);

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;
var jwtSettings = config.GetSection("JwtSettings");
//MYSQL_PASSWORD to get the password from the env var
var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? string.Empty;
var rawConnStr = config.GetConnectionString("DefaultConnection");
var connStr = rawConnStr?.Replace("__PASSWORD__", password);

// Register DbContext
builder.Services.AddDbContext<ERPDbContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr))
);

// JWT Auth
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? jwtSettings["Key"];
// Debug: print to console (just once for dev testing)
Console.WriteLine($"JWT Key loaded? {jwtKey.Length} characters");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("JWT_KEY is missing!");

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<JwtService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//Seed Roles
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
    db.Database.Migrate();

    if (!db.Roles.Any())
    {
        db.Roles.AddRange(new Role { Name = "Admin" }, new Role { Name = "User" });
        db.SaveChanges();
    }
}

app.Run();
