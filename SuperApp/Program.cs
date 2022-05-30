using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true;

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddCookie()
    .AddOpenIdConnect(o =>
    {
        o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        o.ClientId = "super-app";
        o.ClientSecret = "v3rY-s3cr3t!";

        o.MetadataAddress = "https://auth.eid.gedas.dev/.well-known/openid-configuration";

        o.ResponseType = OpenIdConnectResponseType.Code;
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("eID", p => p.RequireClaim("http://schemas.microsoft.com/identity/claims/identityprovider", "Web-eID", "Dokobit", "eeID"));
});

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();