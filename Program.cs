
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Starter_App.src.backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build connection string from environment variables (Docker) or appsettings.json (local dev)
string connectionString;
var dbConnection = builder.Configuration["DB_CONNECTION"];

// If DB_CONNECTION environment variable is set (Docker), use it directly
if (!string.IsNullOrEmpty(dbConnection))
{
    connectionString = dbConnection;
}
else
{
    // Fall back to appsettings.json connection string for local development
    connectionString = builder.Configuration.GetConnectionString("Connection");
}

builder.Services.AddDbContext<AppStarterContext>(options => options.UseSqlServer(connectionString));

// Add Identity services with proper configuration
builder.Services.AddIdentity<AspNetUser, AspNetRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppStarterContext>()
.AddDefaultTokenProviders();

// Configure Authentication for SAML integration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JWT";
    options.DefaultSignInScheme = "JWT";
    options.DefaultChallengeScheme = "SAML";
})
.AddCookie("SAML", options =>
{
    options.LoginPath = "/Authentication/loginWithSaml";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer("JWT", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "")),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.Name
    };
    // Configure JWT to read from cookies
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("jwt"))
            {
                context.Token = context.Request.Cookies["jwt"];
            }
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Starter App API V1");
        c.SwaggerEndpoint("/manageruser/swagger/v1/swagger.json", "Starter App API V1");
        c.RoutePrefix = "swagger";
    
    });
}

// Only use HTTPS redirect when not in Docker (Docker uses HTTP)
if (builder.Configuration["DOTNET_RUNNING_IN_CONTAINER"] == null)
{
    app.UseHttpsRedirection();
}

// Add CORS middleware
app.UseCors("AllowFrontend");

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    var role = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
    var isApiPath = path?.StartsWith("/authentication/") == true;
    var isStaticFile = path?.Contains(".") == true && !path.EndsWith(".html");
    var isSwagger = path?.StartsWith("/swagger") == true;
    
    if (!isApiPath && !isStaticFile && !isSwagger)
{
    // Try to authenticate with both SAML and JWT schemes
    var samlAuth = await context.AuthenticateAsync("SAML");
    var jwtAuth = await context.AuthenticateAsync("JWT");
    
    var isAuthenticated = samlAuth.Succeeded || jwtAuth.Succeeded;
    
    if (!isAuthenticated)
    {
        context.Response.Redirect("/Authentication/loginWithSaml");
        return;
    }
    else
    {
        var principal = samlAuth.Succeeded ? samlAuth.Principal : jwtAuth.Principal;
        var userName = principal?.Identity?.Name;
        
        // Check if user is a temporary user (not in database)
        var isTempUser = principal?.Claims?.Any(c => c.Type == "IsTempUser" && c.Value == "true") ?? false;
        
        // Apply restrictions for temp users
        if (isTempUser)
        {
            // Temp users can only access home (/) and user page (/user)
            var allowedPaths = new[] { "/", "/user" };
            var isAllowed = allowedPaths.Any(allowed => path == allowed);
            
            if (!isAllowed)
            {
                context.Response.Redirect("/?access=denied");
                return;
            }
        }
    }
}
    
    await next();
});

app.UseStaticFiles();

// Map API controllers first
app.MapControllers();

// Fallback to Vue.js frontend for all other routes
app.MapFallbackToFile("index.html");

// Display startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Application started successfully!");
logger.LogInformation("🌐 Access the application at: https://localhost:5001");
logger.LogInformation("📊 Swagger UI available at: https://localhost:5001/swagger");

app.Run();