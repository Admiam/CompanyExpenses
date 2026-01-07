using company_expenses_auth.Components;
using company_expenses_auth.Components.Account;
using company_expenses_auth.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using DotNetEnv;

// Load .env file (for local development)
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection - shared keys for authentication between API and Auth server
// In Docker: /app/shared-keys (volume mount)
// In development: ../shared-keys (relative path)
var keysPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_PATH")
    ?? builder.Configuration["DataProtection:KeysPath"]
    ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "shared-keys");

// Check if we're in Docker (path starts with /app)
if (Directory.Exists("/app/shared-keys"))
{
    keysPath = "/app/shared-keys";
}

Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("CompanyExpenses"); // MUSÍ být stejný jako API server

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    });

// Add Google authentication only if credentials are configured
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}

builder.Services.AddAuthentication().AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Konfigurace cookie pro sdílení mezi Auth serverem a API
var cookieDomain = builder.Configuration["CookieSettings:Domain"] ?? "localhost";
var cookieExpireHours = builder.Configuration.GetValue<int>("CookieSettings:ExpireTimeSpanHours", 24);
var slidingExpiration = builder.Configuration.GetValue<bool>("CookieSettings:SlidingExpiration", true);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.Cookie.Domain = cookieDomain; // Loaded from configuration
    options.Cookie.Path = "/";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None; // Povolí cross-origin
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(cookieExpireHours);
    options.SlidingExpiration = slidingExpiration;
});

// Use SmtpEmailSender for production, IdentityNoOpEmailSender for development without email
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, SmtpEmailSender>();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseAntiforgery();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Apply migrations and seed roles/admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Apply pending migrations
    await context.Database.MigrateAsync();

    // Seed roles
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DbInitializer.SeedRoles(roleManager);

    // Seed admin user (for Docker/first-time setup)
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    await DbInitializer.SeedAdminUser(userManager, logger);
}

app.Run();
