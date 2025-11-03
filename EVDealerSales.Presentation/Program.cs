using EVDealerSales.DataAccess;
using EVDealerSales.Presentation.Architecture;
using EVDealerSales.Presentation.Configuration;
using EVDealerSales.Presentation.Helper;
using EVDealerSales.Presentation.Hubs;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.SetupIocContainer();
builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables();

// Configure Stripe settings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Validate Stripe configuration
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
var stripePublishableKey = builder.Configuration["Stripe:PublishableKey"];

// Set Stripe API key
StripeConfiguration.ApiKey = stripeSecretKey;

// Set up Stripe app info
var appInfo = new AppInfo { Name = "MovieTheater", Version = "v1" };
StripeConfiguration.AppInfo = appInfo;

// Register HTTP client for Stripe
builder.Services.AddHttpClient("Stripe");

// Register the StripeClient as a service
builder.Services.AddTransient<IStripeClient, StripeClient>(s =>
{
    var clientFactory = s.GetRequiredService<IHttpClientFactory>();

    var sysHttpClient = new SystemNetHttpClient(
        clientFactory.CreateClient("Stripe"),
        StripeConfiguration.MaxNetworkRetries,
        appInfo,
        StripeConfiguration.EnableTelemetry);

    return new StripeClient(stripeSecretKey, httpClient: sysHttpClient);
});

if (string.IsNullOrEmpty(stripeSecretKey))
{
    Console.WriteLine("CRITICAL: Stripe Secret Key is missing! Payment processing will fail.");
}

// Add services to the container.
builder.Services.AddRazorPages();

// Add SignalR
builder.Services.AddSignalR();

// Add Memory Cache for chat storage
builder.Services.AddMemoryCache();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.WebHost.UseUrls("http://0.0.0.0:5000");
builder.Services.AddDistributedMemoryCache();
// Configure data protection key persistence. Keys will be written to a folder
// mounted into the container (e.g. host ./data/keys -> container /keys).
// The path can be overridden via configuration: DataProtection:KeyPath or env DATA_PROTECTION_KEY_PATH.
var dataProtectionPath = builder.Configuration["DataProtection:KeyPath"]
                        ?? Environment.GetEnvironmentVariable("DATA_PROTECTION_KEY_PATH")
                        ?? "/keys";

try
{
    if (!Directory.Exists(dataProtectionPath))
    {
        Directory.CreateDirectory(dataProtectionPath);
    }

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("EVDealerSales");
}
catch (Exception ex)
{
    // If key storage cannot be configured (e.g., missing permissions), continue with default ephemeral keys but warn in logs at runtime.
    Console.WriteLine($"Warning: could not configure persistent data protection keys at '{dataProtectionPath}': {ex.Message}");
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

try
{
    app.ApplyMigrations(app.Logger);

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EVDealerSalesDbContext>();
        await DbSeeder.SeedUsersAsync(dbContext);
        await DbSeeder.SeedVehiclesAsync(dbContext);
        await DbSeeder.SeedReportDataAsync(dbContext);
    }
}
catch (Exception e)
{
    app.Logger.LogError(e, "An problem occurred during migration!");
}

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/Home/LandingPage"));

app.MapRazorPages();

// Map SignalR Hub
app.MapHub<ChatHub>("/chatHub");
app.MapHub<ChatbotHub>("/chatbotHub");

app.Run();
