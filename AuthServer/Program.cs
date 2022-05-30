using System.Security.Claims;
using AuthServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthServer.Data;
using AuthServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Dokobit;
using Microsoft.AspNetCore.Authentication.WebEid;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(ctx.Configuration));

IdentityModelEventSource.ShowPII = true;

try
{
    ConfigureServices();

    var app = builder.Build();
    ConfigurePipeline(app);
    ApplyMigrations(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

void ConfigureServices()
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddRazorPages()
#if DEBUG
        .AddRazorRuntimeCompilation()
#endif
        ;

    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddApplicationInsightsTelemetryProcessor<HideAccessTokenTelemetryProcessor>();

    builder.Services.AddSession();
    builder.Services.AddHttpClient<WebEidValidationService>(c => { c.BaseAddress = new Uri(builder.Configuration["Infrastructure:Validator:Uri"]); });

    builder.Services.AddIdentityServer()
        .AddAspNetIdentity<IdentityUser>()
        .AddInMemoryIdentityResources(Config.IdentityResources)
        .AddInMemoryClients(Config.GetClients(builder.Configuration.GetRequiredSection("Auth:Clients").Get<IEnumerable<AppClient>>()));

    builder.Services.AddAuthentication()
        .AddDokobit("Dokobit", "Dokobit", o =>
        {
            o.SignInScheme = IdentityConstants.ExternalScheme;

            o.ApiKey = builder.Configuration["Auth:Providers:Dokobit:ApiKey"];
            o.Environment = DokobitEnvironment.Production;

            o.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name", ClaimValueTypes.String);
            o.ClaimActions.MapJsonKey(ClaimTypes.Surname, "surname", ClaimValueTypes.String);
        })
        .AddDokobit("DokobitTest", "Dokobit TEST", o =>
        {
            o.SignInScheme = IdentityConstants.ExternalScheme;

            o.ApiKey = builder.Configuration["Auth:Providers:DokobitTest:ApiKey"];
            o.Environment = DokobitEnvironment.Testing;

            o.CallbackPath = new PathString("/signin-dokobit-test");

            o.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name", ClaimValueTypes.String);
            o.ClaimActions.MapJsonKey(ClaimTypes.Surname, "surname", ClaimValueTypes.String);
        }).AddOpenIdConnect("eeID", "eeID TEST", o =>
        {
            o.SignInScheme = IdentityConstants.ExternalScheme;

            o.CallbackPath = new PathString("/signin-tara");
            o.MetadataAddress = builder.Configuration["Auth:Providers:eeID:Metadata"];

            o.ClientId = builder.Configuration["Auth:Providers:eeID:ClientId"];
            o.ClientSecret = builder.Configuration["Auth:Providers:eeID:ClientSecret"];

            o.ResponseType = OpenIdConnectResponseType.Code;
            o.UsePkce = false;

            o.Scope.Clear();
            o.Scope.Add(OidcConstants.StandardScopes.OpenId);
        }).AddWebEid(o => { o.SignInScheme = IdentityConstants.ExternalScheme; });

    builder.Services.AddHostedService<TimedDatabasePruneService>();
}

void ConfigurePipeline(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseSession();

    app.UseIdentityServer();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
}

void ApplyMigrations(IHost app)
{
    using var scope = app.Services.CreateScope();
    using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    dbContext.Database.EnsureCreated();
}