using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NRLApp.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// === DATABASE (MySQL/MariaDB via Pomelo) ===
// Henter connection string fra appsettings.*.json
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[DEBUG-CONN] {cs}");

// Definerer hvilken MariaDB-versjon EF skal bruke uten å prøve å koble til databasen.
var serverVersion = ServerVersion.Create(new Version(11, 0, 0), ServerType.MariaDb);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(cs, serverVersion, mySqlOptions =>
    {
        // Forsøker automatisk på nytt ved midlertidige DB-feil
        mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
    });
});

// === IDENTITY (styrket sikkerhet) ===
// Konfigurerer Identity-systemet med litt strengere passordregler og lockout.
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(opt =>
    {
        // Passord-krav
        opt.Password.RequiredLength = 10;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireDigit = true;
        opt.Password.RequireUppercase = true;
        opt.Password.RequireLowercase = true;

        // Lockout-innstillinger (beskytter mot brute force på login)
        opt.Lockout.MaxFailedAccessAttempts = 5;                       // etter 5 feil
        opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10); // låses i 10 min
        opt.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Konfigurerer autentiseringscookie for innlogging
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/AccessDenied";
    opt.SlidingExpiration = true;

    // SIKKERHET: strammer inn hvordan cookie håndteres
    opt.Cookie.Name = "NRLApp.Auth";                     // unikt navn
    opt.Cookie.HttpOnly = true;                          // ikke lesbar fra JS
    opt.Cookie.SameSite = SameSiteMode.Lax;              // beskytter mot CSRF for de fleste scenarier
    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    // (i produksjon bak HTTPS kan dere sette .Always, men da må appen kjøre på https)
});

// === PORT-OPPSETT SOM FUNKER BÅDE LOKALT OG I DOCKER ===
// I container styres port via miljøvariabler; lokalt setter vi eksplisitt til 5099.
var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (!runningInContainer && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(5099)); // lokal port
}

// === LOGGING (enkel, tydelig i konsoll) ===
// Fjerner standard loggere og bruker en enkel konsoll-logger i stedet.
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
});

// MVC + Session
builder.Services.AddControllersWithViews();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(4);

    // SIKKERHET: session-cookie bør ikke være tilgjengelig fra JS
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});


// Setter standard kultur/globalisering til en-US for hele appen.
var defaultCulture = new CultureInfo("en-US");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = new[] { defaultCulture };
    options.SupportedUICultures = new[] { defaultCulture };
});
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var app = builder.Build();

// Feilhåndtering avhenger av om vi kjører i Development eller ikke.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Aktiverer språkstøtten vi satte opp over
app.UseRequestLocalization(app.Services
    .GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Viktig rekkefølge for autentisering/autorisasjon
app.UseAuthentication();
app.UseAuthorization();

// Ruter
// Root-URL sender bare videre til login-siden.
app.MapGet("/", () => Results.Redirect("/Account/Login"));

// Standard MVC-rute
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Egen rute for hinder controller
app.MapControllerRoute(
    name: "obstacles",
    pattern: "obstacle/{action=Area}/{id?}",
    defaults: new { controller = "Obstacle" }
);

// VENT PÅ DB -> MIGRER -> SEED ADMIN ===
// Når appen starter opp, sørger vi for at databasen finnes, er migrert og har en admin-bruker.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<ApplicationDbContext>();

    // 1) Vent (med retry) til DB svarer
    var connected = false;
    for (int attempt = 1; attempt <= 90; attempt++) // inntil 90 sek
    {
        try
        {
            if (await db.Database.CanConnectAsync())
            {
                connected = true;
                logger.LogInformation("DB er tilgjengelig (forsøk {Attempt}).", attempt);
                break;
            }
        }
        catch
        {
            // ignorer, prøv igjen
        }
        logger.LogInformation("Venter på DB (forsøk {Attempt}/90)...", attempt);
        await Task.Delay(1000);
    }

    if (!connected)
    {
        logger.LogError("Fikk ikke kontakt med DB innen tidsfristen. Sjekk compose.yaml og connection string.");
        throw new Exception("Database not reachable in time.");
    }

    // Migrer
    await db.Database.MigrateAsync();

    // 3) Seed admin hvis tomt
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    const string adminEmail = "admin@nrl.local";
    const string adminPass = "Admin!123!";
    const string adminRole = "Admin";
    const string approverRole = "Approver";
    const string pilotRole = "Pilot";
    const string crewRole = "Crew";

    // Sørger for at alle roller finnes
    foreach (var roleName in new[] { adminRole, approverRole, pilotRole, crewRole })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    // Hvis det ikke finnes noen brukere enda, lager vi en admin-konto.
    if (!userManager.Users.Any())
    {
        var admin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPass);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, adminRole);
            await userManager.AddToRoleAsync(admin, approverRole);
        }
        else
            logger.LogError("Klarte ikke å opprette admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => $"{e.Code}:{e.Description}")));
    }
}

// Logger en melding når appen er oppe og kjører
app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "http://localhost:5099";
    Console.WriteLine($"✅ Appen kjører. Åpne: {urls}/Account/Login");
});

app.Run();
