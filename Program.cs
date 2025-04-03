using GameAssetStorage.Data;
using GameAssetStorage.Repositories;
using GameAssetStorage.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Linq;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("GameAssetStorage")));

// Enhanced Data Protection with fallback
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("GameAssetStorage")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
    .AddKeyManagementOptions(options =>
    {
        options.NewKeyLifetime = TimeSpan.FromDays(14);
        options.AutoGenerateKeys = true;
    });

// Register Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Authentication with enhanced security
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

// Add Controllers with route constraints
builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

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

// Error Handling with production logging
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

// Apply Migrations with retry policy
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    int retries = 0;
    while (retries < 5)
    {
        try
        {
            if (db.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying migrations...");
                db.Database.Migrate();
                logger.LogInformation("Migrations applied successfully");
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
    }
}

// Render Configuration with health check
app.Urls.Add($"http://*:{Environment.GetEnvironmentVariable("PORT") ?? "10000"}");
app.UseForwardedHeaders();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Game Asset Storage API V1");
        c.RoutePrefix = "swagger";
    });
}

// Static files with cache control
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000";
    }
});

// Ensure wwwroot exists
try
{
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "wwwroot"));
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to create wwwroot directory");
}

app.UseRouting();
app.UseCors("NetlifyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();