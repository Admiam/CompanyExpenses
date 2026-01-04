using CompanyExpenses.Database.Data;
using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection - MUSÍ být stejné jako v Auth serveru!
var keysPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "shared-keys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("CompanyExpenses"); // DŮLEŽITÉ: stejný název jako Auth server

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI konfigurace
builder.Services.AddSwaggerGen();

// Configure Entity Framework with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Auth Database Context for roles
var authConnectionString = builder.Configuration.GetConnectionString("AuthConnection")
    ?? throw new InvalidOperationException("Connection string 'AuthConnection' not found.");

builder.Services.AddDbContext<CompanyExpenses.Api.Data.AuthDbContext>(options =>
    options.UseSqlServer(authConnectionString));

// Register HttpClient
builder.Services.AddHttpClient();

// Configure Authentication
// API server bude sdílet cookie authentication s Auth serverem
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         options.Cookie.Name = ".AspNetCore.Identity.Application";
//         options.Cookie.Domain = "localhost"; // Stejná doména jako Auth server
//         options.Cookie.Path = "/";
//         options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS required
//         options.Cookie.SameSite = SameSiteMode.None; // Povolí cross-origin cookies
//         options.Cookie.HttpOnly = true; // Zabránit XSS útokům
//         options.LoginPath = "/Account/Login";
//         options.LogoutPath = "/Account/Logout";
//         options.Events = new CookieAuthenticationEvents
//         {
//             OnRedirectToLogin = context =>
//             {
//                 // Pro API vrátit 401 místo redirectu
//                 context.Response.StatusCode = 401;
//                 return Task.CompletedTask;
//             }
//         };
//     });

// builder.Services.AddAuthorization();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = ".AspNetCore.Identity.Application";
        // Doporučení: pro localhost často Domain vůbec nedávat
        // options.Cookie.Domain = "localhost";
        options.Cookie.Path = "/";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.HttpOnly = true;

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = 401;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = 403;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure CORS for frontend app
// ⚠️ DŮLEŽITÉ: AllowAnyOrigin() NEFUNGUJE s AllowCredentials()!
// Proto musíme specifikovat konkrétní origins i v development módu
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:5173",
                "http://localhost:5173",
                "https://localhost:5174",
                "http://localhost:5174"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Potřebné pro cookies a authentication
    });
});

// Register services
builder.Services.AddScoped<CompanyExpenses.Api.Services.IEmailService, CompanyExpenses.Api.Services.EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Swagger UI - interaktivní dokumentace API
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Company Expenses API v1");
        options.RoutePrefix = "swagger"; // Swagger UI na: https://localhost:7200/swagger
        options.DocumentTitle = "Company Expenses API";
        options.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
