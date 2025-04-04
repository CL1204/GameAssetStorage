﻿using GameAssetStorage.Data;
using GameAssetStorage.Repositories;
using GameAssetStorage.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
<<<<<<< HEAD
using System.IO;
=======
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.IO;
using System.Linq;
using System.Threading;
>>>>>>> main

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
<<<<<<< HEAD
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Cloudinary service (🔹 NEW)
builder.Services.AddSingleton<CloudinaryService>();

// Data Protection (store keys in /app/keys folder inside container)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("GameAssetStorage");
=======
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("GameAssetStorage")));

// Enhanced Data Protection with production-ready configuration
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("GameAssetStorage")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
    .AddKeyManagementOptions(options =>
    {
        options.AutoGenerateKeys = true;
        options.NewKeyLifetime = TimeSpan.FromDays(14);
    });
>>>>>>> main

// Register Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Authentication with production security
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.Cookie.SameSite = SameSiteMode.None;
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/access-denied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("IsAdmin", "true"));
});

// CORS with production security
builder.Services.AddCors(options =>
{
    options.AddPolicy("NetlifyCors", policy =>
    {
        policy.WithOrigins(
                "https://gameassetstorage.netlify.app",
                "http://localhost:3000",
                "https://gameasset-backend-aj1g.onrender.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Set-Cookie");
    });
});

// Add Controllers
builder.Services.AddControllers();

// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Game Asset Storage API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

var app = builder.Build();

// Production error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        await next();
    });
}

// Database migration with retry logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    int retries = 0;
    while (retries < 5)
    {
<<<<<<< HEAD
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
=======
        try
        {
            if (db.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying migrations...");
                db.Database.Migrate();
                logger.LogInformation("Migrations applied successfully");
            }

            // Ensure DataProtectionKeys table exists
            if (!db.Database.GetAppliedMigrations().Any(x => x.Contains("DataProtection")))
            {
                logger.LogInformation("Creating DataProtectionKeys table...");
                db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS ""DataProtectionKeys"" (
                        ""Id"" integer GENERATED BY DEFAULT AS IDENTITY,
                        ""FriendlyName"" text NULL,
                        ""Xml"" text NULL,
                        CONSTRAINT ""PK_DataProtectionKeys"" PRIMARY KEY (""Id"")
                    )");
            }
            break;
        }
        catch (Exception ex)
        {
            retries++;
            logger.LogError(ex, $"Migration attempt {retries} failed");
            if (retries >= 5) throw;
            Thread.Sleep(2000 * retries);
        }
>>>>>>> main
    }
}

// Render.com specific configuration
app.Urls.Add($"http://*:{Environment.GetEnvironmentVariable("PORT") ?? "10000"}");
app.UseForwardedHeaders();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
}));

// Development configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Game Asset Storage API V1");
        c.RoutePrefix = "swagger";
    });
}

// Static files configuration
var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

<<<<<<< HEAD
// Serve static files
app.UseStaticFiles();
=======
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(wwwrootPath),
    RequestPath = "",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000";
    }
});
>>>>>>> main

// Middleware pipeline
app.UseRouting();
app.UseCors("NetlifyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
Console.WriteLine("App is fully running and listening.");
