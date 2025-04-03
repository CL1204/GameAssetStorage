using GameAssetStorage.Data;
using GameAssetStorage.Repositories;
using GameAssetStorage.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Cloudinary service (🔹 NEW)
builder.Services.AddSingleton<CloudinaryService>();

// Data Protection (store keys in /app/keys folder inside container)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("GameAssetStorage");

// Register Repositories and Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.None;
    });

builder.Services.AddAuthorization();

// Configure CORS for Netlify
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
            .AllowCredentials();
    });
});

// Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Game Asset Storage API", Version = "v1" });
});

var app = builder.Build();

// Configure error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Auto-apply migrations with error handling
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure for Render
app.Urls.Add($"http://*:{Environment.GetEnvironmentVariable("PORT") ?? "7044"}");
app.UseForwardedHeaders();
app.UseHttpsRedirection();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files
app.UseStaticFiles();

app.UseRouting();

app.UseCors("NetlifyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
Console.WriteLine("App is fully running and listening.");
